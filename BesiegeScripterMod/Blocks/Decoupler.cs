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
