namespace LenchScripterMod.Blocks
{
    public class Decoupler : Block
    {
        private ExplosiveBolt eb;

        internal Decoupler(BlockBehaviour bb) : base(bb)
        {
            eb = bb.GetComponent<ExplosiveBolt>();
        }

        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "EXPLODE")
            {
                Explode();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        public void Explode()
        {
            eb.Explode();
        }

        internal static bool isDecoupler(BlockBehaviour bb)
        {
            return bb.GetComponent<ExplosiveBolt>() != null;
        }
    }
}
