using UnityEngine;

namespace LenchScripter.Internal
{
    /// <summary>
    /// Tracked collider and hit offset.
    /// </summary>
    public class TrackedCollider
    {
        private Collider c;
        private Vector3 offset;
        private Vector3 lastPosition;

        internal TrackedCollider(Collider hitCollider, Vector3 hitPoint)
        {
            c = hitCollider;
            offset = c.transform.InverseTransformPoint(hitPoint);
            lastPosition = getPosition();
        }

        /// <summary>
        /// Implicit conversion to Vector3.
        /// </summary>
        /// <param name="tc"></param>
        static public implicit operator Vector3(TrackedCollider tc)
        {
            return tc.getPosition();
        }

        /// <summary>
        /// Explicit conversion to string.
        /// </summary>
        public override string ToString()
        {
            return getPosition().ToString();
        }

        /// <summary>
        /// Returns true if the collider still exists.
        /// </summary>
        /// <returns></returns>
        public bool exists()
        {
            return c != null;
        }

        /// <summary>
        /// Returns the position of the tracked collider with it's offset.
        /// If the collider no longer exists, returns it's last position.
        /// </summary>
        /// <returns>Vector3 position.</returns>
        public Vector3 getPosition()
        {
            if (exists())
            {
                lastPosition = c.transform.TransformPoint(offset);
            }
            return lastPosition;
        }
    }
}
