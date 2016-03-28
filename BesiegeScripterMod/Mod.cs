using System;
using System.IO;
using System.Collections.Generic;
using spaar.ModLoader;
using UnityEngine;
using NLua;

namespace BesiegeScripterMod
{
    public class ScripterLoader : Mod
    {
        public override string Name { get; } = "Lench Scripter Mod";
        public override string DisplayName { get; } = "Lench Scripter Mod";
        public override string Author { get; } = "lench";
        public override Version Version { get; } = new Version(0, 5, 0, 0);
        public override string VersionExtra { get; } = "";
        public override string BesiegeVersion { get; } = "v0.27";
        public override bool CanBeUnloaded { get; } = true;
        public override bool Preload { get; } = false;

        private Scripter scripter;

        public override void OnLoad()
        {
            UnityEngine.Object.DontDestroyOnLoad(Scripter.Instance);
            this.scripter = Scripter.Instance;

            Game.OnSimulationToggle += this.scripter.OnSimulationToggle;
        }
        public override void OnUnload()
        {
            Game.OnSimulationToggle -= this.scripter.OnSimulationToggle;
            this.scripter.isSimulating = false;
            this.scripter.ScriptStop();
        }
    }

    public class Scripter : SingleInstance<Scripter>
    {
        public override string Name { get; } = "LenchScripter";
        public bool isSimulating;

        private Lua lua;
        private string luaFile;
        private GenericBlock hoveredBlock;

        // Dictionaries with references to building and simulating blocks.
        private Dictionary<string, Transform> buildingBlocks;
        private Dictionary<string, Transform> simulationBlocks;

        private void AddBlockID(Transform block)
        {
            /* Adds block reference to the buildingBlocks dictionary. */
            string name = block.GetComponent<MyBlockInfo>().blockName;
            int typeCount = 0;
            string id;
            do
            {
                typeCount++;
                id = name + " " + typeCount;
            } while (buildingBlocks.ContainsKey(id));

            buildingBlocks[id] = block;
        }

        public void InitializeBuildingBlockIDs()
        {
            /* Populates dictionary with references to building blocks.
               Used for dumping block IDs while building. */
            if (buildingBlocks != null) buildingBlocks.Clear();
            else
            {
                Game.OnBlockPlaced += AddBlockID;
                Game.OnBlockRemoved += InitializeBuildingBlockIDs;
                buildingBlocks = new Dictionary<string, Transform>();
            }

            Transform buildingMachine = GameObject.Find("Building Machine").transform;
            foreach (Transform b in buildingMachine)
            {
                AddBlockID(b);
            }
        }

        public void InitializeSimulationBlockIDs()
        {
            /* Populates dictionary with references to simulation blocks.
               Used for accessing blocks with GetBlock(id) function while simulating. */
            simulationBlocks = new Dictionary<string, Transform>();
            Transform simulationMachine = GameObject.Find("Simulation Machine").transform;
            foreach (Transform b in simulationMachine)
            {
                string name = b.GetComponent<MyBlockInfo>().blockName;

                int typeCount = 0;
                string id;
                do
                {
                    typeCount++;
                    id = name + " " + typeCount;
                } while (simulationBlocks.ContainsKey(id));

                simulationBlocks[id] = b;
            }
        }

        public void ScriptStart()
        {
            simulationBlocks = null;
            this.lua = new Lua();

            // Populate function table
            this.lua.NewTable("besiege");
            this.lua.RegisterFunction("besiege.log", this, typeof(Scripter).GetMethod("Log"));
            this.lua.RegisterFunction("besiege.setToggleMode", this, typeof(Scripter).GetMethod("SetToggleMode"));
            this.lua.RegisterFunction("besiege.setSliderValue", this, typeof(Scripter).GetMethod("SetSliderValue"));
            this.lua.RegisterFunction("besiege.setSliderMin", this, typeof(Scripter).GetMethod("SetSliderMin"));
            this.lua.RegisterFunction("besiege.setSliderMax", this, typeof(Scripter).GetMethod("SetSliderMax"));

            this.lua.RegisterFunction("besiege.getToggleMode", this, typeof(Scripter).GetMethod("GetToggleMode"));
            this.lua.RegisterFunction("besiege.getSliderValue", this, typeof(Scripter).GetMethod("GetSliderValue"));
            this.lua.RegisterFunction("besiege.getSliderMin", this, typeof(Scripter).GetMethod("GetSliderMin"));
            this.lua.RegisterFunction("besiege.getSliderMax", this, typeof(Scripter).GetMethod("GetSliderMax"));

            this.lua.RegisterFunction("besiege.getPositionX", this, typeof(Scripter).GetMethod("GetPositionX"));
            this.lua.RegisterFunction("besiege.getPositionY", this, typeof(Scripter).GetMethod("GetPositionY"));
            this.lua.RegisterFunction("besiege.getPositionZ", this, typeof(Scripter).GetMethod("GetPositionZ"));

            this.lua.RegisterFunction("besiege.getHeading", this, typeof(Scripter).GetMethod("GetHeading"));
            this.lua.RegisterFunction("besiege.getPitch", this, typeof(Scripter).GetMethod("GetPitch"));
            this.lua.RegisterFunction("besiege.getRoll", this, typeof(Scripter).GetMethod("GetRoll"));
            this.lua.RegisterFunction("besiege.getYaw", this, typeof(Scripter).GetMethod("GetYaw"));

            
            // Populate keycode table
            this.lua.NewTable("besiege.keyCodes");
            foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
            {
                string str = value.ToString();
                object[] objArray = new object[] { "besiege.keyCodes[\"", str, "\"] = ", (int)value };
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
                this.ScriptStop();
            }

        }

