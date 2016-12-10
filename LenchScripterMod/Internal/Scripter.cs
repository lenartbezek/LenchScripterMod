using spaar.ModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
// ReSharper disable UnusedMember.Local

namespace Lench.Scripter.Internal
{
    /// <summary>
    /// Main mod controller.
    /// </summary>
    public class Scripter : SingleInstance<Scripter>
    {
        /// <summary>
        /// Name in the Unity hierarchy.
        /// </summary>
        public override string Name { get; } = "Lench Scripter";

        // Python environment
        internal string ScriptFile;
        internal string ScriptCode;

        internal bool EnableScript = true;
        internal bool RebuildIDs;
        internal bool RuntimeError;

        internal bool ModUpdaterEnabled;

        // Hovered block for ID dumping
        private GenericBlock _hoveredBlock;

        private void LoadScript()
        {
            try
            {
                if (ScriptFile != null)
                    PythonEnvironment.ScripterEnvironment.LoadScript(ScriptFile);
                else if (ScriptCode != null)
                    PythonEnvironment.ScripterEnvironment.LoadCode(ScriptCode);
                ScriptOptions.Instance.SuccessMessage = "Successfully compiled code.";
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                ScriptOptions.Instance.ErrorMessage = "Error while compiling code.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
            }
        }

        /// <summary>
        /// Called on setting toggle.
        /// </summary>
        /// <param name="active"></param>
        internal void RunScriptSettingToggle(bool active)
        {
            EnableScript = active;
            if (Game.IsSimulating && EnableScript && PythonEnvironment.Loaded)
                CreateScriptingEnvironment();
            else
                DestroyScriptingEnvironment();
        }

        /// <summary>
        /// Called on python console command.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="namedArgs"></param>
        /// <returns></returns>
        internal string PythonCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length == 0)
            {
                return "Executes a Python expression.";
            }

            string expression = "";
            for (int i = 0; i < args.Length; i++)
                expression += args[i] + " ";

