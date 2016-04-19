using System.Reflection;

namespace LenchScripterMod.Blocks
{
    public class WaterCannon : Block
    {
        private WaterCannonController wcc;
        private MToggle holdToShootToggle;

        internal WaterCannon(BlockBehaviour bb) : base(bb)
        {
            wcc = bb.GetComponent<WaterCannonController>();
            FieldInfo holdFieldInfo = wcc.GetType().GetField("holdToShootToggle", BindingFlags.NonPublic | BindingFlags.Instance);
            holdToShootToggle = holdFieldInfo.GetValue(wcc) as MToggle;
        }

        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "SHOOT")
            {
                Shoot();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        public void Shoot()
        {
            if (holdToShootToggle.IsActive)
            {
                wcc.isActive = true;
            }
            else
            {
                wcc.isActive = !wcc.isActive;
            }
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
