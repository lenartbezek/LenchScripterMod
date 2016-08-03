using System.Reflection;

namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for the Water Cannon block.
    /// </summary>
    public class WaterCannon : Block
    {
        private static FieldInfo holdFieldInfo = typeof(WaterCannonController).GetField("holdToShootToggle", BindingFlags.NonPublic | BindingFlags.Instance);

        private WaterCannonController wcc;

        private bool setShootFlag = false;
        private bool lastShootFlag = false;
        private MToggle holdToShootToggle;
        private bool realHoldToShootToggle;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public WaterCannon(BlockBehaviour bb) : base(bb)
        {
            wcc = bb.GetComponent<WaterCannonController>();

            holdToShootToggle = holdFieldInfo.GetValue(wcc) as MToggle;
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
        /// Shoots the water cannon.
        /// </summary>
        public void Shoot()
        {
            setShootFlag = true;
        }

        /// <summary>
        /// Handles shooting the water cannon.
        /// </summary>
        protected override void Update()
        {
            if (setShootFlag)
            {
                realHoldToShootToggle = realHoldToShootToggle ? realHoldToShootToggle : wcc.isActive;
                holdToShootToggle.IsActive = false;
                wcc.isActive = realHoldToShootToggle ? true : !wcc.isActive;
                lastShootFlag = realHoldToShootToggle;
                setShootFlag = false;
            }
            else if (lastShootFlag)
            {
                holdToShootToggle.IsActive = realHoldToShootToggle;
                wcc.isActive = false;
                lastShootFlag = false;
            }
        }
    }
}
