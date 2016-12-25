using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Lench.Scripter.Internal;
using Lench.Scripter.Resources;
using Lench.Scripter.UI;
using spaar.ModLoader;
using spaar.ModLoader.UI;
using UnityEngine;
using Configuration = Lench.Scripter.Internal.Configuration;
using MachineData = Lench.Scripter.Internal.MachineData;
using Object = UnityEngine.Object;

namespace Lench.Scripter
{
    /// <summary>
    ///     Mod class loaded by the Mod Loader.
    /// </summary>
    public class Mod : spaar.ModLoader.Mod
    {
#pragma warning disable CS1591
        public override string Name { get; } = "LenchScripterMod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "Lench";
        public override Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.4";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;
#pragma warning restore CS1591

        /// <summary>
        ///     Is LenchScripterMod Block API loaded.
        /// </summary>
        public static bool LoadedAPI { get; internal set; }

        /// <summary>
        ///     Is LenchScripterMod full scripter loaded.
        /// </summary>
        public static bool LoadedScripter { get; internal set; }

        /// <summary>
        ///     Automatic update checker.
        /// </summary>
        public static bool UpdateCheckerEnabled { get; internal set; }

        /// <summary>
        ///     Parent GameObject of all mod components.
        /// </summary>
        public static GameObject Controller { get; internal set; }

        internal static IdentifierDisplayWindow IdentifierDisplayWindow;
        internal static ScriptOptionsWindow ScriptOptionsWindow;
        internal static WatchlistWindow WatchlistWindow;
        internal static Toolbar Toolbar;

        /// <summary>
        ///     Instantiates the mod and it's components.
        ///     Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            Game.OnSimulationToggle += Block.OnSimulationToggle;
            Game.OnSimulationToggle += Script.OnSimulationToggle;
            Game.OnBlockPlaced += block => Block.FlagForIDRebuild();
            Game.OnBlockRemoved += Block.FlagForIDRebuild;

            XmlSaver.OnSave += MachineData.Save;
            XmlLoader.OnLoad += MachineData.Load;

            Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
            Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

            Commands.RegisterCommand("lsm", ConfigurationCommand,
                "Scripter Mod configuration command.");
            Commands.RegisterCommand("py", PythonCommand,
                "Executes Python expression.");
            Commands.RegisterCommand("python", PythonCommand,
                "Executes Python expression.");

            SettingsMenu.RegisterSettingsButton("SCRIPT", enabled => Script.Enabled = enabled, true, 12);

            Controller = new GameObject("LenchScripterMod") { hideFlags = HideFlags.DontSave };
            var component = Controller.AddComponent<ModController>();
            component.StartCoroutine(CreateToolbar());

            IdentifierDisplayWindow = new IdentifierDisplayWindow();
            ScriptOptionsWindow = new ScriptOptionsWindow();
            WatchlistWindow = new WatchlistWindow();

            Script.LoadEngine();
            LoadedAPI = true;

            Configuration.Load();
        }

        /// <summary>
        ///     Disables the mod from executing scripts.
        ///     Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {
            Configuration.Save();

            Game.OnSimulationToggle -= Block.OnSimulationToggle;
            Game.OnSimulationToggle -= Script.OnSimulationToggle;
            Game.OnBlockRemoved -= Block.FlagForIDRebuild;

            XmlSaver.OnSave -= MachineData.Save;
            XmlLoader.OnLoad -= MachineData.Load;

            LoadedScripter = false;
            LoadedAPI = false;

