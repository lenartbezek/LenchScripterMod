namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Explosive Decoupler block.
    /// </summary>
    public class Decoupler : Block
    {
        private ExplosiveBolt eb;

        internal Decoupler(BlockBehaviour bb) : base(bb)
        {
            eb = bb.GetComponent<ExplosiveBolt>();
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "EXPLODE")
            {
                Explode();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Explode the decoupler.
        /// </summary>
        public void Explode()
        {
            eb.Explode();
        }

        internal static bool isDecoupler(BlockBehaviour bb)
        {
            return bb.GetComponent<ExplosiveBolt>() != null;
        }
    }
}
