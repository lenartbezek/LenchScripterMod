namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Grenade block.
    /// </summary>
    public class Grenade : Block
    {
        private ControllableBomb cb;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            cb = bb.GetComponent<ControllableBomb>();
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "DETONATE")
            {
                Detonate();
                return;
            }
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Detonate the grenade.
        /// </summary>
        public void Detonate()
        {
            cb.StartCoroutine_Auto(cb.Explode());
        }

        internal static bool isGrenade(BlockBehaviour bb)
        {
            return bb.GetComponent<ControllableBomb>() != null;
        }
    }
}
