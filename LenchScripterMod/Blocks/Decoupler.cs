namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for the Explosive Decoupler block.
    /// </summary>
    public class Decoupler : Block
    {
        private ExplosiveBolt eb;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Decoupler(BlockBehaviour bb) : base(bb)
        {
            eb = bb.GetComponent<ExplosiveBolt>();
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "EXPLODE")
            {
                Explode();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Explode the decoupler.
        /// </summary>
        public void Explode()
        {
            eb.Explode();
        }
    }
}
