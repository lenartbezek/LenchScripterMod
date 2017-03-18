namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for cannon blocks; Cannon and Shrapnel Cannon.
    /// </summary>
    public class Cannon : Block
    {
        private readonly CanonBlock _cb;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Cannon(BlockBehaviour bb) : base(bb)
        {
            _cb = bb.GetComponent<CanonBlock>();
        }

        /// <summary>
        ///     Invokes the block's action.
        ///     Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            switch (actionName)
            {
                case "SHOOT":
                    Shoot();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Shoots the cannon.
        /// </summary>
        public void Shoot()
        {
            _cb.Shoot();
        }
    }
}