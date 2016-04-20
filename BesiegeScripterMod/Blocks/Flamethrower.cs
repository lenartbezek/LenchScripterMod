using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Flamethrower block.
    /// </summary>
    public class Flamethrower : Block
    {
        private FlamethrowerController fc;
        private MToggle holdToFire;
        private FieldInfo keyHeld;

        private bool setIgniteFlag = false;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            fc = bb.GetComponent<FlamethrowerController>();
            FieldInfo holdFieldInfo = fc.GetType().GetField("holdToFire", BindingFlags.NonPublic | BindingFlags.Instance);
            holdToFire = holdFieldInfo.GetValue(fc) as MToggle;
            keyHeld = fc.GetType().GetField("keyHeld", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "IGNITE")
            {
                Ignite();
                return;
            }
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Ignite the flamethrower.
        /// </summary>
        public void Ignite()
        {
            setIgniteFlag = true;
        }

        private void LateUpdate()
        {
            if (!fc.timeOut || STATLORD.infiniteAmmoMode)
            {
                if (holdToFire.IsActive)
                {
                    keyHeld.SetValue(fc, true);
                    fc.FlameOn();
                }
                else
                {
                    fc.Flame();
                }
            }
            setIgniteFlag = false;
        }

        internal static bool isFlamethrower(BlockBehaviour bb)
        {
            return bb.GetComponent<FlamethrowerController>() != null;
        }
    }
}
