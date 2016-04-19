using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Flamethrower block.
    /// </summary>
    public class Flamethrower : Block
    {
        private FlamethrowerController fc;
        private MToggle holdToFire;

        internal Flamethrower(BlockBehaviour bb) : base(bb)
        {
            fc = bb.GetComponent<FlamethrowerController>();
            FieldInfo holdFieldInfo = fc.GetType().GetField("holdToFire", BindingFlags.NonPublic | BindingFlags.Instance);
            holdToFire = holdFieldInfo.GetValue(fc) as MToggle;
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
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Ignite the flamethrower.
        /// </summary>
        public void Ignite()
        {
            if (holdToFire.IsActive)
            {
                if (fc.timeOut || STATLORD.infiniteAmmoMode)
                {
                    fc.Flame();
                }
            }
            else if (fc || STATLORD.infiniteAmmoMode)
            {
                fc.FlameOn();
            }

            if (fc.isFlaming)
            {
                fc.timey = fc.timey + Time.deltaTime;
            }
            if (fc.timey >= 10)
            {
                fc.TimeOut();
            }
        }

        internal static bool isFlamethrower(BlockBehaviour bb)
        {
            return bb.GetComponent<FlamethrowerController>() != null;
        }
    }
}
