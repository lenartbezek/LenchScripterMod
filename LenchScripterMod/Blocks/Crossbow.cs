namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the crossbow block.
    /// </summary>
    public class Crossbow : Block
    {
        private readonly CrossBowBlock _cbb;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Crossbow(BlockBehaviour bb) : base(bb)
        {
            _cbb = bb.GetComponent<CrossBowBlock>();
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
        ///     Shoots the crossbow.
        /// </summary>
        public void Shoot()
        {
            _cbb.SendMessage("FIRE");
        }

        /// <summary>
        ///     Number of arrows remaining.
        /// </summary>
        public int Ammo
        {
            get { return _cbb.ammo; }
            set { _cbb.ammo = value; }
        }
    }
}