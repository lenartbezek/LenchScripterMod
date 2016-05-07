using UnityEngine;

namespace LenchScripter.Internal
{
    /// <summary>
    /// Mark script attached to primitive sphere objects.
    /// Used to mark locations.
    /// </summary>
    public class Mark : MonoBehaviour
    {
        /// <summary>
        /// Should the mark be destroyed at the end of the simulation.
        /// </summary>
        public bool DestroyOnSimulationStop { get; set; } = true;

        private void Awake()
        {
            GetComponent<Renderer>().material.color = Color.red;
            Destroy(GetComponent<SphereCollider>());
        }

        /// <summary>
        /// Implicit conversion to Vector3.
        /// </summary>
        /// <param name="m"></param>
        static public implicit operator Vector3(Mark m)
        {
            return m.transform.position;
        }

        /// <summary>
        /// Explicit conversion to string.
        /// </summary>
        public override string ToString()
        {
            return transform.position.ToString();
        }

        /// <summary>
        /// Moves the mark to the target position.
        /// </summary>
        /// <param name="target">Vector3 target position</param>
        public void move(Vector3 target)
        {
            transform.position = target;
        }

        /// <summary>
        /// Sets the color of the mark.
        /// </summary>
        /// <param name="c">UnityEngine.Color</param>
        public void setColor(Color c)
        {
            GetComponent<Renderer>().material.color = c;
        }

        /// <summary>
        /// Clears the mark.
        /// </summary>
        public void clear(bool manual_call = true)
        {
            if (DestroyOnSimulationStop || manual_call)
            {
                Destroy(gameObject);
                Destroy(this);
            }
        }
    }
}
