using Lench.AdvancedControls.Blocks;
using UnityEngine;

namespace Lench.AdvancedControls
{
    /// <summary>
    /// Tracked collider and hit offset.
    /// </summary>
    public class TrackedCollider
    {
        private Collider c;
        private Block block;
        private Vector3 offset;
        private Vector3 lastPosition;

        internal TrackedCollider(Collider hitCollider, Vector3 hitPoint)
        {
            c = hitCollider;
            offset = c.transform.InverseTransformPoint(hitPoint);
            lastPosition = Position;
            var bb = c.transform.parent.gameObject.GetComponent<BlockBehaviour>();
            if (bb != null)
                block = BlockHandlers.GetBlock(bb);
        }

        /// <summary>
        /// Implicit conversion to Vector3.
        /// </summary>
        /// <param name="tc"></param>
        static public implicit operator Vector3(TrackedCollider tc)
        {
            return tc.Position;
        }

        /// <summary>
        /// Explicit conversion to string.
        /// </summary>
        public override string ToString()
        {
            return Position.ToString();
        }

        /// <summary>
        /// Returns true if the collider still exists.
        /// </summary>
        public bool Exists
        {
            get { return c != null; }
        }

        /// <summary>
        /// Returns true if the collider represents a building block.
        /// </summary>
        public bool IsBlock
        {
            get { return block != null; }
        }

        /// <summary>
        /// Returns block represented by the collider.
        /// </summary>
        /// <returns></returns>
        public Block Block
        {
            get { return block; }
        }

        /// <summary>
        /// Returns the name of the object represented by the collider.
        /// Intended for identifying game objects.
        /// </summary>
        /// <returns></returns>
        public string Name
        {
            get { return c.transform.parent.name; }
        }

        /// <summary>
        /// Position of the tracked collider with it's offset.
        /// If the collider no longer exists, returns it's last position.
        /// </summary>
        /// <returns>Vector3 position.</returns>
        public Vector3 Position
        {
            get
            {
                if (Exists)
                {
                    lastPosition = c.transform.TransformPoint(offset);
                }
                return lastPosition;
            }
        }
    }
}
