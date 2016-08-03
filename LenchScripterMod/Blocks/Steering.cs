using System;
using System.Reflection;
using UnityEngine;

namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for steering blocks; Steering and Steering Hinge.
    /// </summary>
    public class Steering : Block
    {
        private static FieldInfo angleyToBeField = typeof(SteeringWheel).GetField("angleyToBe", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo angleMultiplierField = typeof(SteeringWheel).GetField("angleMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo speedSliderField = typeof(SteeringWheel).GetField("speedSlider", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo limitsSliderField = typeof(SteeringWheel).GetField("limitsSlider", BindingFlags.NonPublic | BindingFlags.Instance);

        private SteeringWheel sw;

        private MSlider speedSlider;
        private MLimits limitsSlider;

        private float desired_input;
        private bool setInputFlag = false;

        private float desired_angle;
        private bool setAngleFlag = false;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Steering(BlockBehaviour bb) : base(bb)
        {
            sw = bb.GetComponent<SteeringWheel>();
            speedSlider = speedSliderField.GetValue(sw) as MSlider;
            limitsSlider = limitsSliderField.GetValue(sw) as MLimits;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not poses such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
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
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
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
            angle /= convertToRadians;
            if (float.IsNaN(angle))
                throw new ArgumentException("Value is not a number (NaN).");
            if (sw.allowLimits && limitsSlider.IsActive)
            {
                if (!sw.flipped)
                    desired_angle = Mathf.Clamp(angle, -limitsSlider.Min, limitsSlider.Max);
                else
                    desired_angle = Mathf.Clamp(angle * -1, -limitsSlider.Max, limitsSlider.Min);
            }
            else
            {
                desired_angle = angle * (sw.flipped ? -1 : 1);
            }

            setAngleFlag = true;
        }

        /// <summary>
        /// Returns the current angle of the joint.
        /// </summary>
        /// <returns>Float value in degrees or radians as specified.</returns>
        public float GetAngle()
        {
            return (float)angleyToBeField.GetValue(sw) * convertToRadians;
        }

        /// <summary>
        /// Handles the movement of the joint.
        /// </summary>
        protected override void LateUpdate()
        {
            if (setAngleFlag)
            {
                float current_angle = (float)angleyToBeField.GetValue(sw);
                if (Mathf.Abs(Mathf.DeltaAngle(current_angle, desired_angle)) < 0.1)
                {
                    setAngleFlag = false;
                }
                else
                {
                    desired_input = Mathf.DeltaAngle(current_angle, desired_angle) / 
                        ((float)angleMultiplierField.GetValue(sw) * speedSlider.Value * Time.deltaTime);
                    desired_input = Mathf.Clamp(desired_input, -1, 1);
                    setInputFlag = true;
                }
            }

            if (setInputFlag)
            {
                if (speedSlider.Value != 0)
                {
                    float speed = desired_input * (float)angleMultiplierField.GetValue(sw) * speedSlider.Value;
                    float current_angle = (float)angleyToBeField.GetValue(sw);
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
                    angleyToBeField.SetValue(sw, new_angle);
                }
                setInputFlag = false;
            }
        }
    }
}
