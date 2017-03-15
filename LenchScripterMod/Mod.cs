using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Lench.Scripter.Internal;
using Lench.Scripter.Resources;
using Lench.Scripter.UI;
using spaar.ModLoader;
using UnityEngine;
using Configuration = Lench.Scripter.Internal.Configuration;
using MachineData = Lench.Scripter.Internal.MachineData;
using Object = UnityEngine.Object;
// ReSharper disable UnusedMember.Local

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
#if DEBUG
        public override string VersionExtra { get; } = "debug";
#endif
        public override string BesiegeVersion { get; } = "v0.42";
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
        internal static SettingsButton EnableScriptButton;
        internal static OptionsButton PythonVersion2Button;
        internal static OptionsButton PythonVersion3Button;

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
            Block.OnInitialisation += Script.Start;

            XmlSaver.OnSave += MachineData.Save;
            XmlLoader.OnLoad += MachineData.Load;

            Commands.RegisterCommand("lsm", ConfigurationCommand,
                "Scripter Mod configuration command.");
            Commands.RegisterCommand("py", PythonCommand,
                "Executes Python expression.");
            Commands.RegisterCommand("python", PythonCommand,
                "Executes Python expression.");

            Controller = new GameObject("LenchScripterMod") { hideFlags = HideFlags.DontSave };
            Controller.AddComponent<ModController>();

            IdentifierDisplayWindow = new IdentifierDisplayWindow();
            ScriptOptionsWindow = new ScriptOptionsWindow();
            WatchlistWindow = new WatchlistWindow();
            Toolbar = new Toolbar
            {
                Texture = Images.IconPython,
                Visible = Script.Enabled,
                Buttons =
                {
                    new Toolbar.Button
                    {
                        Style = new GUIStyle
                        {
                            normal = { background = Images.ButtonKeyNormal },
                            focused = { background = Images.ButtonKeyFocus },
                            hover = { background = Images.ButtonKeyHover },
                            active = { background = Images.ButtonKeyActive },
                            fixedWidth = 32,
                            fixedHeight = 32
                        },
                        Text="",
                        OnClick = OpenIdentifier
                    },
                    new Toolbar.Button
                    {
                        Style = new GUIStyle
                        {
                            normal = { background = Images.ButtonListNormal },
                            focused = { background = Images.ButtonListFocus },
                            hover = { background = Images.ButtonListHover },
                            active = { background = Images.ButtonListActive },
                            fixedWidth = 32,
                            fixedHeight = 32
                        },
                        Text="",
                        OnClick = OpenWatchlist
                    },
                    new Toolbar.Button
                    {
                        Style = new GUIStyle
                        {
                            normal = { background = Images.ButtonScriptNormal },
                            focused = { background = Images.ButtonScriptFocus },
                            hover = { background = Images.ButtonScriptHover },
                            active = { background = Images.ButtonScriptActive },
                            fixedWidth = 32,
                            fixedHeight = 32
                        },
                        Text="",
                        OnClick = OpenScript
                    },
                    new Toolbar.Button
                    {
                        Style = new GUIStyle()
                        {
                            normal = { background = Images.ButtonSettingsNormal },
                            focused = { background = Images.ButtonSettingsFocus },
                            hover = { background = Images.ButtonSettingsHover },
                            active = { background = Images.ButtonSettingsActive },
                            fixedWidth = 32,
                            fixedHeight = 32
                        },
                        Text="",
                        OnClick = OpenSettings
                    }
                }
            };

            Object.DontDestroyOnLoad(DependencyInstaller.Instance);

            LoadedAPI = true;

            Configuration.Load();

            EnableScriptButton = new SettingsButton
            {
                Text = "SCRIPT",
                Value = Script.Enabled,
                OnToggle = enabled =>
                {
                    Script.Enabled = enabled;
                    Toolbar.Visible = enabled;
                }
            };
            EnableScriptButton.Create();

            PythonVersion2Button = new OptionsButton
            {
                Text = "Python 2.7",
                Value = PythonEnvironment.Version == "ironpython2.7",
                OnToggle = enabled =>
                {
                    if (enabled)
                    {
                        if (PythonEnvironment.Version != "ironpython3.0") return;
                        PythonVersion3Button.Value = false;
                        Script.SetVersionAndReload("ironpython2.7");
                    }
                    else
                    {
                        PythonVersion2Button.Value = true;
                    }
                }
            };
            PythonVersion2Button.Create();

            PythonVersion3Button = new OptionsButton
            {
                Text = "Python 3.0",
                Value = PythonEnvironment.Version == "ironpython3.0",
                OnToggle = enabled =>
                {
                    if (enabled)
                    {
                        if (PythonEnvironment.Version != "ironpython2.7") return;
                        PythonVersion2Button.Value = false;
                        Script.SetVersionAndReload("ironpython3.0");
                    }
                    else
                    {
                        PythonVersion3Button.Value = true;
                    }
                }
            };
            PythonVersion3Button.Create();
            
            if (UpdateCheckerEnabled)
                CheckForModUpdate();
        }

        private static void OpenIdentifier()
        {
            IdentifierDisplayWindow.Visible = true;
        }

        private static void OpenWatchlist()
        {
            WatchlistWindow.Visible = true;
        }

        private static void OpenScript()
        {
            if (File.Exists(Script.FilePath))
            {
#if DEBUG
                Debug.Log($"Opening file {Script.FilePath} ...");
#endif
                Application.OpenURL(Script.FilePath);
            }
            else if (!string.IsNullOrEmpty(Script.EmbeddedCode))
            {
#if DEBUG
                Debug.Log($"Exporting code ...");
#endif
                Application.OpenURL(Script.Export());
            }
        }

        private static void OpenSettings()
        {
            ScriptOptionsWindow.Visible = true;
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
            Block.OnInitialisation -= Script.Start;

            XmlSaver.OnSave -= MachineData.Save;
            XmlLoader.OnLoad -= MachineData.Load;

            LoadedScripter = false;
            LoadedAPI = false;

            Object.Destroy(Controller);
        }

        internal class ModController : MonoBehaviour
        {
            private void Start()
            {
                if (Script.LoadEngine(true)) return;

                Debug.Log("[LenchScripterMod]: Additional assets required.\n" +
                          "\tFiles will be placed in Mods/Resources/LenchScripter.\n" +
                          "\tType `lsm python 2.7` or `lsm python 3.0` to download them.");
                DependencyInstaller.Visible = true;
            }

            private void Update()
            {
                if (Toolbar != null)
                    Toolbar.Visible = !StatMaster.inMenu &&
                                      !StatMaster.isSimulating &&
                                       Game.AddPiece != null;
            }
        }

        /// <summary>
        ///     Called on python console command.
        /// </summary>
        public static string PythonCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length == 0)
                return "Executes a Python expression.";

            if (Script.Python == null)
                return "Python engine not initialized.";

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
                                return null;
                            case "3.0":
                                PythonEnvironment.Version = "ironpython3.0";
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