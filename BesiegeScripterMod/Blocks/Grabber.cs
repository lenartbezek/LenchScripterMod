using System.Reflection;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for the Grabber block.
    /// </summary>
    public class Grabber : Block
    {
        private GrabberBlock gb;
        private JoinOnTriggerBlock joint;

        internal override void Initialize(BlockBehaviour bb)
        {
            base.Initialize(bb);
            gb = bb.GetComponent<GrabberBlock>();
            FieldInfo joinFieldInfo = gb.GetType().GetField("joinOnTriggerBlock", BindingFlags.NonPublic | BindingFlags.Instance);
            joint = joinFieldInfo.GetValue(gb) as JoinOnTriggerBlock;
        }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "DETACH")
            {
                Detach();
                return;
            }
            throw new ActionNotFoundException("Block " + blockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Detach or grab with the Grabber.
        /// </summary>
        public void Detach()
        {
            if (joint.isJoined)
            {
                joint.BreakJoint();
            }
            else
            {
                joint.canGrabTimer = 0.05f;
            }
        }

        internal static bool isGrabber(BlockBehaviour bb)
        {
            return bb.GetComponent<GrabberBlock>() != null;
        }
    }
}
