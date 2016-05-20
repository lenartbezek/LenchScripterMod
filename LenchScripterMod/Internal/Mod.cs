using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;

namespace LenchScripter.Internal
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

        internal static Type blockScriptType;

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
            XmlSaver.OnSave += MachineData.Save;
            XmlLoader.OnLoad += MachineData.Load;

            LoadBlockLoaderAssembly();

            Configuration.Load();

            Keybindings.AddKeybinding("Show Block ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
            Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

            Commands.RegisterCommand("python", Scripter.Instance.InteractiveCommand, "Executes Python expression.");

            SettingsMenu.RegisterSettingsButton("SCRIPT", Scripter.Instance.RunScriptSettingToggle, true, 12);
        }

        /// <summary>
        /// Disables the mod from executing scripts.
        /// Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced -= (Transform block) => BlockHandlers.rebuildDict = true;
            Game.OnBlockRemoved -= () => BlockHandlers.rebuildDict = true;
            XmlSaver.OnSave -= MachineData.Save;
            XmlLoader.OnLoad -= MachineData.Load;

            Scripter.Instance.OnSimulationToggle(false);

            Configuration.Save();

            UnityEngine.Object.Destroy(Scripter.Instance);
        }

        /// <summary>
        /// Attempts to load TGYD's BlockLoader assembly.
        /// </summary>
        /// <returns>Returns true if successfull.</returns>
        private bool LoadBlockLoaderAssembly()
        {
            Assembly blockLoaderAssembly;
            try
            {
                blockLoaderAssembly = Assembly.LoadFrom(Application.dataPath + "/Mods/BlockLoader.dll");
            }
            catch (FileNotFoundException)
            {
                return false;
            }

            foreach (Type type in blockLoaderAssembly.GetExportedTypes())
            {
                if (type.FullName == "BlockScript")
                    blockScriptType = type;
            }

            if (blockScriptType == null)
                return false;

            return true;
        }
    }

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

            if(scriptFile != null)
                success = python.LoadScript(scriptFile);
            else if (scriptCode != null)
                success = python.LoadCode(scriptCode);

            if (success)
                ScriptOptions.SuccessMessage = "Successfully compiled code.";
            else
            {
                ScriptOptions.ErrorMessage = "Error while compiling code.\nSee console (Ctrl+K) for more info.";
                ModConsole.AddMessage(LogType.Log, "<b><color=#FF0000>Python error:</color></b>\n"+python.LastException);
            }
        }

        /// <summary>
        /// Called on setting toggle.
        /// </summary>
        /// <param name="active"></param>
        internal void RunScriptSettingToggle(bool active)
        {
            enableScript = active;
            if (isSimulating && enableScript)
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

            object result = python.Evaluate<object>(expression);

            if (result != null)
            {
                result.ToString();
            }
                
            return "";
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

        private void Start()
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
            if (enableScript && (scriptFile != null || scriptCode != null))
            {
                LoadScript();
                scriptFile = null;
                scriptCode = null;
            }

            // Toggle watchlist visibility
            if (Keybindings.Get("Watchlist").Pressed())
            {
                Watchlist.Visible = !Watchlist.Visible;
            }

            // Toggle options visibility
            if (Keybindings.Get("Script Options").Pressed())
            {
                ScriptOptions.Visible = !ScriptOptions.Visible;
            }

            if (!isSimulating)
            {
                // Show block identifiers
                if (Keybindings.Get("Show Block ID").IsDown())
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
                ModConsole.AddMessage(LogType.Log, "<b><color=#FF0000>Python error:</color></b>\n" + python.LastException);
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
                ModConsole.AddMessage(LogType.Log, "<b><color=#FF0000>Python error:</color></b>\n" + python.LastException);
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
                if (enableScript) CreateScriptingEnvironment();
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
