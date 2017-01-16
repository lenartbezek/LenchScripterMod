using System.Reflection;
using UnityEngine;

namespace Lench.Scripter.Blocks
{
    /// <summary>
    ///     Handler for cannon blocks; Cannon and Shrapnel Cannon.
    /// </summary>
    public class Vacuum : Block
    {
        private static readonly FieldInfo IsOffInfo = typeof(VacuumBlock).GetField("isOff",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private static readonly MethodInfo CalculateForcePositionsInfo = typeof(VacuumBlock).GetMethod("CalculateForcePositions",
            BindingFlags.NonPublic | BindingFlags.Instance);

        private readonly VacuumBlock _vb;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        public Vacuum(BlockBehaviour bb) : base(bb)
        {
            _vb = bb.GetComponent<VacuumBlock>();
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
                case "ACTIVATE":
                    Toggle();
                    return;
                default:
                    base.Action(actionName);
                    return;
            }
        }

        /// <summary>
        ///     Activates or deactivates the vacuum block.
        /// </summary>
        public void Toggle()
        {
            Active = !Active;
        }

        /// <summary>
        ///     Is vacuum block active.
        /// </summary>
        public bool Active
        {
            get { return !(bool)IsOffInfo.GetValue(_vb); }
            set
            {
                IsOffInfo.SetValue(_vb, !value);
                CalculateForcePositionsInfo.Invoke(_vb, null);

                if (!value) return;
                foreach (var particle in _vb.particle)
                {
                    particle.randomSeed = (uint)Random.Range(0, 9999999);
                    particle.Play();
                }
            }
        }
    }
}