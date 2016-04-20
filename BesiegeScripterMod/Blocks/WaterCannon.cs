using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Water Cannon block.
    /// </summary>
    public class WaterCannon : Block
    {
        private WaterCannonController wcc;
        private MToggle holdToShootToggle;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            wcc = bb.GetComponent<WaterCannonController>();
            FieldInfo holdFieldInfo = wcc.GetType().GetField("holdToShootToggle", BindingFlags.NonPublic | BindingFlags.Instance);
            holdToShootToggle = holdFieldInfo.GetValue(wcc) as MToggle;
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
        /// Shoots the water cannon.
        /// </summary>
        public void Shoot()
        {
            wcc.isActive = holdToShootToggle.IsActive ? true : !wcc.isActive;

            if (wcc.prevActiveState != wcc.isActive)
            {
                wcc.prevActiveState = wcc.isActive;
                wcc.StartCoroutine_Auto(wcc.SetParticles());
            }
            if (wcc.prevBoilingState != wcc.boiling)
            {
                wcc.prevBoilingState = wcc.boiling;
                wcc.StartCoroutine_Auto(wcc.SetParticles());
            }
            wcc.boiling = wcc.glowCode.lerpedGlowAmount > 0.1f;
        }

        internal static bool isWaterCannon(BlockBehaviour bb)
        {
            return bb.GetComponent<WaterCannonController>() != null;
        }
    }
}
