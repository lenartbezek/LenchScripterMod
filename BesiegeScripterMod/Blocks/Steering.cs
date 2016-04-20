using System;
using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for steering blocks; Steering and Steering Hinge.
    /// </summary>
    public class Steering : Block
    {
        private SteeringWheel sw;
        private FieldInfo angleyToBe;
        private FieldInfo input;

        private float setInputValue;
        private bool setInputFlag = false;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            sw = bb.GetComponent<SteeringWheel>();
            angleyToBe = sw.GetType().GetField("angleyToBe", BindingFlags.NonPublic | BindingFlags.Instance);
            input = sw.GetType().GetField("input", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not poses such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "LEFT")
            {
                SetInput(1);
                return;
            }
            if (actionName == "RIGHT")
            {
                SetInput(-1);
                return;
            }
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Sets the input value on the next LateUpdate.
        /// </summary>
        /// <param name="value">Value to be set.</param>
        public void SetInput(float value = 1)
        {
            if (float.IsNaN(value))
                throw new ArgumentException("Value is not a number (NaN).");
            setInputValue = value;
            setInputFlag = true;
        }

        private void LateUpdate()
        {
            if (setInputFlag)
            {
                setInputFlag = false;
                input.SetValue(sw, setInputValue);
            }
        }

        /// <summary>
        /// Returns the angle of the joint.
        /// </summary>
        /// <returns>Float value in degrees or radians as specified.</returns>
        public float GetAngle()
        {
            return (float)angleyToBe.GetValue(sw) * convertToRadians;
        }

        internal static bool isSteering(BlockBehaviour bb)
        {
            return bb.GetComponent<SteeringWheel>() != null;
        }
    }
}
