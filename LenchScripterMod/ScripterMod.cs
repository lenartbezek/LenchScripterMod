using System;
using System.IO;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;
using LenchScripter.Internal;

namespace LenchScripter
{
    /// <summary>
    /// Mod class loaded by the Mod Loader.
    /// </summary>
    public class ScripterMod : Mod
    {
        public override string Name { get; } = "Lench Scripter Mod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "Lench";
        public override Version Version { get; } = new Version(2, 0, 0);
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.27";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;

        internal static Type blockScript;

        /// <summary>
        /// Is LenchScripterMod loaded.
        /// </summary>
        public static bool Loaded { get { return Scripter.Instance != null; } }

        /// <summary>
        /// Instantiates the mod and it's components.
        /// Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Scripter.Instance);
            Game.OnSimulationToggle += Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced += (Transform block) => BlockHandlers.rebuildDict = true;
            Game.OnBlockRemoved += () => BlockHandlers.rebuildDict = true;
            

            LoadBlockLoaderAssembly();
            if (LoadPythonAssembly())
            {
                PythonEnvironment.InitializeEngine();
                Debug.Log("[LenchScripterMod]: Python assemblies loaded. Script engine available.");

                XmlSaver.OnSave += Internal.MachineData.Save;
                XmlLoader.OnLoad += Internal.MachineData.Load;

                Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
                Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
                Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

                Commands.RegisterCommand("python", Scripter.Instance.InteractiveCommand, "Executes Python expression.");

                SettingsMenu.RegisterSettingsButton("SCRIPT", Scripter.Instance.RunScriptSettingToggle, true, 12);

                Internal.Configuration.Load();
            }
            else
            {
                Debug.Log("[LenchScripterMod]: Running in API only mode. Script engine unavailable.");
            }
        }

        /// <summary>
        /// Disables the mod from executing scripts.
        /// Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {

            Game.OnSimulationToggle -= Scripter.Instance.OnSimulationToggle;
            Scripter.Instance.OnSimulationToggle(false);
            Game.OnBlockPlaced -= (Transform block) => BlockHandlers.rebuildDict = true;
            Game.OnBlockRemoved -= () => BlockHandlers.rebuildDict = true;

            if (PythonEnvironment.Loaded)
            {
                XmlSaver.OnSave -= Internal.MachineData.Save;
                XmlLoader.OnLoad -= Internal.MachineData.Load;
                Internal.Configuration.Save();
            }

            UnityEngine.Object.Destroy(Scripter.Instance);
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
                PythonEnvironment.microsoftScriptingAssembly = Assembly.LoadFrom(Application.dataPath + "/Mods/Resources/LenchScripter/lib/Microsoft.Scripting.dll");
                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
