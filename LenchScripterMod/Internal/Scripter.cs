using spaar.ModLoader;
using System.Collections.Generic;
using UnityEngine;

namespace LenchScripter.Internal
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
        internal PythonEnvironment python;
        internal string scriptFile;
        internal string scriptCode;

        internal bool isSimulating;
        internal bool enableScript = true;

        // Hovered block for ID dumping
        private GenericBlock hoveredBlock;

        private void LoadScript()
        {
            bool success = true;

            if (scriptFile != null)
                success = python.LoadScript(scriptFile);
            else if (scriptCode != null)
                success = python.LoadCode(scriptCode);

            if (success)
                ScriptOptions.SuccessMessage = "Successfully compiled code.";
            else
            {
                ScriptOptions.ErrorMessage = "Error while compiling code.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + python.LastException.Message + "</color></b>\n" + PythonEnvironment.FormatException(python.LastException));
            }
        }

        /// <summary>
        /// Called on setting toggle.
        /// </summary>
        /// <param name="active"></param>
        internal void RunScriptSettingToggle(bool active)
        {
            enableScript = active;
            if (isSimulating && enableScript && PythonEnvironment.Loaded)
                CreateScriptingEnvironment();
            else
                DestroyScriptingEnvironment();
        }

        /// <summary>
        /// Called on lua console command.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="namedArgs"></param>
        /// <returns></returns>
        internal string InteractiveCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length == 0)
                return "Executes a Python expression.";
            if (!isSimulating || python == null)
                return "Can only be called while simulating.";

            string expression = "";
            for (int i = 0; i < args.Length; i++)
                expression += args[i] + " ";

            object result = null;
            var success = python.Execute(expression, out result);
            if (success)
            {
                return result != null ? result.ToString() : "";
            }
            else
            {
                Debug.Log("<b><color=#FF0000>Python error: " + python.LastException.Message + "</color></b>\n" + PythonEnvironment.FormatException(python.LastException));
                return "";
            }

        }

        /// <summary>
        /// Creates environment. Looks for script to load.
        /// </summary>
        private void CreateScriptingEnvironment()
        {
            python = new PythonEnvironment();

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
            python = null;
        }

        /// <summary>
        /// Finds hovered block in buildingBlocks dictionary and shows identifier display.
        /// </summary>
        private void ShowBlockIdentifiers()
        {
            if (Game.AddPiece.HoveredBlock == null)
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

        /// <summary>
        /// Mod functionality.
        /// Calls Lua functions.
        /// </summary>
        private void Update()
        {
            // Initialize block handlers
            if (isSimulating && !BlockHandlers.Initialised)
                BlockHandlers.InitializeBlockHandlers();

            // Execute code on first call
            if (enableScript && (scriptFile != null || scriptCode != null) && PythonEnvironment.Loaded)
            {
                LoadScript();
                scriptFile = null;
                scriptCode = null;
            }

            // Toggle watchlist visibility
            if (Keybindings.Get("Watchlist").Pressed() && PythonEnvironment.Loaded)
            {
                Watchlist.Visible = !Watchlist.Visible;
            }

            // Toggle options visibility
            if (Keybindings.Get("Script Options").Pressed() && PythonEnvironment.Loaded)
            {
                ScriptOptions.Visible = !ScriptOptions.Visible;
            }

            if (!isSimulating)
            {
                // Show block identifiers
                if (Keybindings.Get("Show Block ID").IsDown() && PythonEnvironment.Loaded)
                {
                    ShowBlockIdentifiers();
                }
            }

            if (!isSimulating) return;

            // Call script update.
            var success = python?.CallUpdate();
            if (success.HasValue && !success.Value)
            {
                ScriptOptions.ErrorMessage = "Runtime error.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + python.LastException.Message + "</color></b>\n" + PythonEnvironment.FormatException(python.LastException));
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
            if (!isSimulating) return;

            // Call script update;
            var success = python?.CallFixedUpdate();
            if (success.HasValue && !success.Value)
            {
                ScriptOptions.ErrorMessage = "Runtime error.\nSee console (Ctrl+K) for more info.";
                Debug.Log("<b><color=#FF0000>Python error: " + python.LastException.Message + "</color></b>\n" + PythonEnvironment.FormatException(python.LastException));
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
            this.isSimulating = isSimulating;
            if (isSimulating)
            {
                if (enableScript && PythonEnvironment.Loaded) CreateScriptingEnvironment();
            }
            else
            {
                DestroyScriptingEnvironment();
            }
            ScriptOptions.SuccessMessage = null;
            ScriptOptions.NoteMessage = null;
            ScriptOptions.ErrorMessage = null;
        }
    }
}
