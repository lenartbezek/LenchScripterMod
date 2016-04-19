using System.Reflection;

namespace LenchScripterMod.Blocks
{
    public class Grabber : Block
    {
        private GrabberBlock gb;
        private JoinOnTriggerBlock joint;

        internal Grabber(BlockBehaviour bb) : base(bb)
        {
            gb = bb.GetComponent<GrabberBlock>();
            FieldInfo joinFieldInfo = gb.GetType().GetField("joinOnTriggerBlock", BindingFlags.NonPublic | BindingFlags.Instance);
            joint = joinFieldInfo.GetValue(gb) as JoinOnTriggerBlock;
        }

        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "DETACH")
            {
                Detach();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

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
