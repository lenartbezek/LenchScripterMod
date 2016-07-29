using spaar.ModLoader;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Lench.Scripter.Internal
{
    /// <summary>
    /// Class representing an instance of the mod.
    /// </summary>
    public class Scripter : SingleInstance<Scripter>
    {
        /// <summary>
        /// Name in the Unity hierarchy.
        /// </summary>
        public override string Name { get; } = "Lench Scripter";

        internal Watchlist Watchlist;
        internal IdentifierDisplay IdentifierDisplay;
        internal ScriptOptions ScriptOptions;

        // Python environment
        internal string scriptFile;
        internal string scriptCode;

        internal bool enableScript = true;
        internal bool rebuildIDs = false;
        internal bool runtime_error = false;

        internal bool ModUpdaterEnabled = false;

        // Hovered block for ID dumping
        private GenericBlock hoveredBlock;

        private void LoadScript()
        {
            try
            {
                if (scriptFile != null)
                    PythonEnvironment.MainInstance.LoadScript(scriptFile);
                else if (scriptCode != null)
                    PythonEnvironment.MainInstance.LoadCode(scriptCode);
                ScriptOptions.SuccessMessage = "Successfully compiled code.";
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                ScriptOptions.ErrorMessage = "Error while compiling code.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
            }
        }

        /// <summary>
        /// Called on setting toggle.
        /// </summary>
        /// <param name="active"></param>
        internal void RunScriptSettingToggle(bool active)
        {
            enableScript = active;
            if (Game.IsSimulating && enableScript && PythonEnvironment.Loaded)
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
                return "Executes a Python expression.";

            string expression = "";
            for (int i = 0; i < args.Length; i++)
                expression += args[i] + " ";

            try
            {
                var result = PythonEnvironment.MainInstance.Execute(expression);
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
                    default:
                        return "Invalid command. Enter 'lsm' for all available commands.";
                }
            }
            else
            {
                return "Available commands:\n" +
                    "  lsm modupdate check  \t Checks for mod update.\n" +
                    "  lsm modupdate enable \t Enables update checker.\n" +
                    "  lsm modupdate disable\t Disables update checker.\n";
            }
        }

        private void CheckForModUpdate(bool verbose = false)
        {
            var updater = gameObject.AddComponent<Updater.Updater>();
            updater.Check(
                "Lench Scripter Mod",
                "https://api.github.com/repos/lench4991/LenchScripterMod/releases",
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
            PythonEnvironment.MainInstance = new PythonEnvironment();
            runtime_error = false;

            // Find script file
            if (scriptFile == null)
            {
                ScriptOptions.CheckForScript();
                if (ScriptOptions.ScriptSource == "py")
                {
                    scriptFile = ScriptOptions.ScriptPath;
                }
                if (ScriptOptions.ScriptSource == "bsg")
                {
                    scriptCode = ScriptOptions.Code;
                }
            }
        }

        /// <summary>
        /// Called to stop script.
        /// </summary>
        private void DestroyScriptingEnvironment()
        {
            Functions.ClearMarks(false);
            PythonEnvironment.MainInstance = null;
        }

        /// <summary>
        /// Finds hovered block in buildingBlocks dictionary and shows identifier display.
        /// </summary>
        private void ShowBlockIdentifiers()
        {
            if (Game.AddPiece == null || Game.AddPiece.HoveredBlock == null)
            {
                hoveredBlock = null;
                return;
            }

            hoveredBlock = Game.AddPiece.HoveredBlock;

            IdentifierDisplay.ShowBlock(hoveredBlock);
        }

        private void Awake()
        {
            Watchlist = gameObject.AddComponent<Watchlist>();
            IdentifierDisplay = gameObject.AddComponent<IdentifierDisplay>();
            ScriptOptions = gameObject.AddComponent<ScriptOptions>();
        }

        private void Start()
        {
            if (PythonEnvironment.Loaded)
                CreateScriptingEnvironment();

            if (ModUpdaterEnabled)
                CheckForModUpdate();
        }

        private void OnDestroy()
        {
            DestroyScriptingEnvironment();
            PythonEnvironment.DestroyEngine();
            Destroy(Watchlist);
            Destroy(IdentifierDisplay);
            Destroy(ScriptOptions);
        }

        /// <summary>
        /// Mod functionality.
        /// Calls Python functions.
        /// </summary>
        private void Update()
        {
            // Initialize block handlers
            if (Game.IsSimulating && !BlockHandlers.Initialised)
                BlockHandlers.InitializeBlockHandlers();

            // Initialize block identifiers
            if (!Game.IsSimulating && rebuildIDs)
            {
                rebuildIDs = false;
                BlockHandlers.InitializeBuildingBlockIDs();
            }

            // Execute code on first call
            if (Game.IsSimulating && PythonEnvironment.Loaded && enableScript && (scriptFile != null || scriptCode != null))
            {
                LoadScript();
                scriptFile = null;
                scriptCode = null;
            }

            // Toggle watchlist visibility
            if (PythonEnvironment.Loaded && Keybindings.Get("Watchlist").Pressed())
            {
                Watchlist.Visible = !Watchlist.Visible;
            }

            // Toggle options visibility
            if (PythonEnvironment.Loaded && Keybindings.Get("Script Options").Pressed())
            {
                ScriptOptions.Visible = !ScriptOptions.Visible;
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
                if (!runtime_error)
                    PythonEnvironment.MainInstance?.CallUpdate();
            }
            catch (Exception e)
            {
                runtime_error = true;
                if (e.InnerException != null) e = e.InnerException;
                ScriptOptions.ErrorMessage = "Runtime error.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
            }

            // Call OnUpdate event for Block handlers.
            BlockHandlers.CallUpdate();
        }

        private void LateUpdate()
        {
            // Call OnLateUpdate event for Block handlers.
            BlockHandlers.CallLateUpdate();
        }

        /// <summary>
        /// Calls Lua functions at a fixed rate.
        /// </summary>
        private void FixedUpdate()
        {
            if (!Game.IsSimulating) return;

            // Call script update.
            try
            {
                if (!runtime_error)
                    PythonEnvironment.MainInstance?.CallFixedUpdate();
            }
            catch (Exception e)
            {
                runtime_error = true;
                if (e.InnerException != null) e = e.InnerException;
                ScriptOptions.ErrorMessage = "Runtime error.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
            }

            // Call OnLateUpdate event for Block handlers.
            BlockHandlers.CallFixedUpdate();
        }

        /// <summary>
        /// Handles starting and stopping of the simulation.
        /// </summary>
        /// <param name="isSimulating"></param>
        internal void OnSimulationToggle(bool isSimulating)
        {
            BlockHandlers.DestroyBlockHandlers();
            if (enableScript && PythonEnvironment.Loaded)
            {
                DestroyScriptingEnvironment();
                CreateScriptingEnvironment();
            }
            ScriptOptions.SuccessMessage = null;
            ScriptOptions.NoteMessage = null;
            ScriptOptions.ErrorMessage = null;
        }
    }
}
