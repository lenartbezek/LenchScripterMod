using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
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
        public override Version Version { get; } = new Version(0, 7, 1);
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.27";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;

        /// <summary>
        /// SingleInstance of scripter mod.
        /// </summary>
        public static Scripter scripter;

        internal static LuaWatchlist watchlist;
        internal static Type blockScriptType;

        /// <summary>
        /// Instantiates the mod and it's components.
        /// Looks for and loads assemblies.
        /// </summary>
        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Scripter.Instance);
            scripter = Scripter.Instance;
            Game.OnSimulationToggle += scripter.OnSimulationToggle;

            watchlist = new LuaWatchlist();

            if (LoadBlockLoaderAssembly())
            {
                Debug.Log("[Lench Scripter Mod]: Found TGYD's BlockLoader");
            }

            addKeybinds();
        }

        /// <summary>
        /// Disables the mod from executing scripts.
        /// </summary>
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= scripter.OnSimulationToggle;
            scripter.OnSimulationToggle(false);
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

        private void addKeybinds()
        {
            Keybindings.AddKeybinding("Dump Blocks ID", new Key(KeyCode.None, KeyCode.LeftShift));
            Keybindings.AddKeybinding("Lua Watchlist", new Key(KeyCode.LeftControl, KeyCode.I));
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

        // Object passed to lua
        private static LuaMethodWrapper wrapper;

        internal bool isSimulating;

        // Blocks to remove actionKey in next frame.
        internal List<BlockBehaviour> activatedBlocks = new List<BlockBehaviour>();
        internal bool sendActionKey = false;

        // Lua environment
        internal Lua lua;
        internal string luaFile;

        // Hovered block for ID dumping
        private GenericBlock hoveredBlock;

        // Machine changed - flag for rebuild
        private bool rebuildDict = false;

        // Map: Building Block -> ID
        private Dictionary<GenericBlock, string> buildingBlocks;

        // Map: GUID -> Simulation Block
        private Dictionary<string, BlockBehaviour> guidToSimulationBlock;

        // Map: ID -> Simulation Block
        private Dictionary<string, BlockBehaviour> idToSimulationBlock;

        /// <summary>
        /// Populates dictionary with references to building blocks.
        /// Used for dumping block IDs while building.
        /// Called at first DumpBlockID after machine change.
        /// </summary>
        internal void InitializeBuildingBlockIDs()
        {
            var typeCount = new Dictionary<string, int>();
            if (buildingBlocks == null)
            {
                Game.OnBlockPlaced += (Transform block) => rebuildDict = true;
                Game.OnBlockRemoved += () => rebuildDict = true;
            }
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
        /// </summary>
        private void InitializeSimulationBlockIDs()
        {
            idToSimulationBlock = new Dictionary<string, BlockBehaviour>();
            guidToSimulationBlock = new Dictionary<string, BlockBehaviour>();
            var typeCount = new Dictionary<string, int>();
            for (int i = 0; i < Machine.Active().BuildingBlocks.Count; i++)
            {
                string name = Machine.Active().BuildingBlocks[i].GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                string id = name + " " + typeCount[name];
                string guid = Machine.Active().BuildingBlocks[i].Guid.ToString();
                idToSimulationBlock[id] = Machine.Active().Blocks[i];
                guidToSimulationBlock[guid] = Machine.Active().Blocks[i];
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
            lua.DoString(@"package.path = package.path .. ';"+ Application.dataPath + "/Scripts/?.lua'");

            wrapper = new LuaMethodWrapper();
            lua["besiege"] = wrapper;

            // Populate keycode table
            lua.NewTable("KeyCode");
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
                Debug.Log("Script file: " + luaFile);
                this.luaFile = luaFile;
            }
            else
            {
                Debug.Log("Script file does not exist: " + luaFile);
                ScriptStop();
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
            if (Keybindings.Get("Dump Blocks ID").IsDown())
            {
                if (Game.AddPiece.HoveredBlock == null)
                {
                    this.hoveredBlock = null;
                    return;
                }

                if (this.hoveredBlock != null && Game.AddPiece.HoveredBlock == this.hoveredBlock)
                    return;

                this.hoveredBlock = Game.AddPiece.HoveredBlock;

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
                Debug.Log(key + "  -  " + guid);

            }
        }

        /// <summary>
        /// To be called after blocks respond to action.
        /// </summary>
        internal void clearActionKeys()
        {
            foreach (BlockBehaviour b in activatedBlocks)
            {
                foreach (MKey m in b.Keys)
                {
                    for (int i = 0; i < m.KeyCode.Count; i++)
                        if (m.KeyCode[i] == InputManager.actionKeyCode)
                        {
                            m.AddOrReplaceKey(i, KeyCode.None);
                        }
                }
            }
            activatedBlocks.Clear();
        }

        /// <summary>
        /// Runs script while simulating.
        /// </summary>
        private void Update()
        {
            // Toggle watchlist visibility
            if (Keybindings.Get("Lua Watchlist").Pressed())
                ScripterMod.watchlist.visible = !ScripterMod.watchlist.visible;

            if (isSimulating)
            {
                if (lua == null) return;

                // Load the script on the first update
                if (luaFile != null)
                {
                    lua.DoFile(luaFile);
                    luaFile = null;
                }

                // Send action keys
                if (sendActionKey)
                {
                    try
                    {
                        InputManager.SendKeyDown(VirtualKeyCode.ACTION_KEY_CODE);
                        InputManager.SendKeyUp(VirtualKeyCode.ACTION_KEY_CODE);
                    }
                    catch (DllNotFoundException)
                    {
                        Debug.LogError("Calling block actions is not supported on your system.");
                        LuaMethodWrapper.actionCallsEnabled = false;
                    }
                    clearActionKeys();
                    sendActionKey = false;
                }

                // Call Lua onUpdate
                if ((string)lua.DoString("return type(onUpdate)", "chunk")[0] == "function")
                    lua.DoString("onUpdate()", "chunk");

                // Call Lua onKey
                if (Input.anyKey)
                {
                    if ((string)lua.DoString("return type(onKeyHeld)", "chunk")[0] == "function")
                    {
                        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                        {
                            if (!Input.GetKey(key) || key == InputManager.actionKeyCode) continue;
                            lua.DoString(string.Concat("onKeyHeld(", (int)key, ")"), "chunk");
                        }
                    }

                    if ((string)lua.DoString("return type(onKeyDown)", "chunk")[0] == "function")
                    {
                        foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
                        {
                            if (!Input.GetKeyDown(key) || key == InputManager.actionKeyCode) continue;
                            lua.DoString(string.Concat("onKeyDown(", (int)key, ")"), "chunk");
                        }
                    }
                }
            }
            else
            {
                DumpHoveredBlock();
            }
        }

        /// <summary>
        /// Calls the editor GUI to render.
        /// </summary>
        private void OnGUI()
        {
            ScripterMod.watchlist.Render();
        }

        /// <summary>
        /// Handles starting and stopping of the simulation.
        /// </summary>
        /// <param name="isSimulating"></param>
        internal void OnSimulationToggle(bool isSimulating)
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
        internal BlockBehaviour GetBlock(string blockId)
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
