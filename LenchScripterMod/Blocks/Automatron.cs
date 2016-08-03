namespace Lench.AdvancedControls.Blocks
{
    /// <summary>
    /// Handler for spaar's Automatron block.
    /// </summary>
    public class Automatron : Block
    {

        /// <summary>
        /// Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Automatron(BlockBehaviour bb) : base(bb) { }

        /// <summary>
        /// Invokes the block's action.
        /// Throws ActionNotFoundException if the block does not posess such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public override void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "ACTIVATE")
            {
                Activate();
                return;
            }
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        /// Triggers the block.
        /// </summary>
        public void Activate()
        {
            bs.SendMessage("TriggerActions");
        }
    }
}
