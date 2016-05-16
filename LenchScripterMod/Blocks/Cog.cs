using System;
using System.Reflection;

namespace LenchScripter.Blocks
{
    /// <summary>
    /// Handler for all wheel and cog blocks.
    /// </summary>
    public class Cog : Block
    {
        private static FieldInfo input = typeof(CogMotorController).GetField("input", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo toggleFieldInfo = typeof(CogMotorController).GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);

        private CogMotorController cmc;
        private MToggle toggle;

        private float desired_input;
        private bool setInputFlag = false;
        private bool lastInputFlag = false;
        private bool realToggle;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Cog(BlockBehaviour bb) : base(bb)
        {
            cmc = bb.GetComponent<CogMotorController>();
            toggle = toggleFieldInfo.GetValue(cmc) as MToggle;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not poses such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "FORWARDS")
            {
                SetInput(1);
                return;
            }
            if (actionName == "REVERSE")
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
        }

        /// <summary>
        /// Sets the desired input value to be read at the next FixedUpdate of the BlockBehaviour script.
        /// </summary>
        protected override void Update()
        {
            if (setInputFlag)
            {
                realToggle = lastInputFlag ? realToggle : toggle.IsActive;
                toggle.IsActive = true;
                input.SetValue(cmc, desired_input);
                lastInputFlag = true;
                setInputFlag = false;
            }
            else if (lastInputFlag)
            {
                toggle.IsActive = realToggle;
                lastInputFlag = false;
            }
        }
    }
}
