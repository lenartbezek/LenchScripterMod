using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using spaar.ModLoader;
using UnityEngine;
using LenchScripter.Blocks;

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

        internal static Watchlist Watchlist;
        internal static IdentifierDisplay IdentifierDisplay;
        internal static ScriptOptions ScriptOptions;
        internal static Type blockScriptType;

        /// <summary>
        /// Instantiates the mod and it's components.
        /// Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Scripter.Instance);
            Game.OnSimulationToggle += Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced += (Transform block) => Scripter.Instance.rebuildDict = true;
            Game.OnBlockRemoved += () => Scripter.Instance.rebuildDict = true;
            XmlSaver.OnSave += MachineData.Save;
            XmlLoader.OnLoad += MachineData.Load;

            Watchlist = Scripter.Instance.gameObject.AddComponent<Watchlist>();
            IdentifierDisplay = Scripter.Instance.gameObject.AddComponent<IdentifierDisplay>();
            ScriptOptions = Scripter.Instance.gameObject.AddComponent<ScriptOptions>();

            LoadBlockLoaderAssembly();

            Configuration.Load();

            Keybindings.AddKeybinding("Show Blocks ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
            Keybindings.AddKeybinding("Script Options", new Key(KeyCode.LeftControl, KeyCode.U));

            Commands.RegisterCommand("py", Scripter.Instance.InteractiveCommand, "Executes Python expression.");

            SettingsMenu.RegisterSettingsButton("SCRIPT", Scripter.Instance.RunScriptSettingToggle, true, 12);
        }

        /// <summary>
        /// Disables the mod from executing scripts.
        /// Destroys GameObjects.
        /// </summary>
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= Scripter.Instance.OnSimulationToggle;
            Game.OnBlockPlaced -= (Transform block) => Scripter.Instance.rebuildDict = true;
            Game.OnBlockRemoved -= () => Scripter.Instance.rebuildDict = true;
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

        // Python environment
        internal PythonEnvironment python;
        internal string scriptFile;
        internal string scriptCode;

        internal bool isSimulating;
        internal bool enableScript = true;
        internal bool handlersInitialised = false;

        // Hovered block for ID dumping
        private GenericBlock hoveredBlock;

        // Machine changed - flag for rebuild
        internal bool rebuildDict = false;

        // Map: Building Block -> ID
        internal Dictionary<GenericBlock, string> buildingBlocks;

        // Map: GUID -> Simulation Block
        internal Dictionary<Guid, Block> guidToSimulationBlock;

        // Map: ID -> Simulation Block
        internal Dictionary<string, Block> idToSimulationBlock;

        // Map: BlockType -> BlockHandler type
        internal Dictionary<int, Type> HandlerTypes = new Dictionary<int, Type>
        {
            {(int)BlockType.Cannon, typeof(Cannon)},
            {(int)BlockType.ShrapnelCannon, typeof(Cannon)},
            {(int)BlockType.CogMediumPowered, typeof(Cog)},
            {(int)BlockType.Wheel, typeof(Cog)},
            {(int)BlockType.LargeWheel, typeof(Cog)},
            {(int)BlockType.Drill, typeof(Cog)},
            {(int)BlockType.Decoupler, typeof(Decoupler)},
            {(int)BlockType.Flamethrower, typeof(Flamethrower)},
            {(int)BlockType.FlyingBlock, typeof(FlyingSpiral)},
            {(int)BlockType.Grabber, typeof(Grabber)},
            {(int)BlockType.Grenade, typeof(Grenade)},
            {(int)BlockType.Piston, typeof(Piston)},
            {59, typeof(Rocket) },
            {(int)BlockType.Spring, typeof(Spring)},
            {(int)BlockType.RopeWinch, typeof(Spring)},
            {(int)BlockType.SteeringHinge, typeof(Steering)},
            {(int)BlockType.SteeringBlock, typeof(Steering)},
            {(int)BlockType.WaterCannon, typeof(WaterCannon)},
            {410, typeof(Automatron)}
        };

        /// <summary>
        /// Events invoked on updates.
        /// </summary>
        internal delegate void UpdateEventHandler();
        internal event UpdateEventHandler OnUpdate;

        internal delegate void LateUpdateEventHandler();
        internal event LateUpdateEventHandler OnLateUpdate;

        internal delegate void FixedUpdateEventHandler();
        internal event FixedUpdateEventHandler OnFixedUpdate;

        /// <summary>
        /// Event invoked when simulation block handlers are initialised.
        /// </summary>
        public delegate void InitialisationEventHandler();

        /// <summary>
        /// Initializes and returns new Block object.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        /// <returns>LenchScripterMod.Block object.</returns>
        private Block CreateBlock(BlockBehaviour bb)
        {
            Block block;
            if (HandlerTypes.ContainsKey(bb.GetBlockID()))
                block = (Block)Activator.CreateInstance(HandlerTypes[bb.GetBlockID()], new object[] { bb });
            else
                block = new Block(bb);
            return block;
        }

        /// <summary>
        /// Finds blockGuid string in dictionary of simulation blocks.
        /// </summary>
        /// <param name="blockGuid">Block's GUID.</param>
        /// <returns>Returns reference to blocks Block handler object.</returns>
        internal Block GetBlock(Guid blockGuid)
        {
            if (guidToSimulationBlock.ContainsKey(blockGuid))
                return guidToSimulationBlock[blockGuid];
            throw new BlockNotFoundException("Block " + blockGuid + " not found.");
        }

        /// <summary>
        /// Returns Block handler for a given BlockBehaviour.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        internal Block GetBlock(BlockBehaviour bb)
        {
            foreach (KeyValuePair<Guid, Block> entry in guidToSimulationBlock)
                if (entry.Value.GetBlockBehaviour().Equals(bb)) return entry.Value;
            throw new BlockNotFoundException("Given BlockBehaviour has no corresponding Block handler.");
        }

        /// <summary>
        /// Finds blockId string in dictionary of simulation blocks.
        /// </summary>
        /// <param name="blockId">Block's sequential identifier.</param>
        /// <returns>Returns reference to blocks Block handler object.</returns>
        internal Block GetBlock(string blockId)
        {
            if (idToSimulationBlock.ContainsKey(blockId.ToUpper()))
                return idToSimulationBlock[blockId.ToUpper()];
            throw new BlockNotFoundException("Block " + blockId + " not found.");
        }

        /// <summary>
        /// Populates dictionary with references to building blocks.
        /// Used for dumping block IDs while building.
        /// Called at first DumpBlockID after machine change.
        /// </summary>
        internal void InitializeBuildingBlockIDs()
        {
            var typeCount = new Dictionary<string, int>();
            buildingBlocks = new Dictionary<GenericBlock, string>();
            for (int i = 0; i < Machine.Active().BuildingBlocks.Count; i++)
            {
                GenericBlock block = Machine.Active().BuildingBlocks[i].GetComponent<GenericBlock>();
                string name = Machine.Active().BuildingBlocks[i].GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                buildingBlocks[block] = name + " " + typeCount[name];
            }
            rebuildDict = false;
        }

        /// <summary>
        /// Populates dictionary with references to simulation blocks.
        /// Used for accessing blocks with GetBlock(blockId) while simulating.
        /// Called at the start of simulation.
        /// Invokes OnInitialisation event.
        /// </summary>
        internal void InitializeSimulationBlockHandlers()
        {
            idToSimulationBlock = new Dictionary<string, Block>();
            guidToSimulationBlock = new Dictionary<Guid, Block>();
            var typeCount = new Dictionary<string, int>();
            for (int i = 0; i < Machine.Active().BuildingBlocks.Count; i++)
            {
                string name = Machine.Active().BuildingBlocks[i].GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                string id = name + " " + typeCount[name];
                Guid guid = Machine.Active().BuildingBlocks[i].Guid;
                Block b = CreateBlock(Machine.Active().Blocks[i]);
                idToSimulationBlock[id] = b;
                guidToSimulationBlock[guid] = b;
            }

            handlersInitialised = true;
            BlockHandlers.OnInitialisation?.Invoke();
        }

        private void LoadScript()
        {
            try
            {
                if(scriptFile != null)
                    python.LoadScript(scriptFile);
                if (scriptCode != null)
                    python.LoadCode(scriptCode);
                ScripterMod.ScriptOptions.SuccessMessage = "Successfully executed code.";
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                ScripterMod.ScriptOptions.ErrorMessage = "Error executing code.\nSee console for more info.";
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
                return "Executes a Lua expression.";
            if (!isSimulating || python == null)
                return "Can only be called while simulating.";

            string expression = "";
            for (int i = 0; i < args.Length; i++)
                expression += args[i] + " ";

            object result = python.Engine.Execute(expression);

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
            idToSimulationBlock = null;
            python = new PythonEnvironment();

            // Find script file
            if (scriptFile == null)
            {
                ScripterMod.ScriptOptions.CheckForScript();
                if (ScripterMod.ScriptOptions.ScriptSource == "py")
                {
                    scriptFile = ScripterMod.ScriptOptions.ScriptPath;
                }
                if (ScripterMod.ScriptOptions.ScriptSource == "bsg")
                {
                    scriptCode = ScripterMod.ScriptOptions.Code;
                }
            }
        }

        /// <summary>
        /// Called to stop script.
        /// </summary>
        private void DestroyScriptingEnvironment()
        {
            Functions.ClearMarks(false);
            idToSimulationBlock = null;
            python = null;
        }

        /// <summary>
        /// Finds hovered block in buildingBlocks dictionary and dumps its ID string
        /// if LeftShift is pressed.
        /// </summary>
        private void ShowBlockIdentifiers()
        {
            if (Game.AddPiece.HoveredBlock == null)
            {
                hoveredBlock = null;
                return;
            }

            hoveredBlock = Game.AddPiece.HoveredBlock;

            if (rebuildDict || buildingBlocks == null)
                InitializeBuildingBlockIDs();

            string key;
            try
            {
                key = buildingBlocks[hoveredBlock];
            }
            catch (KeyNotFoundException)
            {
                InitializeBuildingBlockIDs();
                key = buildingBlocks[hoveredBlock];
            }
            string guid = hoveredBlock.GetComponent<BlockBehaviour>().Guid.ToString();
            ScripterMod.IdentifierDisplay.ShowBlock(hoveredBlock);
        }

        /// <summary>
        /// Mod functionality.
        /// Calls Lua functions.
        /// </summary>
        private void Update()
        {
            // Initialize 
            if (isSimulating && !handlersInitialised)
                InitializeSimulationBlockHandlers();

            // Execute code on first call
            if (scriptFile != null || scriptCode != null)
            {
                LoadScript();
                scriptFile = null;
                scriptCode = null;
            }

            // Toggle watchlist visibility
            if (Keybindings.Get("Watchlist").Pressed())
            {
                ScripterMod.Watchlist.Visible = !ScripterMod.Watchlist.Visible;
            }

            // Toggle options visibility
            if (Keybindings.Get("Script Options").Pressed())
            {
                ScripterMod.ScriptOptions.Visible = !ScripterMod.ScriptOptions.Visible;
            }

            if (!isSimulating)
            {
                // Show block identifiers
                if (Keybindings.Get("Show Blocks ID").IsDown())
                {
                    ShowBlockIdentifiers();
                }
            }

            if (!isSimulating) return;

            // Call script update.
            python?.Update?.Invoke();

            // Call OnUpdate event for Block handlers.
            OnUpdate?.Invoke();
        }

        private void LateUpdate()
        {
            // Call OnLateUpdate event for Block handlers.
            OnLateUpdate?.Invoke();
        }

        /// <summary>
        /// Calls Lua functions at a fixed rate.
        /// </summary>
        private void FixedUpdate()
        {
            if (!isSimulating) return;

            // Call script update;
            python?.FixedUpdate?.Invoke();

            // Call OnLateUpdate event for Block handlers.
            OnFixedUpdate?.Invoke();
        }

        /// <summary>
        /// Handles starting and stopping of the simulation.
        /// </summary>
        /// <param name="isSimulating"></param>
        internal void OnSimulationToggle(bool isSimulating)
        {
            handlersInitialised = false;
            this.isSimulating = isSimulating;
            if (isSimulating)
            {
                if (enableScript) CreateScriptingEnvironment();
            }
            else
            {
                DestroyScriptingEnvironment();
            }
            ScripterMod.ScriptOptions.SuccessMessage = null;
            ScripterMod.ScriptOptions.NoteMessage = null;
            ScripterMod.ScriptOptions.ErrorMessage = null;
        }
    }

}
