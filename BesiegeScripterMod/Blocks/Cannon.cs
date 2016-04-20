using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for cannon blocks; Cannon and Shrapnel Cannon.
    /// </summary>
    public class Cannon : Block
    {
        private CanonBlock cb;
        private ArrowTurret turret;
        private ShrapnelCannon shrapnel;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            cb = bb.GetComponent<CanonBlock>();
            FieldInfo turret_field = cb.GetType().GetField("turret", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo shrapnel_field = cb.GetType().GetField("shrapnel", BindingFlags.NonPublic | BindingFlags.Instance);
            turret = turret_field.GetValue(cb) as ArrowTurret;
            shrapnel = shrapnel_field.GetValue(cb) as ShrapnelCannon;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "SHOOT")
            {
                Shoot();
                return;
            }
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
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

        internal static bool isCannon(BlockBehaviour bb)
        {
            return bb.GetComponent<CanonBlock>() != null;
        }
    }
}
