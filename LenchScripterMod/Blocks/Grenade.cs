namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for the Grenade block.
    /// </summary>
    public class Grenade : Block
    {
        private ControllableBomb cb;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Grenade(BlockBehaviour bb) : base(bb)
        {
            cb = bb.GetComponent<ControllableBomb>();
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "DETONATE")
            {
                Detonate();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Detonate the grenade.
        /// </summary>
        public void Detonate()
        {
            cb.StartCoroutine_Auto(cb.Explode());
        }
    }
}
