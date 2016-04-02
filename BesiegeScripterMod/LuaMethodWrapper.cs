using System.Collections.Generic;
using UnityEngine;
using NLua.Exceptions;

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

        public LuaMethodWrapper()
        {
            marks = new List<Mark>();
            stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            startTime = Time.time;
            convertToDegrees = Mathf.Rad2Deg;
            convertToRadians = 1;
        }

        /// <summary>
        /// Returns BlockBehaviour object of the specified block.
        /// </summary>
        public BlockBehaviour getBlock(string blockId)
        {
            return ScripterMod.scripter.GetBlock(blockId);
        }

        public void log(string msg)
        {
            Debug.Log(msg);
        }

        /// <summary>
        /// Reports the value of the variable to the watchlist.
        /// </summary>
        public void watch(string name, System.Object value)
        {
            ScripterMod.watchlist.AddToWatchlist(name, value, false);
        }

        /// <summary>
        /// Clears the watchlist.
        /// </summary>
        public void clearWatchlist()
        {
            ScripterMod.watchlist.ClearWatchlist();
        }

        /// <summary>
        /// Gets simulation time, independent of in-game time scaling.
        /// </summary>
        /// <returns></returns>
        public long getTime()
        {
            return stopwatch.ElapsedMilliseconds;
        }

        /// <summary>
        /// Gets simulation time, consistent with in-game time scaling.
        /// </summary>
        /// <returns></returns>
        public float getScaledTime()
        {
            return (Time.time - startTime) * 1000;
        }

        /// <summary>
        /// Makes any future calls to angle functions return degrees.
        /// </summary>
        public void useDegrees()
        {
            convertToDegrees = Mathf.Rad2Deg;
            convertToRadians = 1;
        }

        /// <summary>
        /// Makes any future calls to angle functions return radians.
        /// </summary>
        public void useRadians()
        {
            convertToDegrees = 1;
            convertToRadians = Mathf.Deg2Rad;
        }

        /// <summary>
        /// Used to toggle various block properties.
        /// Throws an exception if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <param name="value">Boolean value to be set.</param>
        public void setToggleMode(string blockId, string toggleName, bool value)
        {
            BlockBehaviour b = getBlock(blockId);
            foreach (MToggle m in b.Toggles)
            {
                if (m.DisplayName.ToUpper() == toggleName.ToUpper())
                {
                    m.IsActive = value;
                    return;
                }
            }
            throw new LuaException("Toggle " + toggleName + " not found.");
        }

        /// <summary>
        /// Used to set slider value of various block properties.
        /// Throws an exception if the property is not found.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier.</param>
        /// <param name="toggleName">Case insensitive string specifying the property to be set.
        /// Usually identical to in-game label.</param>
        /// <param name="value">Float value to be set.</param>
        public void setSliderValue(string blockId, string sliderName, float value)
        {
            if (float.IsNaN(value))
                throw new LuaException("Value is not a number (NaN).");
            BlockBehaviour b = getBlock(blockId);
            foreach (MSlider m in b.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    m.Value = value;
                    return;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
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
            BlockBehaviour b = getBlock(blockId);
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
            BlockBehaviour b = getBlock(blockId);
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
            BlockBehaviour b = getBlock(blockId);
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
            BlockBehaviour b = getBlock(blockId);
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
        /// Used to add blocks key controls.
        /// Throws an exception if the key name is not found.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <param name="keyName">Key display name.</param>
        /// <param name="key">New key to be assigned.</param>
        public void addKey(string blockId, string keyName, int keyValue)
        {
            KeyCode key = (KeyCode)keyValue;
            BlockBehaviour b = getBlock(blockId);
            foreach (MKey m in b.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    for (int i = 0; i < m.KeyCode.Count; i++)
                        if(m.KeyCode[i] == KeyCode.None)
                        {
                            m.AddOrReplaceKey(i, key);
                            return;
                        }
                    m.AddKey(key);
                    return;
                }
            }
            throw new LuaException("Key " + keyName + " not found.");
        }

        /// <summary>
        /// Used to replace blocks key controls.
        /// Throws an exception if the key name is not found.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <param name="keyName">Key display name.</param>
        /// <param name="key">New key to be assigned.</param>
        public void replaceKey(string blockId, string keyName, int keyValue)
        {
            KeyCode key = (KeyCode)keyValue;
            BlockBehaviour b = getBlock(blockId);
            foreach (MKey m in b.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    m.AddOrReplaceKey(0, key);
                }
            }
            throw new LuaException("Key " + keyName + " not found.");
        }

        /// <summary>
        /// Used to replaced blocks key controls.
        /// Throws an exception if the key name is not found.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <param name="keyName">Key display name.</param>
        public int getKey(string blockId, string keyName)
        {
            BlockBehaviour b = getBlock(blockId);
            foreach (MKey m in b.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    return (int)m.KeyCode[0];
                }
            }
            throw new LuaException("Key " + keyName + " not found.");
        }

        /// <summary>
        /// Used to remove all of the blocks key controls.
        /// Throws an exception if the key name is not found.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <param name="keyName">Key display name.</param>
        /// <param name="key">New key to be assigned.</param>
        public void clearKeys(string blockId, string keyName)
        {
            BlockBehaviour b = getBlock(blockId);
            foreach (MKey m in b.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    for (int i = 0; i < m.KeyCode.Count; i++)
                        m.AddOrReplaceKey(i, KeyCode.None);
                    return;
                }
            }
            throw new LuaException("Key " + keyName + " not found.");
        }

        /// <summary>
        /// Returns the position vector of the specified block.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getPosition(string blockId = "STARTING BLOCK 1")
        {
            return getBlock(blockId).transform.position;
        }

        /// <summary>
        /// Returns the velocity vector of the specified block
        /// in units per second.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getVelocity(string blockId = "STARTING BLOCK 1")
        {
            return getBlock(blockId).GetComponent<Rigidbody>().velocity;
        }

        /// <summary>
        /// Returns the euler angles vector of the specified block,
        /// respective to the blocks forward, right, up vectors.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getEulerAngles(string blockId = "STARTING BLOCK 1")
        {
            Vector3 d2r = new Vector3(convertToRadians, convertToRadians, convertToRadians);
            Vector3 euler = getBlock(blockId).transform.eulerAngles;
            euler.Scale(d2r);
            return euler;
        }

        /// <summary>
        /// Returns the angular velocity vector of the specified block.
        /// If no argument is used, starting block is used.
        /// </summary>
        /// <param name="blockId">Block identifier.</param>
        /// <returns>Vector3 object.</returns>
        public Vector3 getAngularVelocity(string blockId = "STARTING BLOCK 1")
        {
            Vector3 r2d = new Vector3(convertToDegrees, convertToDegrees, convertToDegrees);
            Vector3 angularVelocity = getBlock(blockId).GetComponent<Rigidbody>().angularVelocity;
            angularVelocity.Scale(r2d);
            return angularVelocity;
        }

        /// Following angle functions are swapped in a way to fit starting blocks initial position.
        /// This means that at the start of the simulation, starting blocks angles will be 0, 0, 0.

        /// <summary>
        /// Calculates the heading of the specified block.
        /// Works the same as GetYaw.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from 0 to 360 or 2*PI.</returns>
        public float getHeading(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = getBlock(blockId).transform.rotation;
            float jaw = Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * convertToDegrees;
            return jaw < 0 ? jaw + 2 * Mathf.PI * convertToDegrees : jaw;
        }

        /// <summary>
        /// Returns directed yaw angle of the specified block in degrees.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from -180 to 180.</returns>
        public float getYaw(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = getBlock(blockId).transform.rotation;
            return Mathf.Atan2(2 * q.y * q.w - 2 * q.x * q.z, 1 - 2 * q.y * q.y - 2 * q.z * q.z) * convertToDegrees;
        }

        /// <summary>
        /// Returns directed pitch angle of the specified block in degrees.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from -180 to 180.</returns>
        public float getPitch(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = getBlock(blockId).transform.rotation;
            return -Mathf.Atan2(2 * q.x * q.w - 2 * q.y * q.z, 1 - 2 * q.x * q.x - 2 * q.z * q.z) * convertToDegrees;
        }

        /// <summary>
        /// Returns directed roll angle of the specified block in degrees.
        /// </summary>
        /// <param name="blockId">Blocks unique identifier. Default is starting block.</param>
        /// <returns>Float value ranging from -180 to 180.</returns>
        public float getRoll(string blockId = "STARTING BLOCK 1")
        {
            Quaternion q = getBlock(blockId).transform.rotation;
            return -Mathf.Asin(2 * q.x * q.y + 2 * q.z * q.w) * Mathf.Rad2Deg;
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

        public void move(Vector3 target)
        {
            transform.position = target;
        }

        public void setColor(Color c)
        {
            GetComponent<Renderer>().material.color = c;
        }

        public void clear()
        {
            Destroy(gameObject);
            Destroy(this);
        }
    }
}
