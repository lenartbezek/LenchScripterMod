using System.Reflection;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the Flamethrower block.
    /// </summary>
    public class Flamethrower : Block
    {
        private static readonly FieldInfo HoldFieldInfo = typeof(FlamethrowerController).GetField("holdToFire",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo KeyHeld = typeof(FlamethrowerController).GetField("keyHeld",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FlamethrowerController _fc;
        private readonly MToggle _holdToFire;
        private bool _lastIgniteFlag;
        private bool _setIgniteFlag;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Flamethrower(BlockBehaviour bb) : base(bb)
        {
            _fc = bb.GetComponent<FlamethrowerController>();
            _holdToFire = HoldFieldInfo.GetValue(_fc) as MToggle;
        }

        /// <summary>
        ///     Remaining time of the flamethrower.
        /// </summary>
        public float RemainingTime
        {
            get { return 10 - _fc.timey; }
            set { _fc.timey = 10 - value; }
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
                case "IGNITE":
                    Ignite();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Ignite the flamethrower.
        /// </summary>
        public void Ignite()
        {
            _setIgniteFlag = true;
        }

        /// <summary>
        ///     Handles igniting the Flamethrower.
        /// </summary>
        protected override void LateUpdate()
        {
            if (_setIgniteFlag)
            {
                if (!_fc.timeOut || StatMaster.GodTools.InfiniteAmmoMode)
                    if (_holdToFire.IsActive)
                        _fc.FlameOn();
                    else
                        _fc.Flame();
                _setIgniteFlag = false;
                _lastIgniteFlag = true;
            }
            else if (_lastIgniteFlag)
            {
                KeyHeld.SetValue(_fc, true);
                _lastIgniteFlag = false;
            }
        }
    }
}