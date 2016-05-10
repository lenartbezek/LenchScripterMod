namespace LenchScripter.Blocks
{
    /// <summary>
    /// Handler for the Spring and Rope blocks.
    /// </summary>
    public class Spring : Block
    {
        private SpringCode sc;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Spring(BlockBehaviour bb) : base(bb)
        {
            sc = bb.GetComponent<SpringCode>();
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
                Wind();
                return;
            }
            if (sc.winchMode && actionName == "UNWIND")
            {
                Unwind();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Controls the spring / winch. Contracts if rate is positive and unwinds if negative.
        /// Springs cannot be unwound.
        /// </summary>
        /// <param name="rate">Rate of movement.</param>
        public void SetInput(float rate = 1)
        {
            if (sc.winchMode)
            {
                if (rate > 0)
                    sc.WinchContract(rate);
                else
                    sc.WinchUnwind(rate);
            }
            else
            {
                sc.Contract(rate);
            }
        }

        /// <summary>
        /// Contracts the spring.
        /// </summary>
        public void Contract()
        {
            sc.Contract(1);
        }

        /// <summary>
        /// Winds the winch.
        /// </summary>
        public void Wind()
        {
            sc.WinchContract(1);
        }

        /// <summary>
        /// Unwinds the winch.
        /// </summary>
        public void Unwind()
        {
            sc.WinchUnwind(1);
        }
    }
}
