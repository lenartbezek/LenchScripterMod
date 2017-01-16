namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for the Explosive Decoupler block.
    /// </summary>
    public class Decoupler : Block
    {
        private readonly ExplosiveBolt _eb;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Decoupler(BlockBehaviour bb) : base(bb)
        {
            _eb = bb.GetComponent<ExplosiveBolt>();
        }

        /// <summary>
        ///     Sets the decoupler explode power.
        /// </summary>
        public float ExplodePower
        {
            get { return _eb.explodePower; }
            set { _eb.explodePower = value; }
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
                case "EXPLODE":
                    Explode();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Explode the decoupler.
        /// </summary>
        public void Explode()
        {
            try
            {
                _eb.Explode();
            }
            catch
            {
                // Calling Explode imediatelly after simulation start will throw a NullReferenceException
            }
        }
    }
}