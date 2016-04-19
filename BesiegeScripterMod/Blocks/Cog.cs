using System.Reflection;
using UnityEngine;

namespace LenchScripterMod.Blocks
{
    /// <summary>
    /// Handler for all wheel and cog blocks.
    /// </summary>
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
