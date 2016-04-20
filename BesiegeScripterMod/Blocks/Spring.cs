namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Spring and Rope blocks.
    /// </summary>
    public class Spring : Block
    {
        private SpringCode sc;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
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
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
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

        internal static bool isSpring(BlockBehaviour bb)
        {
            return bb.GetComponent<SpringCode>() != null;
        }
    }
}
