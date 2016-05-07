namespace LenchScripter.Blocks
{
    /// <summary>
    /// Handler for the Rocket block.
    /// </summary>
    public class Rocket : Block
    {
        private TimedRocket tr;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Rocket(BlockBehaviour bb) : base(bb)
        {
            tr = bb.GetComponent<TimedRocket>();
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "LAUNCH")
            {
                Launch();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Launch the rocket.
        /// </summary>
        public void Launch()
        {
            if (!tr.hasFired)
            {
                tr.hasFired = true;
                tr.StartCoroutine(tr.Fire(0));
            }
        }

        /// <summary>
        /// Returns true if the rocket has fired.
        /// </summary>
        /// <returns></returns>
        public bool hasFired()
        {
            return tr.hasFired;
        }
    }
}
