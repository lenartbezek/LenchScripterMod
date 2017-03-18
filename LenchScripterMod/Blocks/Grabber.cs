using System.Reflection;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the Grabber block.
    /// </summary>
    public class Grabber : Block
    {
        private static readonly FieldInfo JoinFieldInfo = typeof(GrabberBlock).GetField("joinOnTriggerBlock",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly JoinOnTriggerBlock _join;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Grabber(BlockBehaviour bb) : base(bb)
        {
            var gb = bb.GetComponent<GrabberBlock>();
            _join = JoinFieldInfo.GetValue(gb) as JoinOnTriggerBlock;
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
                case "DETACH":
                    Detach();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Detach or grab with the Grabber.
        /// </summary>
        public void Detach()
        {
            _join.OnKeyPressed();
        }
    }
}