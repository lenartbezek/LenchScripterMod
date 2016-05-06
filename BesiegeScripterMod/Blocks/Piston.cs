using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Piston block.
    /// </summary>
    public class Piston : Block
    {
        private static FieldInfo toggleFieldInfo = typeof(SliderCompress).GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo extendFieldInfo = typeof(SliderCompress).GetField("extendKey", BindingFlags.NonPublic | BindingFlags.Instance);

        private SliderCompress sc;
        private MToggle toggleMode;
        private MKey extendKey;

        private bool setExtendFlag = false;
        private bool lastExtendFlag = false;
        private float defaultStartLimit;
        private float defaultNewLimit;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Piston(BlockBehaviour bb) : base(bb)
        {
            sc = bb.GetComponent<SliderCompress>();

            toggleMode = toggleFieldInfo.GetValue(sc) as MToggle;
            extendKey = extendFieldInfo.GetValue(sc) as MKey;

            defaultStartLimit = sc.startLimit;
            defaultNewLimit = sc.newLimit;
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
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Extend the piston.
        /// </summary>
        public void Extend()
        {
            if (toggleMode.IsActive)
            {
                sc.posToBe = (sc.posToBe != sc.newLimit ? sc.newLimit : sc.startLimit);
            }
            else
            {
                setExtendFlag = true;
            }
        }

        internal override void Update()
        {
            if (setExtendFlag)
            {
                if (!extendKey.IsDown)
                {
                    sc.startLimit = defaultNewLimit;
                    sc.newLimit = defaultStartLimit;
                }
                setExtendFlag = false;
                lastExtendFlag = true;
            }
            else if (lastExtendFlag)
            {
                sc.startLimit = defaultStartLimit;
                sc.newLimit = defaultNewLimit;
                lastExtendFlag = false;
            }
        }

        internal static bool isPiston(BlockBehaviour bb)
        {
            return bb.GetComponent<SliderCompress>() != null;
        }
    }
}