            Object.Destroy(Controller);
        }

        internal class ModController : MonoBehaviour
        {
            
        }

        internal static IEnumerator CreateToolbar()
        {
            if (!Elements.IsInitialized) yield return null;
            Toolbar = new Toolbar
            {
                Texture = Images.ic_python_32,
                Visible = true,
                Buttons =
                {
                    new Toolbar.Button
                    {
                        Texture = Images.ic_key_32,
                        OnClick = () => { IdentifierDisplayWindow.Visible = true; }
                    },
                    new Toolbar.Button
                    {
                        Texture = Images.ic_eye_32,
                        OnClick = () => { WatchlistWindow.Visible = true; }
                    },
                    new Toolbar.Button
                    {
                        Texture = Images.ic_code_32,
                        OnClick = () => { }
                    },
                    new Toolbar.Button
                    {
                        Texture = Images.ic_settings_32,
                        OnClick = () => { ScriptOptionsWindow.Visible = true; }
                    }
                }
            };
        }

        /// <summary>
        ///     Called on python console command.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="namedArgs"></param>
        /// <returns></returns>
        public static string PythonCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length == 0)
                return "Executes a Python expression.";

            var expression = args.Aggregate("", (current, t) => current + (t + " "));

            try
            {
                var result = Script.Python.Execute(expression);
                return result?.ToString() ?? "";
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" +
                          PythonEnvironment.FormatException(e));
                return "";
            }
        }

        /// <summary>
        ///     Called on lsm console command.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="namedArgs"></param>
        /// <returns></returns>
        public static string ConfigurationCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length <= 0)
                return "Available commands:\n" +
                       "  lsm modupdate check  \t Checks for mod update.\n" +
                       "  lsm modupdate enable \t Enables update checker.\n" +
                       "  lsm modupdate disable\t Disables update checker.\n" +
                       "  lsm python version   \t Current Python version.\n" +
                       "  lsm python 2.7       \t Switches to IronPython 2.7.\n" +
                       "  lsm python 3.0       \t Switches to IronPython 3.0.\n";
            switch (args[0].ToLower())
            {
                case "modupdate":
                    if (args.Length > 1)
                        switch (args[1].ToLower())
                        {
                            case "check":
                                CheckForModUpdate(true);
                                return "Checking for mod updates ...";
                            case "enable":
                                UpdateCheckerEnabled = true;
                                return "Mod update checker enabled.";
                            case "disable":
                                UpdateCheckerEnabled = false;
                                return "Mod update checker disabled.";
                            default:
                                return
                                    "Invalid argument [check/enable/disable]. Enter 'lsm' for all available commands.";
                        }
                    return "Missing argument [check/enable/disable]. Enter 'lsm' for all available commands.";
                case "python":
                    if (args.Length > 1)
                        switch (args[1].ToLower())
                        {
                            case "version":
                                return (string)Script.Python.Execute("sys.version");
                            case "2.7":
                                PythonEnvironment.Version = "ironpython2.7";
                                if (!Script.LoadEngine(true))
                                {
                                    PythonEnvironment.DestroyEngine();
                                    DependencyInstaller.InstallIronPython();
                                }
                                return null;
                            case "3.0":
                                PythonEnvironment.Version = "ironpython3.0";
                                if (!Script.LoadEngine(true))
                                {
                                    PythonEnvironment.DestroyEngine();
                                    DependencyInstaller.InstallIronPython();
                                }
                                return null;
                            default:
                                return "Invalid argument [version/2.7/3.0]. Enter 'lsm' for all available commands.";
                        }
                    return "Missing argument [version/2.7/3.0]. Enter 'lsm' for all available commands.";
                default:
                    return "Invalid command. Enter 'lsm' for all available commands.";
            }
        }

        /// <summary>
        ///     Checks for mod update.
        /// </summary>
        /// <param name="verbose">Log status into console.</param>
        public static void CheckForModUpdate(bool verbose = false)
        {
            Updater.Check(
                "Lench Scripter Mod",
                "https://api.github.com/repos/lench4991/LenchScripterMod/releases/latest",
                Assembly.GetExecutingAssembly().GetName().Version,
                new List<Updater.Link>
                {
                    new Updater.Link
                    {
                        DisplayName = "Spiderling forum page",
                        URL = "http://forum.spiderlinggames.co.uk/index.php?threads/3003/"
                    },
                    new Updater.Link
                    {
                        DisplayName = "GitHub release page",
                        URL = "https://github.com/lench4991/LenchScripterMod/releases/latest"
                    }
                },
                verbose);
        }
    }
}