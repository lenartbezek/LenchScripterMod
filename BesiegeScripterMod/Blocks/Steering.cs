using System;
using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for steering blocks; Steering and Steering Hinge.
    /// </summary>
    public class Steering : Block
    {
        private SteeringWheel sw;

        private MSlider speedSlider;
        private MLimits limitsSlider;

        private FieldInfo angleyToBe;
        private FieldInfo angleMultiplier;

        private float desired_input;
        private bool setInputFlag = false;

        private float desired_angle;
        private bool setAngleFlag = false;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            sw = bb.GetComponent<SteeringWheel>();
            speedSlider = sw.GetType().GetField("speedSlider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sw) as MSlider;
            limitsSlider = sw.GetType().GetField("limitsSlider", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(sw) as MLimits;
            angleyToBe = sw.GetType().GetField("angleyToBe", BindingFlags.NonPublic | BindingFlags.Instance);
            angleMultiplier = sw.GetType().GetField("angleMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
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
            desired_input = value;
            setInputFlag = true;
            setAngleFlag = false;
        }

        /// <summary>
        /// Moves the joint to the desired angle.
        /// </summary>
        /// <param name="angle">Float value in degrees.</param>
        public void SetAngle(float angle)
        {
            desired_angle = angle;
            setAngleFlag = true;
        }

        /// <summary>
        /// Returns the current angle of the joint.
        /// </summary>
        /// <returns>Float value in degrees or radians as specified.</returns>
        public float GetAngle()
        {
            return (float)angleyToBe.GetValue(sw) * convertToRadians;
        }

        private void LateUpdate()
        {
            if (setAngleFlag)
            {
                float current_angle = (float)angleyToBe.GetValue(sw);
                if (Mathf.Abs(Mathf.DeltaAngle(current_angle, desired_angle)) < 0.1)
                {
                    setAngleFlag = false;
                }
                else
                {
                    desired_input = Mathf.Clamp(Mathf.DeltaAngle(current_angle, desired_angle), -1, 1);
                    setInputFlag = true;
                }
            }

            if (setInputFlag)
            {
                if (speedSlider.Value != 0)
                {
                    float speed = desired_input * (float)angleMultiplier.GetValue(sw) * speedSlider.Value;
                    float current_angle = (float)angleyToBe.GetValue(sw);
                    float new_angle = current_angle + speed * Time.deltaTime;
                    if (sw.allowLimits && limitsSlider.IsActive)
                    {
                        if (!sw.flipped)
                            new_angle = Mathf.Clamp(new_angle, -limitsSlider.Min, limitsSlider.Max);
                        else
                            new_angle = Mathf.Clamp(new_angle, -limitsSlider.Max, limitsSlider.Min);
                    }
                    else if (new_angle > 180)
                        new_angle = new_angle - 360;
                    else if (new_angle < -180)
                        new_angle = new_angle + 360;
                    angleyToBe.SetValue(sw, new_angle);
                }
                setInputFlag = false;
            }
        }

        internal static bool isSteering(BlockBehaviour bb)
        {
            return bb.GetComponent<SteeringWheel>() != null;
        }
    }
}
