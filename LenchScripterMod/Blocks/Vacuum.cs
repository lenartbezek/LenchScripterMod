namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for cannon blocks; Cannon and Shrapnel Cannon.
    /// </summary>
    public class Vacuum : Block
    {
        private readonly VacuumBlock _vb;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Vacuum(BlockBehaviour bb) : base(bb)
        {
            _vb = bb.GetComponent<VacuumBlock>();
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
                case "ACTIVATE":
                    Activate();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Activates the vacuum block.
        /// </summary>
        public void Activate()
        {
            // TODO
        }
    }
}