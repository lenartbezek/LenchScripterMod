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
        private FieldInfo angleyToBe;
        private FieldInfo angleMultiplier;
        private MSlider speedSlider;
        private MLimits limitsSlider;

        internal Steering(BlockBehaviour bb) : base(bb)
        {
            sw = bb.GetComponent<SteeringWheel>();

            angleyToBe = sw.GetType().GetField("angleyToBe", BindingFlags.NonPublic | BindingFlags.Instance);
            angleMultiplier = sw.GetType().GetField("angleMultiplier", BindingFlags.NonPublic | BindingFlags.Instance);

            FieldInfo speedFieldInfo = sw.GetType().GetField("speedSlider", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo limitsFieldInfo = sw.GetType().GetField("limitsSlider", BindingFlags.NonPublic | BindingFlags.Instance);
            speedSlider = speedFieldInfo.GetValue(sw) as MSlider;
            limitsSlider = limitsFieldInfo.GetValue(sw) as MLimits;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
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
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        public void SetInput(float input)
        {
            float value = input * Time.deltaTime * (float)angleMultiplier.GetValue(sw) * speedSlider.Value;
            angleyToBe.SetValue(sw, (float)angleyToBe.GetValue(sw) + value);
            if (sw.allowLimits && limitsSlider.IsActive)
            {
                if (!sw.flipped)
                {
                    angleyToBe.SetValue(sw, Mathf.Clamp((float)angleyToBe.GetValue(sw), - limitsSlider.Min, limitsSlider.Max));
                }
                else
                {
                    angleyToBe.SetValue(sw, Mathf.Clamp((float)angleyToBe.GetValue(sw), - limitsSlider.Max, limitsSlider.Min));
                }
            }
            else if ((float)angleyToBe.GetValue(sw) > (float)180)
            {
                angleyToBe.SetValue(sw, (float)angleyToBe.GetValue(sw) - 360);
            }
            else if ((float)angleyToBe.GetValue(sw) < (float)-180)
            {
                angleyToBe.SetValue(sw, (float)angleyToBe.GetValue(sw) + 360);
            }
        }

        public float GetAngle()
        {
            return (float)angleyToBe.GetValue(sw);
        }

        internal static bool isSteering(BlockBehaviour bb)
        {
            return bb.GetComponent<SteeringWheel>() != null;
        }
    }
}
