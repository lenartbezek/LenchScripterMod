using System.Reflection;

namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for cannon blocks; Cannon and Shrapnel Cannon.
    /// </summary>
    public class Cannon : Block
    {
        private CanonBlock cb;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Cannon(BlockBehaviour bb) : base(bb)
        {
            cb = bb.GetComponent<CanonBlock>();
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "SHOOT")
            {
                Shoot();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Shoots the cannon.
        /// </summary>
        public void Shoot()
        {
            cb.Shoot();
        }
    }
}
