using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Flying Spiral block.
    /// </summary>
    public class FlyingSpiral : Block
    {
        private FlyingController fc;

        private MToggle automaticToggle;
        private MToggle toggleMode;
        private MToggle reverseToggle;
        private Rigidbody rigidbody;

        private FieldInfo flying;
        private FieldInfo speedToGo;
        private FieldInfo lerpySpeed;
        private FieldInfo lerpedSpeed;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            fc = bb.GetComponent<FlyingController>();

            FieldInfo automaticFieldInfo = fc.GetType().GetField("automaticToggle", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo toggleFieldInfo = fc.GetType().GetField("toggleMode", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo reverseFieldInfo = fc.GetType().GetField("reverseToggle", BindingFlags.NonPublic | BindingFlags.Instance);
            FieldInfo rigidbodyFieldInfo = fc.GetType().GetField("myRigidbody", BindingFlags.NonPublic | BindingFlags.Instance);

            automaticToggle = automaticFieldInfo.GetValue(fc) as MToggle;
            toggleMode = toggleFieldInfo.GetValue(fc) as MToggle;
            reverseToggle = reverseFieldInfo.GetValue(fc) as MToggle;
            rigidbody = rigidbodyFieldInfo.GetValue(fc) as Rigidbody;

            flying = fc.GetType().GetField("flying", BindingFlags.NonPublic | BindingFlags.Instance);
            speedToGo = fc.GetType().GetField("speedToGo", BindingFlags.NonPublic | BindingFlags.Instance);
            lerpySpeed = fc.GetType().GetField("lerpySpeed", BindingFlags.NonPublic | BindingFlags.Instance);
            lerpedSpeed = fc.GetType().GetField("lerpedSpeed", BindingFlags.NonPublic | BindingFlags.Instance);
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "SPIN")
            {
                Spin();
                return;
            }
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Spin the Flying Spiral.
        /// </summary>
        public void Spin()
        {
            if (!fc.canFly)
            {
                flying.SetValue(fc, false);
            }
            else
            {
                if (!automaticToggle.IsActive)
                {
                    if (!(bool)flying.GetValue(fc))
                    {
                        speedToGo.SetValue(fc, fc.speed);
                        lerpySpeed.SetValue(fc, fc.lerpSpeed + Random.Range(-2, 3));
                        rigidbody.drag = 1.5f;
                        flying.SetValue(fc, true);
                    }
                }
                else if (toggleMode.IsActive)
                {
                    if (!fc.isFrozen)
                    {
                        speedToGo.SetValue(fc, fc.speed);
                        lerpySpeed.SetValue(fc, fc.lerpSpeed + Random.Range(-2, 3));
                        rigidbody.drag = 1.5f;
                        flying.SetValue(fc, true);
                    }
                }
                else if (!fc.isFrozen)
                {
                    if (!(bool)flying.GetValue(fc))
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
                if ((Vector3)speedToGo.GetValue(fc) != Vector3.zero || (Vector3)lerpedSpeed.GetValue(fc) != Vector3.zero)
                {
                    lerpedSpeed.SetValue(fc, Vector3.Lerp((Vector3)lerpedSpeed.GetValue(fc), (Vector3)speedToGo.GetValue(fc), Time.deltaTime * (float)lerpySpeed.GetValue(fc)));
                    fc.spinObj.Rotate((((Vector3)lerpedSpeed.GetValue(fc) * Time.deltaTime) * -1) * (!reverseToggle.IsActive ? 1f : -1f));
                }
            }
        }

        internal static bool isFlyingSpiral(BlockBehaviour bb)
        {
            return bb.GetComponent<FlyingController>() != null;
        }
    }

}