            try
            {
                var result = PythonEnvironment.ScripterEnvironment.Execute(expression);
                return result != null ? result.ToString() : "";
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
                return "";
            }
        }

        internal string ConfigurationCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length > 0)
            {
                switch (args[0].ToLower())
                {
                    case "modupdate":
                        if (args.Length > 1)
                        {
                            switch (args[1].ToLower())
                            {
                                case "check":
                                    CheckForModUpdate(true);
                                    return "Checking for mod updates ...";
                                case "enable":
                                    ModUpdaterEnabled = true;
                                    return "Mod update checker enabled.";
                                case "disable":
                                    ModUpdaterEnabled = false;
                                    return "Mod update checker disabled.";
                                default:
                                    return "Invalid argument [check/enable/disable]. Enter 'lsm' for all available commands.";
                            }
                        }
                        else
                        {
                            return "Missing argument [check/enable/disable]. Enter 'lsm' for all available commands.";
                        }
                    case "python":
                        if (args.Length > 1)
                        {
                            switch (args[1].ToLower())
                            {
                                case "version":
                                    return (string)PythonEnvironment.ScripterEnvironment.Execute("sys.version");
                                case "2.7":
                                    PythonEnvironment.Version = "ironpython2.7";
                                    if (PythonEnvironment.LoadPythonAssembly())
                                    {
                                        PythonEnvironment.InitializeEngine();
                                        PythonEnvironment.ScripterEnvironment = new PythonEnvironment();
                                        return (string)PythonEnvironment.ScripterEnvironment.Execute("sys.version");
                                    }
                                    else
                                    {
                                        PythonEnvironment.DestroyEngine();
                                        DependencyInstaller.InstallIronPython();
                                        return null;
                                    }
                                case "3.0":
                                    PythonEnvironment.Version = "ironpython3.0";
                                    if (PythonEnvironment.LoadPythonAssembly())
                                    {
                                        PythonEnvironment.InitializeEngine();
                                        PythonEnvironment.ScripterEnvironment = new PythonEnvironment();
                                        return (string)PythonEnvironment.ScripterEnvironment.Execute("sys.version");
                                    }
                                    else
                                    {
                                        PythonEnvironment.DestroyEngine();
                                        DependencyInstaller.InstallIronPython();
                                        return null;
                                    }
                                default:
                                    return "Invalid argument [version/2.7/3.0]. Enter 'lsm' for all available commands.";
                            }
                        }
                        else
                        {
                            return "Missing argument [version/2.7/3.0]. Enter 'lsm' for all available commands.";
                        }
                    default:
                        return "Invalid command. Enter 'lsm' for all available commands.";
                }
            }
            else
            {
                return "Available commands:\n" +
                    "  lsm modupdate check  \t Checks for mod update.\n" +
                    "  lsm modupdate enable \t Enables update checker.\n" +
                    "  lsm modupdate disable\t Disables update checker.\n" +
                    "  lsm python version   \t Current Python version.\n" +
                    "  lsm python 2.7       \t Switches to IronPython 2.7.\n" +
                    "  lsm python 3.0       \t Switches to IronPython 3.0.\n";
            }
        }

        private void CheckForModUpdate(bool verbose = false)
        {
            var updater = gameObject.AddComponent<Updater.Updater>();
            updater.Check(
                "Lench Scripter Mod",
                "https://api.github.com/repos/lench4991/LenchScripterMod/releases/latest",
                Assembly.GetExecutingAssembly().GetName().Version,
                new List<Updater.Updater.Link>()
                    {
                            new Updater.Updater.Link() { DisplayName = "Spiderling forum page", URL = "http://forum.spiderlinggames.co.uk/index.php?threads/3003/" },
                            new Updater.Updater.Link() { DisplayName = "GitHub release page", URL = "https://github.com/lench4991/LenchScripterMod/releases/latest" }
                    },
                verbose);
        }

        /// <summary>
        /// Creates environment. Looks for script to load.
        /// </summary>
        private void CreateScriptingEnvironment()
        {
            PythonEnvironment.ScripterEnvironment = new PythonEnvironment();
            RuntimeError = false;

            // Find script file
            if (ScriptFile == null)
            {
                ScriptOptions.Instance.CheckForScript();
                if (ScriptOptions.Instance.ScriptSource == "py")
                {
                    ScriptFile = ScriptOptions.Instance.ScriptPath;
                }
                if (ScriptOptions.Instance.ScriptSource == "bsg")
                {
                    ScriptCode = ScriptOptions.Instance.Code;
                }
            }
        }

        /// <summary>
        /// Called to stop script.
        /// </summary>
        public void DestroyScriptingEnvironment()
        {
            Functions.ClearMarks(false);
            PythonEnvironment.ScripterEnvironment = null;
        }

        /// <summary>
        /// Finds hovered block in buildingBlocks dictionary and shows identifier display.
        /// </summary>
        private void ShowBlockIdentifiers()
        {
            if (Game.AddPiece == null || Game.AddPiece.HoveredBlock == null)
            {
                _hoveredBlock = null;
                return;
            }

            _hoveredBlock = Game.AddPiece.HoveredBlock;

            IdentifierDisplay.Instance.ShowBlock(_hoveredBlock);
        }
        
        private void Awake()
        {
            gameObject.AddComponent<DependencyInstaller>();
            gameObject.AddComponent<BlockHandlerController>();
            gameObject.AddComponent<Watchlist>();
            gameObject.AddComponent<IdentifierDisplay>();
            gameObject.AddComponent<ScriptOptions>();
        }
        
        private void Start()
        {
            ScripterMod.LoadScripter();

            if (!PythonEnvironment.Loaded)
                DependencyInstaller.Instance.Visible = true;

            if (ModUpdaterEnabled)
                CheckForModUpdate();
        }

        private void OnDestroy()
        {
            DestroyScriptingEnvironment();
            PythonEnvironment.DestroyEngine();
            Destroy(DependencyInstaller.Instance);
            Destroy(BlockHandlerController.Instance);
            Destroy(Watchlist.Instance);
            Destroy(IdentifierDisplay.Instance);
            Destroy(ScriptOptions.Instance);
        }

        /// <summary>
        /// Mod functionality.
        /// Calls Python functions.
        /// </summary>
        private void Update()
        {
            // Initialize block handlers
            if (Game.IsSimulating && !BlockHandlerController.Initialised)
                BlockHandlerController.InitializeBlockHandlers();

            // Initialize block identifiers
            if (!Game.IsSimulating && RebuildIDs)
            {
                RebuildIDs = false;
                BlockHandlerController.InitializeBuildingBlockIDs();
            }

            // Execute code on first call
            if (Game.IsSimulating && PythonEnvironment.Loaded && EnableScript && (ScriptFile != null || ScriptCode != null))
            {
                LoadScript();
                ScriptFile = null;
                ScriptCode = null;
            }

            // Toggle watchlist visibility
            if (PythonEnvironment.Loaded && Keybindings.Get("Watchlist").Pressed())
            {
                Watchlist.Instance.Visible = !Watchlist.Instance.Visible;
            }

            // Toggle options visibility
            if (PythonEnvironment.Loaded && Keybindings.Get("Script Options").Pressed())
            {
                ScriptOptions.Instance.Visible = !ScriptOptions.Instance.Visible;
            }

            if (!Game.IsSimulating)
            {
                // Show block identifiers
                if (PythonEnvironment.Loaded && Keybindings.Get("Show Block ID").IsDown())
                {
                    ShowBlockIdentifiers();
                }
            }

            if (!Game.IsSimulating) return;

            // Call script update.
            try
            {
                if (!RuntimeError)
                    PythonEnvironment.ScripterEnvironment?.CallUpdate();
            }
            catch (Exception e)
            {
                RuntimeError = true;
                if (e.InnerException != null) e = e.InnerException;
                ScriptOptions.Instance.ErrorMessage = "Runtime error.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
            }
        }

        /// <summary>
        /// Calls Python functions at a fixed rate.
        /// </summary>
        private void FixedUpdate()
        {
            if (!Game.IsSimulating) return;

            // Call script update.
            try
            {
                if (!RuntimeError)
                    PythonEnvironment.ScripterEnvironment?.CallFixedUpdate();
            }
            catch (Exception e)
            {
                RuntimeError = true;
                if (e.InnerException != null) e = e.InnerException;
                ScriptOptions.Instance.ErrorMessage = "Runtime error.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
            }
        }

        /// <summary>
        /// Handles starting and stopping of the simulation.
        /// </summary>
        /// <param name="isSimulating"></param>
        internal void OnSimulationToggle(bool isSimulating)
        {
            Functions.ResetTimer();
            BlockHandlerController.DestroyBlockHandlers();
            if (EnableScript && PythonEnvironment.Loaded)
            {
                DestroyScriptingEnvironment();
                CreateScriptingEnvironment();
            }
            ScriptOptions.Instance.SuccessMessage = null;
            ScriptOptions.Instance.NoteMessage = null;
            ScriptOptions.Instance.ErrorMessage = null;
        }
    }
}
