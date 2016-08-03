using System.Reflection;
using UnityEngine;

namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for the Flying Spiral block.
    /// </summary>
    public class FlyingSpiral : Block
    {
        private static FieldInfo flying = typeof(FlyingController).GetField("flying", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo speedToGo = typeof(FlyingController).GetField("speedToGo", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo lerpySpeed = typeof(FlyingController).GetField("lerpySpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo lerpedSpeed = typeof(FlyingController).GetField("lerpedSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo automaticFieldInfo = typeof(FlyingController).GetField("automaticToggle", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo toggleFieldInfo = typeof(FlyingController).GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo reverseFieldInfo = typeof(FlyingController).GetField("reverseToggle", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo rigidbodyFieldInfo = typeof(FlyingController).GetField("myRigidbody", BindingFlags.NonPublic | BindingFlags.Instance);

        private FlyingController fc;

        private MToggle automaticToggle;
        private MToggle toggleMode;
        private MToggle reverseToggle;
        private Rigidbody rigidbody;

        private bool setFlyingFlag = false;
        private bool lastFlyingFlag = false;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public FlyingSpiral(BlockBehaviour bb) : base(bb)
        {
            fc = bb.GetComponent<FlyingController>();
            automaticToggle = automaticFieldInfo.GetValue(fc) as MToggle;
            toggleMode = toggleFieldInfo.GetValue(fc) as MToggle;
            reverseToggle = reverseFieldInfo.GetValue(fc) as MToggle;
            rigidbody = rigidbodyFieldInfo.GetValue(fc) as Rigidbody;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "SPIN")
            {
                Spin();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Spin the Flying Spiral.
        /// </summary>
        public void Spin()
        {
            setFlyingFlag = true;
        }

        private void Fly(bool f)
        {
            if (f && !fc.isFrozen && fc.canFly)
            {
                speedToGo.SetValue(fc, fc.speed);
                lerpySpeed.SetValue(fc, fc.lerpSpeed + Random.Range(-2, 3));
                rigidbody.drag = 1.5f;
                flying.SetValue(fc, true);
            }
            else
            {
                speedToGo.SetValue(fc, Vector3.zero);
                rigidbody.drag = 0.5f;
                flying.SetValue(fc, false);
            }
        }

        /// <summary>
        /// Sets the speed and drag of the block to make it fly.
        /// </summary>
        protected override void Update()
        {
            if (setFlyingFlag)
            {
                if (toggleMode.IsActive)
                    Fly(!(bool)flying.GetValue(fc));
                else
                    Fly(true);
                setFlyingFlag = false;
                lastFlyingFlag = true;
            }
            else if (lastFlyingFlag)
            {
                if (!toggleMode.IsActive)
                    Fly(false);
                lastFlyingFlag = false;
            }
        }
    }

}
