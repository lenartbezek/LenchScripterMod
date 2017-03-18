using System;
using System.Reflection;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Block handler for Pixali's VectorThruster block (ID: 790)
    /// </summary>
    public class VectorThruster : Block
    {
        private static Type _scriptType;
        private static FieldInfo _codeControlledField;
        private static PropertyInfo _isOnField;
        private static PropertyInfo _verticalField;
        private static PropertyInfo _horizontalField;
        private static PropertyInfo _powerField;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public VectorThruster(BlockBehaviour bb) : base(bb)
        {
            if (_scriptType == null) ResolveFieldInfo(Bs);
        }

        /// <summary>
        ///     Is the vector thruster on.
        /// </summary>
        public bool IsOn
        {
            get { return (bool) _isOnField.GetValue(Bs, null); }
            set { _isOnField.SetValue(Bs, value, null); }
        }

        /// <summary>
        ///     Is the vector thruster code controlled.
        ///     Must be set to true to prevent it responding to keyboard presses.
        /// </summary>
        public bool CodeControlled
        {
            get { return (bool) _codeControlledField.GetValue(Bs); }
            set { _codeControlledField.SetValue(Bs, value); }
        }

        /// <summary>
        ///     Thrusters vertical offset bias.
        /// </summary>
        public float VerticalBias
        {
            get { return (float) _verticalField.GetValue(Bs, null); }
            set { _verticalField.SetValue(Bs, value, null); }
        }

        /// <summary>
        ///     Thrusters horizontal offset bias.
        /// </summary>
        public float HorizontalBias
        {
            get { return (float) _horizontalField.GetValue(Bs, null); }
            set { _horizontalField.SetValue(Bs, value, null); }
        }

        /// <summary>
        ///     Thrusters power.
        /// </summary>
        public float Power
        {
            get { return (float) _powerField.GetValue(Bs, null); }
            set { _powerField.SetValue(Bs, value, null); }
        }

        private static void ResolveFieldInfo(object bs)
        {
            _scriptType = bs.GetType();
            _codeControlledField = _scriptType.GetField("codeControlled", BindingFlags.Public | BindingFlags.Instance);
            _isOnField = _scriptType.GetProperty("IsOn", BindingFlags.Public | BindingFlags.Instance);
            _verticalField = _scriptType.GetProperty("UpDownAmount", BindingFlags.Public | BindingFlags.Instance);
            _horizontalField = _scriptType.GetProperty("LeftRightAmount", BindingFlags.Public | BindingFlags.Instance);
            _powerField = _scriptType.GetProperty("PowerAmount", BindingFlags.Public | BindingFlags.Instance);
        }
    }
}