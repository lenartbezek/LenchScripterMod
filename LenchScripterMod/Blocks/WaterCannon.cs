using System.Reflection;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the Water Cannon block.
    /// </summary>
    public class WaterCannon : Block
    {
        private static readonly FieldInfo HoldFieldInfo = typeof(WaterCannonController).GetField("holdToShootToggle",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly MToggle _holdToShootToggle;
        private bool _setShootFlag;
        private bool _lastShootFlag;
        private bool _realHoldToShootToggle;
        private readonly WaterCannonController _wcc;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public WaterCannon(BlockBehaviour bb) : base(bb)
        {
            _wcc = bb.GetComponent<WaterCannonController>();

            _holdToShootToggle = HoldFieldInfo.GetValue(_wcc) as MToggle;
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
        ///     Shoots the water cannon.
        /// </summary>
        public void Shoot()
        {
            _setShootFlag = true;
        }

        /// <summary>
        ///     Handles shooting the water cannon.
        /// </summary>
        protected override void Update()
        {
            if (_setShootFlag)
            {
                _realHoldToShootToggle = _realHoldToShootToggle ? _realHoldToShootToggle : _wcc.isActive;
                _holdToShootToggle.IsActive = false;
                _wcc.isActive = _realHoldToShootToggle || !_wcc.isActive;
                _lastShootFlag = _realHoldToShootToggle;
                _setShootFlag = false;
            }
            else if (_lastShootFlag)
            {
                _holdToShootToggle.IsActive = _realHoldToShootToggle;
                _wcc.isActive = false;
                _lastShootFlag = false;
            }
        }
    }
}