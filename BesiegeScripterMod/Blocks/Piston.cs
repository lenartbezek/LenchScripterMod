using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Piston block.
    /// </summary>
    public class Piston : Block
    {
        private SliderCompress sc;
        private MSlider speedSlider;
        private MToggle toggleMode;
        private MKey extendKey;

        internal Piston(BlockBehaviour bb) : base(bb)
        {
            sc = bb.GetComponent<SliderCompress>();

            FieldInfo speedFieldInfo = sc.GetType().GetField("speedSlider", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo toggleFieldInfo = sc.GetType().GetField("extendKey", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo extendFieldInfo = sc.GetType().GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);

            speedSlider = speedFieldInfo.GetValue(sc) as MSlider;
            toggleMode = toggleFieldInfo.GetValue(sc) as MToggle;
            extendKey = extendFieldInfo.GetValue(sc) as MKey;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "EXTEND")
            {
                Extend();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Extend the piston.
        /// </summary>
        public void Extend()
        {
            if (sc.myJoint == null || sc.myJoint.connectedBody == null) return;
            if (toggleMode.IsActive)
            {
                sc.posToBe = sc.newLimit;
            }
            else
            {
                sc.posToBe = (sc.posToBe != sc.newLimit ? sc.newLimit : sc.startLimit);
            }
            float joint_pos = sc.myJoint.targetPosition.x;
            if (joint_pos != sc.posToBe)
            {
                joint_pos = Mathf.Lerp(joint_pos, sc.posToBe, Time.deltaTime * sc.lerpSpeed * speedSlider.Value);
                Vector3 target_pos = sc.myJoint.targetPosition;
                target_pos.x = joint_pos;
                sc.myJoint.targetPosition = target_pos;
            }
        }

        internal static bool isPiston(BlockBehaviour bb)
        {
            return bb.GetComponent<SliderCompress>() != null;
        }
    }
}
