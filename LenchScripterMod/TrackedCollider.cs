using UnityEngine;

namespace Lench.Scripter
{
    /// <summary>
    ///     Tracked collider and hit offset.
    /// </summary>
    public class TrackedCollider
    {
        private readonly Collider _c;
        private Vector3 _lastPosition;
        private readonly Vector3 _offset;

        internal TrackedCollider(Collider hitCollider, Vector3 hitPoint)
        {
            _c = hitCollider;
            _offset = _c.transform.InverseTransformPoint(hitPoint);
            _lastPosition = Position;
            var bb = _c.transform.parent.gameObject.GetComponent<BlockBehaviour>();
            if (bb != null)
                Block = Block.Get(bb);
        }

        /// <summary>
        ///     Returns true if the collider still exists.
        /// </summary>
        public bool Exists => _c != null;

        /// <summary>
        ///     Returns true if the collider represents a building block.
        /// </summary>
        public bool IsBlock => Block != null;

        /// <summary>
        ///     Returns block represented by the collider.
        /// </summary>
        /// <returns></returns>
        public Block Block { get; }

        /// <summary>
        ///     Returns the name of the object represented by the collider.
        ///     Intended for identifying game objects.
        /// </summary>
        /// <returns>Name of the parent transform of the collider.</returns>
        public string Name => _c.transform.parent.name;

        /// <summary>
        ///     Position of the tracked collider with it's offset.
        ///     If the collider no longer exists, returns it's last position.
        /// </summary>
        /// <returns>Vector3 position.</returns>
        public Vector3 Position
        {
            get
            {
                if (Exists)
                    _lastPosition = _c.transform.TransformPoint(_offset);
                return _lastPosition;
            }
        }

        /// <summary>
        ///     Implicit conversion to Vector3.
        /// </summary>
        public static implicit operator Vector3(TrackedCollider tc)
        {
            return tc.Position;
        }

        /// <summary>
        ///     Explicit conversion to string.
        /// </summary>
        public override string ToString()
        {
            return Position.ToString();
        }
    }
}