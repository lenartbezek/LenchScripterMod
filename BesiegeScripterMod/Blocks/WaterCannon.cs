using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Water Cannon block.
    /// </summary>
    public class WaterCannon : Block
    {
        private WaterCannonController wcc;

        private bool setShootFlag = false;
        private bool lastShootFlag = false;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public WaterCannon(BlockBehaviour bb) : base(bb)
        {
            wcc = bb.GetComponent<WaterCannonController>();
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
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Shoots the water cannon.
        /// </summary>
        public void Shoot()
        {
            setShootFlag = true;
        }

        internal override void Update()
        {
            if (setShootFlag)
            {
                wcc.isActive = true;
                lastShootFlag = false;
            }
            else if (lastShootFlag)
            {
                wcc.isActive = false;
                lastShootFlag = false;
            }
        }
    }
}
