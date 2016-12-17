using System;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;
using Configuration = Lench.Scripter.Internal.Configuration;
using MachineData = Lench.Scripter.Internal.MachineData;
using Object = UnityEngine.Object;

namespace Lench.Scripter
{
    /// <summary>
    ///     Mod class loaded by the Mod Loader.
    /// </summary>
    public class ScripterMod : Mod
    {
        /// <summary>
        ///     Is LenchScripterMod API loaded.
        /// </summary>
        public static bool LoadedAPI { get; private set; }

        /// <summary>
        ///     Is LenchScripterMod full scripter loaded.
        /// </summary>
        public static bool LoadedScripter { get; private set; }

        /// <summary>
        ///     Automatic update checker.
        /// </summary>
        public static bool UpdateCheckerEnabled
        {
            get { return Internal.Scripter.Instance.ModUpdaterEnabled; }
            set { Internal.Scripter.Instance.ModUpdaterEnabled = value; }
        }

        /// <summary>
        ///     Loads mod's scripting features.
        /// </summary>
        public static bool LoadScripter()
        {
            if (PythonEnvironment.LoadPythonAssembly())
            {
                PythonEnvironment.InitializeEngine();
                PythonEnvironment.ScripterEnvironment = new PythonEnvironment();

                ModConsole.AddMessage(LogType.Log,
                    "[LenchScripterMod]: " + PythonEnvironment.ScripterEnvironment.Execute("sys.version"));
                LoadedScripter = true;
                return true;
            }
            LoadedScripter = false;
            return false;
        }

        /// <summary>
        ///     Instantiates the mod and it's components.
        ///     Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            Object.DontDestroyOnLoad(Internal.Scripter.Instance);
            Game.OnSimulationToggle += Internal.Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced += block => Internal.Scripter.Instance.RebuildIDs = true;
            Game.OnBlockRemoved += () => Internal.Scripter.Instance.RebuildIDs = true;

            XmlSaver.OnSave += MachineData.Save;
            XmlLoader.OnLoad += MachineData.Load;

            Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
            Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

            Commands.RegisterCommand("lsm", Internal.Scripter.Instance.ConfigurationCommand,
                "Scripter Mod configuration command.");
            Commands.RegisterCommand("py", Internal.Scripter.Instance.PythonCommand, "Executes Python expression.");
            Commands.RegisterCommand("python", Internal.Scripter.Instance.PythonCommand, "Executes Python expression.");

            SettingsMenu.RegisterSettingsButton("SCRIPT", Internal.Scripter.Instance.RunScriptSettingToggle, true, 12);

            Configuration.Load();

            LoadedAPI = true;
        }

        /// <summary>
        ///     Disables the mod from executing scripts.
        ///     Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= Internal.Scripter.Instance.OnSimulationToggle;
            Internal.Scripter.Instance.OnSimulationToggle(false);

            XmlSaver.OnSave -= MachineData.Save;
            XmlLoader.OnLoad -= MachineData.Load;

            Configuration.Save();

            Object.Destroy(Internal.Scripter.Instance);
            Object.Destroy(GameObject.Find("Lench Scripter").transform.gameObject);

            LoadedScripter = false;
            LoadedAPI = false;
        }

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
        public override string BesiegeVersion { get; } = "v0.4";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;
#pragma warning restore CS1591
    }
}