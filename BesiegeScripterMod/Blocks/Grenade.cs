namespace LenchScripterMod.Blocks
{
    public class Grenade : Block
    {
        private ControllableBomb cb;

        internal Grenade(BlockBehaviour bb) : base(bb)
        {
            cb = bb.GetComponent<ControllableBomb>();
        }

        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "DETONATE")
            {
                Detonate();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        public void Detonate()
        {
            cb.StartCoroutine_Auto(cb.Explode());
        }

        internal static bool isGrenade(BlockBehaviour bb)
        {
            return bb.GetComponent<ControllableBomb>() != null;
        }
    }
}
