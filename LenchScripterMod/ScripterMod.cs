using System;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;

namespace Lench.Scripter
{
    /// <summary>
    /// Mod class loaded by the Mod Loader.
    /// </summary>
    public class ScripterMod : Mod
    {

#pragma warning disable CS1591
        public override string Name { get; } = "LenchScripterMod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "Lench";
        public override Version Version
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return new Version(v.Major, v.Minor, v.Build);
            }
        }
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.3";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;
#pragma warning restore CS1591

        /// <summary>
        /// Is LenchScripterMod API loaded.
        /// </summary>
        public static bool LoadedAPI { get; private set; } = false;

        /// <summary>
        /// Is LenchScripterMod full scripter loaded.
        /// </summary>
        public static bool LoadedScripter { get; private set; } = false;

        /// <summary>
        /// Loads mod's scripting features.
        /// </summary>
        public static void LoadScripter()
        {
            if (LoadedScripter)
                throw new InvalidOperationException("Mod's scripting features already loaded.");
            LoadedScripter = true;

            PythonEnvironment.InitializeEngine();
            Internal.Scripter.Instance.python = new PythonEnvironment();

            Debug.Log("[LenchScripterMod]: Python assemblies loaded. Script engine ready.");

            XmlSaver.OnSave += Internal.MachineData.Save;
            XmlLoader.OnLoad += Internal.MachineData.Load;

            Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
            Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

            Commands.RegisterCommand("lsm", Internal.Scripter.Instance.ConfigurationCommand, "Enter 'lsm' for all available commands.");
            Commands.RegisterCommand("py", Internal.Scripter.Instance.PythonCommand, "Executes Python expression.");
            Commands.RegisterCommand("python", Internal.Scripter.Instance.PythonCommand, "Executes Python expression.");

            SettingsMenu.RegisterSettingsButton("SCRIPT", Internal.Scripter.Instance.RunScriptSettingToggle, true, 12);

            Internal.Configuration.Load();
        }

        /// <summary>
        /// Automatic update checker.
        /// </summary>
        public static bool UpdateCheckerEnabled
        {
            get
            {
                return Internal.Scripter.Instance.ModUpdaterEnabled;
            }
            set
            {
                Internal.Scripter.Instance.ModUpdaterEnabled = value;
            }
        }

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
                LoadScripter();
            }

            LoadedAPI = true;
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

            LoadedScripter = false;
            LoadedAPI = false;
        }

        /// <summary>
        /// Attempts to load TGYD's BlockLoader assembly.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        public static bool LoadBlockLoaderAssembly()
        {
            Assembly assembly;
            try
            {
                assembly = Assembly.LoadFrom(Application.dataPath + "/Mods/BlockLoader.dll");
                Blocks.Block._blockScriptType = assembly.GetType("BlockScript");
                return Blocks.Block._blockScriptType != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Loads Python assemblies.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        public static bool LoadPythonAssembly()
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
