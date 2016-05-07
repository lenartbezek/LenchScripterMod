using System.Reflection;

namespace LenchScripter.Blocks
{
    /// <summary>
    /// Handler for the Flamethrower block.
    /// </summary>
    public class Flamethrower : Block
    {
        private static FieldInfo holdFieldInfo = typeof(FlamethrowerController).GetField("holdToFire", BindingFlags.NonPublic | BindingFlags.Instance);
        private static FieldInfo keyHeld = typeof(FlamethrowerController).GetField("keyHeld", BindingFlags.NonPublic | BindingFlags.Instance);

        private FlamethrowerController fc;
        private MToggle holdToFire;

        private bool setIgniteFlag = false;
        private bool lastIgniteFlag = false;

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Flamethrower(BlockBehaviour bb) : base(bb)
        {
            fc = bb.GetComponent<FlamethrowerController>();
            holdToFire = holdFieldInfo.GetValue(fc) as MToggle;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "IGNITE")
            {
                Ignite();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Ignite the flamethrower.
        /// </summary>
        public void Ignite()
        {
            setIgniteFlag = true;
        }

        /// <summary>
        /// Sets the remaining time of the flamethrower.
        /// </summary>
        /// <param name="t">Float value; time in seconds.</param>
        public void setRemainingTime(float t)
        {
            fc.timey = 10 - t;
        }

        /// <summary>
        /// Returns the remaining time of the flamethrower.
        /// </summary>
        /// <returns>Float value; time in seconds.</returns>
        public float getRemainingTime()
        {
            return 10 - fc.timey;
        }

        /// <summary>
        /// Handles igniting the Flamethrower.
        /// </summary>
        protected override void LateUpdate()
        {
            if (setIgniteFlag)
            {
                if (!fc.timeOut || STATLORD.infiniteAmmoMode)
                {
                    if (holdToFire.IsActive)
                    {
                        fc.FlameOn();
                    }
                    else
                    {
                        fc.Flame();
                    }
                }
                setIgniteFlag = false;
                lastIgniteFlag = true;
            }
            else if(lastIgniteFlag)
            {
                keyHeld.SetValue(fc, true);
                lastIgniteFlag = false;
            }
        }
    }
}
