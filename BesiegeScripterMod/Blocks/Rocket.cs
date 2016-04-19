namespace LenchScripterMod.Blocks
{
    public class Rocket : Block
    {
        private TimedRocket tr;

        internal Rocket(BlockBehaviour bb) : base(bb)
        {
            tr = bb.GetComponent<TimedRocket>();
        }

        public override void action(string actionName)
        {
            actionName = actionName.ToUpper();
            if (actionName == "LAUNCH")
            {
                Launch();
                return;
            }
            throw new ActionNotFoundException("Block " + name + " has no " + actionName + " action.");
        }

        public void Launch()
        {
            tr.hasFired = true;
            tr.StartCoroutine(tr.Fire(0));
        }

        public bool hasFired()
        {
            return tr.hasFired;
        }

        internal static bool isRocket(BlockBehaviour bb)
        {
            return bb.GetComponent<TimedRocket>() != null;
        }
    }
}
