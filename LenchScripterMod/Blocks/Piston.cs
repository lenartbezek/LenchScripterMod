using System.Reflection;
using UnityEngine;

namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for the Piston block.
    /// </summary>
    public class Piston : Block
    {
        private static FieldInfo toggleFieldInfo = typeof(SliderCompress).GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo extendFieldInfo = typeof(SliderCompress).GetField("extendKey", BindingFlags.NonPublic | BindingFlags.Instance);

        private SliderCompress sc;
        private MToggle toggleMode;
        private MKey extendKey;

        private bool setExtendFlag = false;
        private bool lastExtendFlag = false;
        private bool setPositionFlag = false;
        private float targetPosition;

        private float defaultStartLimit;
        private float defaultNewLimit;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Piston(BlockBehaviour bb) : base(bb)
        {
            sc = bb.GetComponent<SliderCompress>();

            toggleMode = toggleFieldInfo.GetValue(sc) as MToggle;
            extendKey = extendFieldInfo.GetValue(sc) as MKey;

            defaultStartLimit = sc.startLimit;
            defaultNewLimit = sc.newLimit;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "EXTEND")
            {
                Extend();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Extend the piston.
        /// </summary>
        public void Extend()
        {
            if (toggleMode.IsActive)
            {
                sc.posToBe = (sc.posToBe != sc.newLimit ? sc.newLimit : sc.startLimit);
            }
            else
            {
                setExtendFlag = true;
            }
        }

        /// <summary>
        /// Set the position between compressed and extended position.
        /// </summary>
        /// <param name="t"></param>
        public void SetPosition(float t)
        {
            targetPosition = Mathf.Lerp(defaultStartLimit, defaultNewLimit, t);
            setPositionFlag = true;
        }

        /// <summary>
        /// Handles extending and compressing the piston.
        /// </summary>
        protected override void Update()
        {
            if (setExtendFlag)
            {
                if (!extendKey.IsDown)
                {
                    sc.startLimit = defaultNewLimit;
                    sc.newLimit = defaultStartLimit;
                }
                setExtendFlag = false;
                lastExtendFlag = true;
                setPositionFlag = false;
            }
            else if (setPositionFlag)
            {
                if (toggleMode.IsActive)
                {
                    sc.posToBe = targetPosition;
                }
                else
                {
                    sc.startLimit = targetPosition;
                    sc.newLimit = targetPosition;
                }
                lastExtendFlag = true;
                setPositionFlag = false;
            }
            else if (lastExtendFlag)
            {
                sc.startLimit = defaultStartLimit;
                sc.newLimit = defaultNewLimit;
                lastExtendFlag = false;
            }
        }
    }
}
