using spaar.ModLoader;
using System;
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

        internal bool enableScript = true;
        internal bool rebuildIDs = false;
        internal bool runtime_error = false;

        // Hovered block for ID dumping
        private GenericBlock hoveredBlock;

        private void LoadScript()
        {
            try
            {
                if (scriptFile != null)
                    python.LoadScript(scriptFile);
                else if (scriptCode != null)
                    python.LoadCode(scriptCode);
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
        /// Called on lua console command.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="namedArgs"></param>
        /// <returns></returns>
        internal string InteractiveCommand(string[] args, IDictionary<string, string> namedArgs)
        {
            if (args.Length == 0)
                return "Executes a Python expression.";
            if (!Game.IsSimulating || python == null)
                return "Can only be called while simulating.";

            string expression = "";
            for (int i = 0; i < args.Length; i++)
                expression += args[i] + " ";

            try
            {
                var result = python.Execute(expression);
                return result != null ? result.ToString() : "";
            }
            catch (Exception e)
            {
                if (e.InnerException != null) e = e.InnerException;
                Debug.Log("<b><color=#FF0000>Python error: " + e.Message + "</color></b>\n" + PythonEnvironment.FormatException(e));
                return "";
            }

        }

        /// <summary>
        /// Creates environment. Looks for script to load.
        /// </summary>
        private void CreateScriptingEnvironment()
        {
            python = new PythonEnvironment();
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
            python = null;
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

        private void OnDestroy()
        {
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
            if (enableScript && (scriptFile != null || scriptCode != null) && PythonEnvironment.Loaded)
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
                    python?.CallUpdate();
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
                    python?.CallFixedUpdate();
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
