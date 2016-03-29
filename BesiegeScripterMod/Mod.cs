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
        public override Version Version { get; } = new Version(0, 3, 1, 0);
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

        // Map: ID -> Simulation Block
        private Dictionary<string, Transform> simulationBlocks;

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
            if (typeCount != null) typeCount.Clear();
            else typeCount = new Dictionary<string, int>();
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
            simulationBlocks = new Dictionary<string, Transform>();
            Transform simulationMachine = GameObject.Find("Simulation Machine").transform;
            foreach (Transform b in simulationMachine)
            {
                string name = b.GetComponent<MyBlockInfo>().blockName.ToUpper();

                int c = 0;
                string id;
                do
                {
                    c++;
                    id = name + " " + c;
                } while (simulationBlocks.ContainsKey(id));

                simulationBlocks[id] = b;
            }
        }

        /// <summary>
        /// Called to start script.
        /// </summary>
        private void ScriptStart()
        {
            simulationBlocks = null;

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
            foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
            {
                string str = value.ToString();
                object[] objArray = new object[] { "KeyCode[\"", str, "\"] = ", (int)value };
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

                Debug.Log(buildingBlocks[hoveredBlock]);
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
                    foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (!Input.GetKey(value)) continue;
                        this.lua.DoString(string.Concat("onKeyHeld(", (int)value, ")"), "chunk");
                    }
                }
                if ((string)this.lua.DoString("return type(onKeyDown)", "chunk")[0] == "function")
                {
                    foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (!Input.GetKeyDown(value)) continue;
                        this.lua.DoString(string.Concat("onKeyDown(", (int)value, ")"), "chunk");
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
        /// <returns>Returns reference to blocks Transform object.</returns>
        public Transform GetBlock(string blockId)
        {
            if (simulationBlocks == null) {
                InitializeSimulationBlockIDs();
            }
            return simulationBlocks[blockId.ToUpper()];
        }
    }

    /// <summary>
    /// Used as a wrapper for all Lua accessible functions.
    /// Instantiated at the start of the simulation.
    /// </summary>
    public class LuaMethodWrapper
    {
        private System.Diagnostics.Stopwatch stopwatch;

        public LuaMethodWrapper()
        {
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
        }

        private Transform GetBlock(string blockId)
        {
            return ScripterMod.scripter.GetBlock(blockId);
        }

        public void log(string msg)
        {
            Debug.Log(msg);
        }

        public long getTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Used to toggle various block properties.
        /// Does nothing if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <param name="value">Boolean value to be set.</param>
        public void setToggleMode(string blockId, string toggleName, bool value)
        {
            BlockBehaviour b = GetBlock(blockId).GetComponent<BlockBehaviour>();
            foreach (MToggle m in b.Toggles)
            {
                if(m.DisplayName.ToUpper() == toggleName.ToUpper())
                {
                    m.IsActive = value;
                    return;
                } 
            }    
        }

        /// <summary>
        /// Used to set slider value of various block properties.
        /// Does nothing if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <param name="value">Float value to be set.</param>
        public void setSliderValue(string blockId, string sliderName, float value)
        {
            BlockBehaviour b = GetBlock(blockId).GetComponent<BlockBehaviour>();
            foreach (MSlider m in b.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    m.Value = value;
                    return;
                }
            }
        }

        /// <summary>
        /// Used to get the toggle value of various block properties.
        /// Throws an exception if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <returns>Returns the toggle value of a specified property.</returns>
        public bool getToggleMode(string blockId, string toggleName)
        {
            BlockBehaviour b = GetBlock(blockId).GetComponent<BlockBehaviour>();
            foreach (MToggle m in b.Toggles)
            {
                if (m.DisplayName.ToUpper() == toggleName.ToUpper())
                {
                    return m.IsActive;
                }
            }
            throw new LuaException("Toggle " + toggleName + " not found.");
        }

        /// <summary>
        /// Used to get the slider value of various block properties.
        /// Throws an exception if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <returns>Returns the float value of a specified property.</returns>
        public float getSliderValue(string blockId, string sliderName)
        {
            BlockBehaviour b = GetBlock(blockId).GetComponent<BlockBehaviour>();
            foreach (MSlider m in b.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    return m.Value;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        /// Used to get the minimum slider value of various block properties.
        /// Throws an exception if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <returns>Returns the float value of a specified property.</returns>
        public float getSliderMin(string blockId, string sliderName)
        {
            BlockBehaviour b = GetBlock(blockId).GetComponent<BlockBehaviour>();
            foreach (MSlider m in b.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    return m.Min;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        /// Used to get the maximum slider value of various block properties.
        /// Throws an exception if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <returns>Returns the float value of a specified property.</returns>
        public float getSliderMax(string blockId, string sliderName)
        {
            BlockBehaviour b = GetBlock(blockId).GetComponent<BlockBehaviour>();
            foreach (MSlider m in b.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    return m.Max;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        /// Returns the position vector of the specified block.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getPosition(string blockId = "STARTING BLOCK 1")
        {
            return GetBlock(blockId).transform.position;
        }

        /// <summary>
        /// Returns the velocity vector of the specified block.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getVelocity(string blockId = "STARTING BLOCK 1")
        {
            return GetBlock(blockId).GetComponent<Rigidbody>().velocity;
        }

        /// <summary>
        /// Returns the euler angles vector of the specified block,
        /// respective to the blocks forward, right, up vectors.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object with values in degrees.</returns>
        public Vector3 getEulerAngles(string blockId = "STARTING BLOCK 1")
        {
            return GetBlock(blockId).transform.eulerAngles;
        }

        /// <summary>
        /// Returns the angular velocity vector of the specified block.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getAngularVelocity(string blockId = "STARTING BLOCK 1")
        {
            return GetBlock(blockId).GetComponent<Rigidbody>().angularVelocity;
        }

        /// Following angle functions are swapped in a way to fit starting blocks initial position.
        /// This means that at the start of the simulation, starting blocks angles will be 0, 0, 0.

        /// <summary>
        /// Calculates the heading of the specified block in degrees.
        /// Works the same as GetYaw.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from 0 to 360.</returns>
        public float getHeading(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = this.GetBlock(blockId).transform.rotation;
            float jaw = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * Mathf.Rad2Deg;
            return jaw < 0 ? jaw + 360 : jaw;
        }

        /// <summary>
        /// Returns directed yaw angle of the specified block in degrees.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from -180 to 180.</returns>
        public float getYaw(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = this.GetBlock(blockId).transform.rotation;
            return Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns directed pitch angle of the specified block in degrees.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from -180 to 180.</returns>
        public float getPitch(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = this.GetBlock(blockId).transform.rotation;
            return - Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Returns directed roll angle of the specified block in degrees.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from -180 to 180.</returns>
        public float getRoll(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = this.GetBlock(blockId).transform.rotation;
            return - Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w) * Mathf.Rad2Deg;
        }

        /// <summary>
        /// Uses raycast to find out where mouse cursor is pointing.
        /// If not sucessfull, returns zero vector.
        /// </summary>
        /// <returns>Returns an x, y, z positional vector of the hit.</returns>
        public Vector3 getRaycastHit()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
            return new Vector3(0, 0, 0);
        }
    }
}
