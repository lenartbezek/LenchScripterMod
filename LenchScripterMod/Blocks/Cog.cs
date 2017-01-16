using System;
using System.Reflection;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for all wheel and cog blocks.
    /// </summary>
    public class Cog : Block
    {
        private static readonly FieldInfo Input = typeof(CogMotorControllerHinge).GetField("input",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly CogMotorControllerHinge _cmc;

        private float _desiredInput;
        private bool _setInputFlag;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Cog(BlockBehaviour bb) : base(bb)
        {
            _cmc = bb.GetComponent<CogMotorControllerHinge>();
        }

        /// <summary>
        ///     Invokes the block's action.
        ///     Throws ActionNotFoundException if the block does not poses such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            switch (actionName)
            {
                case "FORWARDS":
                    SetInput(1);
                    return;
                case "REVERSE":
                    SetInput(-1);
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Sets the input value on the next LateUpdate.
        /// </summary>
        /// <param name="value">Value to be set.</param>
        public void SetInput(float value)
        {
            if (float.IsNaN(value))
                throw new ArgumentException("Value is not a number (NaN).");
            _desiredInput = value;
            _setInputFlag = true;
        }

        /// <summary>
        ///     Sets the desired input value to be read at the next FixedUpdate of the BlockBehaviour script.
        /// </summary>
        protected override void LateUpdate()
        {
            if (!_setInputFlag) return;

            Input.SetValue(_cmc, _desiredInput);
            _setInputFlag = false;
        }
    }
}