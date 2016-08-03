using System;
using System.Reflection;

namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Block handler for Pixali's VectorThruster block (ID: 790)
    /// </summary>
    public class VectorThruster : Block
    {

        private static Type scriptType;
        private static FieldInfo codeControlledField;
        private static PropertyInfo isOnField;
        private static PropertyInfo verticalField;
        private static PropertyInfo horizontalField;
        private static PropertyInfo powerField;

        private static void ResolveFieldInfo(object bs)
        {
            scriptType = bs.GetType();
            codeControlledField = scriptType.GetField("codeControlled", BindingFlags.Public | BindingFlags.Instance);
            isOnField = scriptType.GetProperty("IsOn", BindingFlags.Public | BindingFlags.Instance);
            verticalField = scriptType.GetProperty("UpDownAmount", BindingFlags.Public | BindingFlags.Instance);
            horizontalField = scriptType.GetProperty("LeftRightAmount", BindingFlags.Public | BindingFlags.Instance);
            powerField = scriptType.GetProperty("PowerAmount", BindingFlags.Public | BindingFlags.Instance);
        }

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public VectorThruster(BlockBehaviour bb) : base(bb)
        {
            if (scriptType == null) ResolveFieldInfo(bs);
        }

        /// <summary>
        /// Is the vector thruster on.
        /// </summary>
        public bool IsOn
        {
            get
            {
                return (bool)isOnField.GetValue(bs, null);
            }
            set
            {
                isOnField.SetValue(bs, value, null);
            }
        }

        /// <summary>
        /// Is the vector thruster code controlled.
        /// Must be set to true to prevent it responding to keyboard presses.
        /// </summary>
        public bool CodeControlled
        {
            get
            {
                return (bool)codeControlledField.GetValue(bs);
            }
            set
            {
                codeControlledField.SetValue(bs, value);
            }
        }

        /// <summary>
        /// Thrusters vertical offset bias.
        /// </summary>
        public float VerticalBias
        {
            get
            {
                return (float)verticalField.GetValue(bs, null);
            }
            set
            {
                verticalField.SetValue(bs, value, null);
            }
        }

        /// <summary>
        /// Thrusters horizontal offset bias.
        /// </summary>
        public float HorizontalBias
        {
            get
            {
                return (float)horizontalField.GetValue(bs, null);
            }
            set
            {
                horizontalField.SetValue(bs, value, null);
            }
        }

        /// <summary>
        /// Thrusters power.
        /// </summary>
        public float Power
        {
            get
            {
                return (float)powerField.GetValue(bs, null);
            }
            set
            {
                powerField.SetValue(bs, value, null);
            }
        }
    }
}
