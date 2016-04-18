using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    public class Cog : Block
    {
        private CogMotorController cmc;

        internal Cog(BlockBehaviour bb) : base(bb)
        {
            cmc = bb.GetComponent<CogMotorController>();
        }

        internal static bool isCog(BlockBehaviour bb)
        {
            return bb.GetComponent<CogMotorController>() != null;
        }
    }
}
