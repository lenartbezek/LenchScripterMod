using System.Reflection;
using UnityEngine;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the Piston block.
    /// </summary>
    public class Piston : Block
    {
        private static readonly FieldInfo ToggleFieldInfo = typeof(SliderCompress).GetField("toggleMode",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly FieldInfo ExtendFieldInfo = typeof(SliderCompress).GetField("extendKey",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly float _defaultNewLimit;
        private readonly float _defaultStartLimit;
        private bool _setExtendFlag;
        private bool _setPositionFlag;
        private float _targetPosition;
        private bool _lastExtendFlag;
        private readonly MKey _extendKey;
        private readonly MToggle _toggleMode;
        private readonly SliderCompress _sc;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Piston(BlockBehaviour bb) : base(bb)
        {
            _sc = bb.GetComponent<SliderCompress>();

            _toggleMode = ToggleFieldInfo.GetValue(_sc) as MToggle;
            _extendKey = ExtendFieldInfo.GetValue(_sc) as MKey;

            _defaultStartLimit = _sc.startLimit;
            _defaultNewLimit = _sc.newLimit;
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
                case "EXTEND":
                    Extend();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Extend the piston.
        /// </summary>
        public void Extend()
        {
            if (_toggleMode.IsActive)
                _sc.posToBe = _sc.posToBe != _sc.newLimit ? _sc.newLimit : _sc.startLimit;
            else
                _setExtendFlag = true;
        }

        /// <summary>
        ///     Set the position between compressed and extended position.
        /// </summary>
        /// <param name="t"></param>
        public void SetPosition(float t)
        {
            _targetPosition = Mathf.Lerp(_defaultStartLimit, _defaultNewLimit, t);
            _setPositionFlag = true;
        }

        /// <summary>
        ///     Handles extending and compressing the piston.
        /// </summary>
        protected override void Update()
        {
            if (_setExtendFlag)
            {
                if (!_extendKey.IsDown)
                {
                    _sc.startLimit = _defaultNewLimit;
                    _sc.newLimit = _defaultStartLimit;
                }
                _setExtendFlag = false;
                _lastExtendFlag = true;
                _setPositionFlag = false;
            }
            else if (_setPositionFlag)
            {
                if (_toggleMode.IsActive)
                {
                    _sc.posToBe = _targetPosition;
                }
                else
                {
                    _sc.startLimit = _targetPosition;
                    _sc.newLimit = _targetPosition;
                }
                _lastExtendFlag = true;
                _setPositionFlag = false;
            }
            else if (_lastExtendFlag)
            {
                _sc.startLimit = _defaultStartLimit;
                _sc.newLimit = _defaultNewLimit;
                _lastExtendFlag = false;
            }
        }
    }
}