using System.Reflection;
using UnityEngine;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the Flying Spiral block.
    /// </summary>
    public class FlyingSpiral : Block
    {
        private static readonly FieldInfo Flying = typeof(FlyingController).GetField("flying",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo SpeedToGo = typeof(FlyingController).GetField("speedToGo",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo LerpySpeed = typeof(FlyingController).GetField("lerpySpeed",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ToggleFieldInfo = typeof(FlyingController).GetField("toggleMode",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo RigidbodyFieldInfo = typeof(FlyingController).GetField("myRigidbody",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly FlyingController _fc;
        private bool _lastFlyingFlag;
        private readonly Rigidbody _rigidbody;

        private bool _setFlyingFlag;
        private readonly MToggle _toggleMode;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public FlyingSpiral(BlockBehaviour bb) : base(bb)
        {
            _fc = bb.GetComponent<FlyingController>();
            _toggleMode = ToggleFieldInfo.GetValue(_fc) as MToggle;
            _rigidbody = RigidbodyFieldInfo.GetValue(_fc) as Rigidbody;
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
                case "SPIN":
                    Spin();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Spin the Flying Spiral.
        /// </summary>
        public void Spin()
        {
            _setFlyingFlag = true;
        }

        private void Fly(bool f)
        {
            if (f && !_fc.isFrozen && _fc.canFly)
            {
                SpeedToGo.SetValue(_fc, _fc.speed);
                LerpySpeed.SetValue(_fc, _fc.lerpSpeed + Random.Range(-2, 3));
                _rigidbody.drag = 1.5f;
                Flying.SetValue(_fc, true);
            }
            else
            {
                SpeedToGo.SetValue(_fc, Vector3.zero);
                _rigidbody.drag = 0.5f;
                Flying.SetValue(_fc, false);
            }
        }

        /// <summary>
        ///     Sets the speed and drag of the block to make it fly.
        /// </summary>
        protected override void Update()
        {
            if (_setFlyingFlag)
            {
                if (_toggleMode.IsActive)
                    Fly(!(bool) Flying.GetValue(_fc));
                else
                    Fly(true);
                _setFlyingFlag = false;
                _lastFlyingFlag = true;
            }
            else if (_lastFlyingFlag)
            {
                if (!_toggleMode.IsActive)
                    Fly(false);
                _lastFlyingFlag = false;
            }
        }
    }
}