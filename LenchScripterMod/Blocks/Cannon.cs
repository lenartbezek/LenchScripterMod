using System.Reflection;

namespace LenchScripter.Blocks
{
    /// <summary>
    /// Handler for cannon blocks; Cannon and Shrapnel Cannon.
    /// </summary>
    public class Cannon : Block
    {
        private static FieldInfo turret_field = typeof(CanonBlock).GetField("turret", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo shrapnel_field = typeof(CanonBlock).GetField("shrapnel", BindingFlags.NonPublic | BindingFlags.Instance);

        private CanonBlock cb;
        private ArrowTurret turret;
        private ShrapnelCannon shrapnel;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Cannon(BlockBehaviour bb) : base(bb)
        {
            cb = bb.GetComponent<CanonBlock>();
            turret = turret_field.GetValue(cb) as ArrowTurret;
            shrapnel = shrapnel_field.GetValue(cb) as ShrapnelCannon;
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
            if (turret)
                cb.StartCoroutine_Auto(turret.Shoot());
            if (shrapnel)
                cb.StartCoroutine_Auto(shrapnel.Shoot());
        }
    }
}
