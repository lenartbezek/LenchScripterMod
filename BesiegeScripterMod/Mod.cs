using System;
using System.IO;
using System.Collections.Generic;
using spaar.ModLoader;
using UnityEngine;
using NLua;
using NLua.Exceptions;

namespace LenchScripterMod
{
    public class ScripterMod : Mod
    {
        public override string Name { get; } = "Lench Scripter Mod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "Lench";
        public override Version Version { get; } = new Version(0, 4, 1, 0);
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.27";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;

        public static Scripter scripter;

        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Scripter.Instance);
            scripter = Scripter.Instance;
            Game.OnSimulationToggle += scripter.OnSimulationToggle;
        }
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= scripter.OnSimulationToggle;
            scripter.OnSimulationToggle(false);
        }
    }

    public class Scripter : SingleInstance<Scripter>
    {
        public override string Name { get; } = "LenchScripter";
        public bool isSimulating;

        private static LuaMethodWrapper wrapper;

        private Lua lua;
        private string luaFile;
        private GenericBlock hoveredBlock;

        // Map: Block type -> type count
        private Dictionary<string, int> typeCount;

        // Map: Building Block -> ID
        private Dictionary<GenericBlock, string> buildingBlocks;

        // Map: GUID -> Simulation Block
        private Dictionary<string, BlockBehaviour> guidToSimulationBlock;

        // Map: ID -> Simulation Block
        private Dictionary<string, BlockBehaviour> idToSimulationBlock;

        /// <summary>
        /// Rebuilds ID dictionary.
        /// </summary>
        private void AddBlockID(Transform block)
        {
            InitializeBuildingBlockIDs();
        }

        /// <summary>
        /// Populates dictionary with references to building blocks.
        /// Used for dumping block IDs while building.
        /// </summary>
        private void InitializeBuildingBlockIDs()
        {
            typeCount = new Dictionary<string, int>();
            if (buildingBlocks != null) buildingBlocks.Clear();
            else
            {
                Game.OnBlockPlaced += AddBlockID;
                Game.OnBlockRemoved += InitializeBuildingBlockIDs;
                buildingBlocks = new Dictionary<GenericBlock, string>();
            }
            Transform buildingMachine = GameObject.Find("Building Machine").transform;
            foreach (Transform b in buildingMachine)
            {
                GenericBlock block = b.GetComponent<GenericBlock>();
                string name = block.GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                buildingBlocks[block] = name + " " + typeCount[name];
            }
        }

        /// <summary>
        /// Populates dictionary with references to simulation blocks.
        /// Used for accessing blocks with GetBlock(blockId) while simulating.
        /// Called at first call of GetBlock(blockId);
        /// </summary>
        private void InitializeSimulationBlockIDs()
        {
            idToSimulationBlock = new Dictionary<string, BlockBehaviour>();
            guidToSimulationBlock = new Dictionary<string, BlockBehaviour>();
            Transform simulationMachine = GameObject.Find("Simulation Machine").transform;
            for (int i = 0; i < Machine.Active().BuildingBlocks.Count; i++)
            {
                string guid = Machine.Active().BuildingBlocks[i].Guid.ToString();
                guidToSimulationBlock[guid] = Machine.Active().Blocks[i];
            }
            foreach (Transform b in simulationMachine)
            {
                string name = b.GetComponent<MyBlockInfo>().blockName.ToUpper();

                int c = 0;
                string id;
                do
                {
                    c++;
                    id = name + " " + c;
                } while (idToSimulationBlock.ContainsKey(id));

                idToSimulationBlock[id] = b.GetComponent<BlockBehaviour>();
            }
        }

        /// <summary>
        /// Called to start script.
        /// </summary>
        private void ScriptStart()
        {
            idToSimulationBlock = null;

            // Lua Environment
            lua = new Lua();
            lua.LoadCLRPackage();
            lua.DoString(@" import 'System'
                            import 'UnityEngine'
                            import 'Assembly-CSharp'  ");

            wrapper = new LuaMethodWrapper();
            lua["besiege"] = wrapper;

            // Populate keycode table
            this.lua.NewTable("KeyCode");
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                string str = key.ToString();
                object[] objArray = new object[] { "KeyCode[\"", str, "\"] = ", (int)key };
                lua.DoString(string.Concat(objArray), "chunk");
            }

            // Find script file
            string luaFile = string.Concat(Application.dataPath, "/Scripts/", MyTextField.lastNameUsed, ".lua");
            if (File.Exists(luaFile))
            {
                UnityEngine.Debug.Log("Script file: " + luaFile);
                this.luaFile = luaFile;
            }
            else
            {
                Debug.Log("Script file does not exist: " + luaFile);
                this.ScriptStop();
            }

        }

        /// <summary>
        /// Called to stop script.
        /// </summary>
        private void ScriptStop()
        {
            this.lua.Close();
            this.lua.Dispose();
            this.lua = null;
            wrapper.clearMarks();
            wrapper = null;
            Debug.Log("Script stopped");
        }

        /// <summary>
        /// Finds hovered block in buildingBlocks dictionary and dumps its ID string
        /// if LeftShift is pressed.
        /// </summary>
        private void DumpHoveredBlock()
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (Game.AddPiece.HoveredBlock == null)
                {
                    this.hoveredBlock = null;
                    return;
                }

                if (this.hoveredBlock != null && Game.AddPiece.HoveredBlock == this.hoveredBlock)
                    return;

                this.hoveredBlock = Game.AddPiece.HoveredBlock;

                if (buildingBlocks == null)
                    InitializeBuildingBlockIDs();

                string key = buildingBlocks[hoveredBlock];
                string guid = hoveredBlock.GetComponent<BlockBehaviour>().Guid.ToString();
                Debug.Log(key + " - " + guid);
            }
        }

        /// <summary>
        /// Calls Lua functions.
        /// </summary>
        private void Update()
        {
            if (!this.isSimulating)
            {
                this.DumpHoveredBlock();
                return;
            }

            if (!AddPiece.isSimulating) return;
            if (this.lua == null) return;

            if (this.luaFile != null)
            {   // load the script on the first update
                this.lua.DoFile(this.luaFile);
                this.luaFile = null;
            }

            if ((string)this.lua.DoString("return type(onUpdate)", "chunk")[0] == "function")
                this.lua.DoString("onUpdate()", "chunk");

            if (Input.anyKey)
            {
                if ((string)this.lua.DoString("return type(onKeyHeld)", "chunk")[0] == "function")
                {
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (!Input.GetKey(key)) continue;
                        this.lua.DoString(string.Concat("onKeyHeld(", (int)key, ")"), "chunk");
                    }
                }
                if ((string)this.lua.DoString("return type(onKeyDown)", "chunk")[0] == "function")
                {
                    foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (!Input.GetKeyDown(key)) continue;
                        this.lua.DoString(string.Concat("onKeyDown(", (int)key, ")"), "chunk");
                    }
                }
            }
        }

        /// <summary>
        /// Handles starting and stopping of the simulation.
        /// </summary>
        /// <param name="isSimulating"></param>
        public void OnSimulationToggle(bool isSimulating)
        {
            this.isSimulating = isSimulating;
            if (isSimulating)
                this.ScriptStart();
            else if (this.lua != null)
                this.ScriptStop();
        }

        /// <summary>
        /// Finds blockId string in dictionary of simulation blocks.
        /// On first call of the simulation, it also initializes the dictionary.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <returns>Returns reference to blocks BlockBehaviour object.</returns>
        public BlockBehaviour GetBlock(string blockId)
        {
            if (idToSimulationBlock == null)
                InitializeSimulationBlockIDs();

            if (idToSimulationBlock.ContainsKey(blockId.ToUpper()))
                return idToSimulationBlock[blockId.ToUpper()];
            if (guidToSimulationBlock.ContainsKey(blockId))
                return guidToSimulationBlock[blockId];
            throw new LuaException("Block " + blockId + " not found.");
        }
    }
}
