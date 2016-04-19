using System.Collections.Generic;
using UnityEngine;
using NLua.Exceptions;
using LenchScripterMod.Blocks;

namespace LenchScripterMod
{

    /// <summary>
    /// Used as a wrapper for all Lua accessible functions.
    /// Instantiated at the start of the simulation.
    /// </summary>
    public class LuaMethodWrapper
    {
        // Using radians or degrees
        private float convertToDegrees;
        private float convertToRadians;

        // Measuring time
        private System.Diagnostics.Stopwatch stopwatch;
        private float startTime;

        // List of placed marks
        private List<Mark> marks;

        /// <summary>
        /// Instantiates the interface that is passed to Lua as besiege object.
        /// </summary>
        internal LuaMethodWrapper()
        {
            marks = new List<Mark>();
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            startTime = Time.time;
            convertToDegrees = Mathf.Rad2Deg;
            convertToRadians = 1;
        }
  

        public Block getBlock(string blockId)
        {
            return ScripterMod.scripter.GetBlock(blockId);
        }

        public bool exists(string blockId)
        {
            Block b = getBlock(blockId);
            return b.exists();
        }

        public void log(string msg)
        {
            Debug.Log(msg);
        }

        public long getTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }

        public float getScaledTime()
        {
            return (Time.time - startTime) * 1000;
        }

        public void watch(string name, System.Object value)
        {
            ScripterMod.watchlist.AddToWatchlist(name, value, false);
        }

        public void clearWatchlist()
        {
            ScripterMod.watchlist.ClearWatchlist();
        }

        public void useDegrees()
        {
            Block.useDegrees();
            convertToDegrees = Mathf.Rad2Deg;
            convertToRadians = 1;
        }

        public void useRadians()
        {
            Block.useRadians();
            convertToDegrees = 1;
            convertToRadians = Mathf.Deg2Rad;
        }

        public void action(string blockId, string actionName)
        {
            Block b = getBlock(blockId);
            b.action(actionName);
        }

        public void setToggleMode(string blockId, string toggleName, bool value)
        {
            Block b = getBlock(blockId);
            b.setToggleMode(toggleName, value);
        }

        public void setSliderValue(string blockId, string sliderName, float value)
        {
            Block b = getBlock(blockId);
            b.setSliderValue(sliderName, value);
        }

        public void setLimitValue(string blockId, string limitName, float value)
        {
            Block b = getBlock(blockId);
            // TODO!
        }

        public bool getToggleMode(string blockId, string toggleName)
        {
            Block b = getBlock(blockId);
            return b.getToggleMode(toggleName);
        }

        public float getSliderValue(string blockId, string sliderName)
        {
            Block b = getBlock(blockId);
            return b.getSliderValue(sliderName);
        }

        public float getLimitValue(string blockId, string limitName)
        {
            Block b = getBlock(blockId);
            // TODO!
            return 0;
        }

        public float getSliderMin(string blockId, string sliderName)
        {
            Block b = getBlock(blockId);
            return b.getSliderMin(sliderName);
        }

        public float getSliderMax(string blockId, string sliderName)
        {
            Block b = getBlock(blockId);
            return b.getSliderMax(sliderName);
        }

        public void addKey(string blockId, string keyName, int keyValue)
        {
            Block b = getBlock(blockId);
            b.addKey(keyName, keyValue);
        }

        public void replaceKey(string blockId, string keyName, int keyValue)
        {
            Block b = getBlock(blockId);
            b.replaceKey(keyName, keyValue);
        }

        public int getKey(string blockId, string keyName)
        {
            Block b = getBlock(blockId);
            return b.getKey(keyName);
        }

        public void clearKeys(string blockId, string keyName)
        {
            Block b = getBlock(blockId);
            b.clearKeys(keyName);
        }

        public Vector3 getForward(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getForward();
        }

        public Vector3 getUp(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getUp();
        }

        public Vector3 getRight(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getRight();
        }

        public Vector3 getPosition(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getPosition();
        }

        public Vector3 getVelocity(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getVelocity();
        }

        public float getMass(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getMass();
        }

        public Vector3 getCenterOfMass(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getCenterOfMass();
        }

        /// <summary>
        /// Returns the mass of the machine.
        /// </summary>
        /// <returns>Float value representing total mass.</returns>
        public float getMachineMass()
        {
            return Machine.Active().Mass;
        }

        /// <summary>
        /// Returns the center of mass of the machine in the world.
        /// </summary>
        /// <returns>Vector3 position of world COM.</returns>
        public Vector3 getMachineCenterOfMass()
        {
            Vector3 center = new Vector3(0, 0, 0);
            for (int i = 0; i < Machine.Active().Blocks.Count; i++)
            {
                Rigidbody body = Machine.Active().Blocks[i].GetComponent<Rigidbody>();
                if(body != null)
                    center += body.worldCenterOfMass * body.mass;
            }
            return center / Machine.Active().Mass;
        }

        public Vector3 getEulerAngles(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getEulerAngles();
        }

        public Vector3 getAngularVelocity(string blockId = "STARTING BLOCK 1")
        {
            Block b = getBlock(blockId);
            return b.getAngularVelocity();
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
            throw new LuaException("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        /// Creates a mark at a given position.
        /// </summary>
        /// <param name="pos">Vector3 specifying position.</param>
        /// <returns>Reference to the mark.</returns>
        public Mark createMark(Vector3 pos)
        {
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Mark m = obj.AddComponent<Mark>();
            m.move(pos);
            marks.Add(m);
            return m;
        }

        /// <summary>
        /// Clears all marks.
        /// Called by user or at the end of the simulation.
        /// </summary>
        public void clearMarks()
        {
            foreach (Mark m in marks)
            {
                m.clear();
            }
            marks.Clear();
        }
    }

    /// <summary>
    /// Mark script attached to primitive sphere objects.
    /// Used to mark locations through Lua script.
    /// </summary>
    public class Mark : MonoBehaviour
    {

        void Start()
        {
            GetComponent<Renderer>().material.color = Color.red;
            Destroy(GetComponent<SphereCollider>());
        }

        /// <summary>
        /// Moves the mark to the target position.
        /// </summary>
        /// <param name="target">Vector3 target position.</param>
        public void move(Vector3 target)
        {
            transform.position = target;
        }

        /// <summary>
        /// Sets the color of the mark.
        /// </summary>
        /// <param name="c">UnityEngine.Color object.</param>
        public void setColor(Color c)
        {
            GetComponent<Renderer>().material.color = c;
        }

        /// <summary>
        /// Clears the mark.
        /// </summary>
        public void clear()
        {
            Destroy(gameObject);
            Destroy(this);
        }
    }

}