        public void ScriptStop()
        {
            this.lua.Close();
            this.lua.Dispose();
            this.lua = null;
            Debug.Log("Script stopped");
        }

        private void DumpHoveredBlock()
        {
            /* Finds hovered block in buildingBlocks dictionary and dumps its ID. */
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
            
            foreach (KeyValuePair<string, Transform> entry in buildingBlocks)
            {
                if (entry.Value.GetComponent<GenericBlock>() == this.hoveredBlock)
                    Debug.Log(entry.Key);
            }
        }

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

            if ((string)this.lua.DoString("return type(besiege.onUpdate)", "chunk")[0] == "function")
                this.lua.DoString("besiege:onUpdate()", "chunk");

            if (Input.anyKey)
            {
                if ((string)this.lua.DoString("return type(besiege.onKeyHeld)", "chunk")[0] == "function")
                {
                    foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (!Input.GetKey(value)) continue;
                        this.lua.DoString(string.Concat("besiege:onKeyHeld(", (int)value, ")"), "chunk");
                    }
                }
                if ((string)this.lua.DoString("return type(besiege.onKeyDown)", "chunk")[0] == "function")
                {
                    foreach (KeyCode value in Enum.GetValues(typeof(KeyCode)))
                    {
                        if (!Input.GetKeyDown(value)) continue;
                        this.lua.DoString(string.Concat("besiege:onKeyDown(", (int)value, ")"), "chunk");
                    }
                }
            }
        }

        public void OnSimulationToggle(bool isSimulating)
        {
            this.isSimulating = isSimulating;
            if (isSimulating)
                this.ScriptStart();
            else if (this.lua != null)
                this.ScriptStop();
        }

        // Internal lua methods
        private Transform GetBlock(string id)
        {
            /* Returns block identifier.
               Initializes block dictionary on the first call. */
            if (simulationBlocks == null) InitializeSimulationBlockIDs();
            return simulationBlocks[id];
        }

        // Exposed lua methods
        public void Log(string msg)
        {
            Debug.Log(msg);
        }

        public void SetToggleMode(string id, bool value)
        {
            
        }

        public void SetSliderValue(string id, float value)
        {
            
        }

        public void SetSliderMin(string id, float value)
        {
            
        }

        public void SetSliderMax(string id, float value)
        {
            
        }

        public bool GetToggleMode(string id)
        {
            return true;
        }

        public float GetSliderValue(string id)
        {
            return 0;
        }

        public float GetSliderMin(string id)
        {
            return 0;
        }

        public float GetSliderMax(string id)
        {
            return 0;
        }


        public float GetPositionX(string id)
        {
            return this.GetBlock(id).transform.position.x;
        }
        public float GetPositionY(string id)
        {
            return this.GetBlock(id).transform.position.y;
        }
        public float GetPositionZ(string id)
        {
            return this.GetBlock(id).transform.position.z;
        }

        public float GetHeading(string id)
        {
            Quaternion q = this.GetBlock(id).transform.rotation;
            float jaw = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * Mathf.Rad2Deg;
            return jaw < 0 ? jaw + 360 : jaw;
        }

        public float GetYaw(string id)
        {
            Quaternion q = this.GetBlock(id).transform.rotation;
            return Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * Mathf.Rad2Deg;
        }

        public float GetPitch(string id)
        {
            Quaternion q = this.GetBlock(id).transform.rotation;
            return - Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z) * Mathf.Rad2Deg;
        }

        public float GetRoll(string id)
        {
            Quaternion q = this.GetBlock(id).transform.rotation;
            return - Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w) * Mathf.Rad2Deg;
        }
    }
}
