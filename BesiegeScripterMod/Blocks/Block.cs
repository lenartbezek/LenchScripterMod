using NLua.Exceptions;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    public class Block
    {
        private static float convertToDegrees = Mathf.Rad2Deg;
        private static float convertToRadians = 1;

        public string blockName;

        private BlockBehaviour bb;
        private System.Object bs;

        internal Block(BlockBehaviour bb)
        {
            this.bb = bb;
            this.bs = bb.GetComponent(ScripterMod.blockScriptType);
            this.blockName = bb.GetComponent<MyBlockInfo>().blockName;
        }

        public virtual void action(string actionName) {
        }

        public bool exists()
        {
            return bb.GetComponent<Rigidbody>() != null;
        }

        public void setToggleMode(string toggleName, bool value)
        {
            foreach (MToggle m in bb.Toggles)
            {
                if (m.DisplayName.ToUpper() == toggleName.ToUpper())
                {
                    m.IsActive = value;
                    return;
                }
            }
            throw new LuaException("Toggle " + toggleName + " not found.");
        }

        public void setSliderValue(string sliderName, float value)
        {
            if (float.IsNaN(value))
                throw new LuaException("Value is not a number (NaN).");
            foreach (MSlider m in bb.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    m.Value = value;
                    return;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        public bool getToggleMode(string toggleName)
        {
            foreach (MToggle m in bb.Toggles)
            {
                if (m.DisplayName.ToUpper() == toggleName.ToUpper())
                {
                    return m.IsActive;
                }
            }
            throw new LuaException("Toggle " + toggleName + " not found.");
        }

        public float getSliderValue(string sliderName)
        {
            foreach (MSlider m in bb.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    return m.Value;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        public float getSliderMin(string sliderName)
        {
            foreach (MSlider m in bb.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    return m.Min;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        public float getSliderMax(string sliderName)
        {
            foreach (MSlider m in bb.Sliders)
            {
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    return m.Max;
                }
            }
            throw new LuaException("Slider " + sliderName + " not found.");
        }

        public void addKey(string keyName, int keyValue)
        {
            KeyCode key = (KeyCode)keyValue;
            foreach (MKey m in bb.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    for (int i = 0; i < m.KeyCode.Count; i++)
                        if (m.KeyCode[i] == KeyCode.None)
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

        public void replaceKey(string keyName, int keyValue)
        {
            KeyCode key = (KeyCode)keyValue;
            foreach (MKey m in bb.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    m.AddOrReplaceKey(0, key);
                }
            }
            throw new LuaException("Key " + keyName + " not found.");
        }

        public int getKey(string keyName)
        {
            foreach (MKey m in bb.Keys)
            {
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    return (int)m.KeyCode[0];
                }
            }
            throw new LuaException("Key " + keyName + " not found.");
        }

        public void clearKeys(string keyName)
        {
            foreach (MKey m in bb.Keys)
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

        public Vector3 getForward()
        {
            return bb.transform.forward;
        }

        public Vector3 getUp()
        {
            return bb.transform.up;
        }

        public Vector3 getRight()
        {
            return bb.transform.right;
        }

        public Vector3 getPosition()
        {
            return bb.transform.position;
        }

        public Vector3 getVelocity()
        {
            Rigidbody body = bb.GetComponent<Rigidbody>();
            if (body != null)
                return body.velocity;
            throw new LuaException("Block " + blockName + " has no rigid body.");
        }

        public float getMass()
        {
            Rigidbody body = bb.GetComponent<Rigidbody>();
            if (body != null)
                return body.mass;
            throw new LuaException("Block " + blockName + " has no rigid body.");
        }

        public Vector3 getCenterOfMass()
        {
            Rigidbody body = bb.GetComponent<Rigidbody>();
            if (body != null)
                return body.centerOfMass;
            throw new LuaException("Block " + blockName + " has no rigid body.");
        }

        public Vector3 getEulerAngles()
        {
            Vector3 d2r = new Vector3(convertToRadians, convertToRadians, convertToRadians);
            Vector3 euler = bb.transform.eulerAngles;
            euler.Scale(d2r);
            return euler;
        }

        public Vector3 getAngularVelocity()
        {
            Rigidbody body = bb.GetComponent<Rigidbody>();
            if (body != null)
            {
                Vector3 convertUnits = new Vector3(convertToDegrees, convertToDegrees, convertToDegrees);
                Vector3 angularVelocity = body.angularVelocity;
                angularVelocity.Scale(convertUnits);
                return angularVelocity;
            }
            throw new LuaException("Block " + blockName + " has no rigid body.");
        }
    }
}
