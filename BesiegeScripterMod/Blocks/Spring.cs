using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Spring and Rope blocks.
    /// </summary>
    public class Spring : Block
    {
        private SpringCode sc;
        private MSlider speedSlider;
        private MToggle toggleMode;
        private FieldInfo isToggled;
        private FieldInfo shouldContract;
        private FieldInfo maxMgntd;

        internal Spring(BlockBehaviour bb) : base(bb)
        {
            sc = bb.GetComponent<SpringCode>();
            FieldInfo speedFieldInfo = sc.GetType().GetField("speedSlider", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo toggleFieldInfo = sc.GetType().GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);

            speedSlider = speedFieldInfo.GetValue(sc) as MSlider;
            toggleMode = toggleFieldInfo.GetValue(sc) as MToggle;

            isToggled = sc.GetType().GetField("isToggled", BindingFlags.NonPublic | BindingFlags.Instance);
            shouldContract = sc.GetType().GetField("shouldContract", BindingFlags.NonPublic | BindingFlags.Instance);
            maxMgntd = sc.GetType().GetField("maxMgntd", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (!sc.winchMode && actionName == "CONTRACT")
            {
                Contract();
                return;
            }
            if (sc.winchMode && actionName == "WIND")
            {
                Wind(1);
                return;
            }
            if (sc.winchMode && actionName == "UNWIND")
            {
                Unwind(-1);
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Contracts the spring.
        /// </summary>
        public void Contract()
        {
            if (sc.winchMode || !this.toggleMode.IsActive)
            {
                shouldContract.SetValue(sc, true);
            }
            else
            {
                bool t = (bool)isToggled.GetValue(sc);
                isToggled.SetValue(sc, t);
                shouldContract.SetValue(sc, t);
            }
        }

        /// <summary>
        /// Winds the winch.
        /// </summary>
        /// <param name="rate">Rate to be winded at.</param>
        public void Wind(float rate)
        {
            if (!sc.winchMode) return;
            float m = (float)maxMgntd.GetValue(sc) - Time.fixedDeltaTime * rate * speedSlider.Value;
            maxMgntd.SetValue(sc, Mathf.Max(m, 0.1f));
            sc.AnimateWinch(1);
        }

        /// <summary>
        /// Unwinds the winch.
        /// </summary>
        /// <param name="rate">Rate to be unwinded at.</param>
        public void Unwind(float rate)
        {
            if (!sc.winchMode) return;
            maxMgntd.SetValue(sc, (float)maxMgntd.GetValue(sc) + Time.fixedDeltaTime * rate * speedSlider.Value);
            sc.AnimateWinch(-1);
        }

        internal static bool isSpring(BlockBehaviour bb)
        {
            return bb.GetComponent<SpringCode>() != null;
        }
    }
}
