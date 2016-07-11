using System;
using System.IO;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;
using Lench.Scripter.Internal;

namespace Lench.Scripter
{
    /// <summary>
    /// Mod class loaded by the Mod Loader.
    /// </summary>
    public class ScripterMod : Mod
    {
        public override string Name { get; } = "LenchScripterMod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "Lench";
        public override Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.3";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;

        internal static Type blockScript;

        /// <summary>
        /// Is LenchScripterMod loaded.
        /// </summary>
        public static bool Loaded { get { return Internal.Scripter.Instance != null; } }

        /// <summary>
        /// Instantiates the mod and it's components.
        /// Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Internal.Scripter.Instance);
            Game.OnSimulationToggle += Internal.Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced += (Transform block) => Internal.Scripter.Instance.rebuildIDs = true;
            Game.OnBlockRemoved += () => Internal.Scripter.Instance.rebuildIDs = true;

            LoadBlockLoaderAssembly();

            if (LoadPythonAssembly())
            {
                PythonEnvironment.InitializeEngine();
                Debug.Log("[LenchScripterMod]: Python assemblies loaded. Script engine ready.");

                XmlSaver.OnSave += Internal.MachineData.Save;
                XmlLoader.OnLoad += Internal.MachineData.Load;

                Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
                Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
                Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

                Commands.RegisterCommand("python", Internal.Scripter.Instance.InteractiveCommand, "Executes Python expression.");

                SettingsMenu.RegisterSettingsButton("SCRIPT", Internal.Scripter.Instance.RunScriptSettingToggle, true, 12);

                Internal.Configuration.Load();

                Internal.Scripter.Instance.gameObject.AddComponent<Updater>();
            }
        }

        /// <summary>
        /// Disables the mod from executing scripts.
        /// Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {

            Game.OnSimulationToggle -= Internal.Scripter.Instance.OnSimulationToggle;
            Internal.Scripter.Instance.OnSimulationToggle(false);
            Game.OnBlockPlaced -= (Transform block) => Internal.Scripter.Instance.rebuildIDs = true;
            Game.OnBlockRemoved -= () => Internal.Scripter.Instance.rebuildIDs = true;

            if (PythonEnvironment.Loaded)
            {
                XmlSaver.OnSave -= Internal.MachineData.Save;
                XmlLoader.OnLoad -= Internal.MachineData.Load;
                Internal.Configuration.Save();
            }

            UnityEngine.Object.Destroy(Internal.Scripter.Instance);
            UnityEngine.Object.Destroy(GameObject.Find("Lench Scripter").transform.gameObject);
        }

        /// <summary>
        /// Attempts to load TGYD's BlockLoader assembly.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        private bool LoadBlockLoaderAssembly()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(Application.dataPath + "/Mods/BlockLoader.dll");
                blockScript = assembly.GetType("BlockScript");
                return blockScript != null;
            }
            catch (FileNotFoundException)
            {
                return false;
            }
        }

        /// <summary>
        /// Loads Python assemblies into a new domain.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        private bool LoadPythonAssembly()
        {
            try
            {
                PythonEnvironment.ironPythonAssembly = Assembly.LoadFrom(Application.dataPath + "/Mods/Resources/LenchScripter/lib/IronPython.dll");
                Assembly.LoadFrom(Application.dataPath + "/Mods/Resources/LenchScripter/lib/IronPython.Modules.dll");

                PythonEnvironment.microsoftScriptingAssembly = Assembly.LoadFrom(Application.dataPath + "/Mods/Resources/LenchScripter/lib/Microsoft.Scripting.dll");
                Assembly.LoadFrom(Application.dataPath + "/Mods/Resources/LenchScripter/lib/Microsoft.Scripting.Core.dll");
                Assembly.LoadFrom(Application.dataPath + "/Mods/Resources/LenchScripter/lib/Microsoft.Dynamic.dll");
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
