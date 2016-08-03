using System;
using UnityEngine;
using Lench.AdvancedControls.Blocks;

namespace Lench.AdvancedControls
{
    /// <summary>
    /// Used as a wrapper for all Python accessible functions.
    /// Instantiated at the start of the simulation.
    /// </summary>
    public static class Functions
    {
        /// <summary>
        /// Logs message into console.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object message)
        {
            Debug.Log(message.ToString());
        }

        /// <summary>
        /// Returns the block's handler.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>Block object.</returns>
        public static Block GetBlock(string blockId)
        {
            try
            {
                return BlockHandlers.GetBlock(new Guid(blockId));
            }
            catch (FormatException)
            {
                return BlockHandlers.GetBlock(blockId);
            }
        }

        /// <summary>
        /// Returns true if the block exists and has RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>Boolean value.</returns>
        public static bool Exists(string blockId)
        {
            try
            {
                Block b = GetBlock(blockId);
                return b.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the the time in seconds from the start of the simulation.
        /// Consistent with the in-game time-scale slider.
        /// Useful for calculating the rate of change (speed).
        /// </summary>
        /// <returns>Float value.</returns>
        public static float GetTime()
        {
            return Time.time;
        }

        /// <summary>
        /// Adds a global variable to the watchlist.
        /// </summary>
        /// <param name="name">Name of the global variable.</param>
        public static void Watch(string name)
        {
            throw new InvalidOperationException("Watchlist is unavailable in ACM.");
        }

        /// <summary>
        /// Adds a value to watchlist under the specified display name.
        /// </summary>
        /// <param name="name">Display name of the variable.</param>
        /// <param name="value">Variable value to be reported.</param>
        public static void Watch(string name, object value)
        {
            throw new InvalidOperationException("Watchlist is unavailable in ACM.");
        }

        /// <summary>
        /// Clears all entries from the watchlist.
        /// </summary>
        public static void ClearWatchlist()
        {
            throw new InvalidOperationException("Watchlist is unavailable in ACM.");
        }

        /// <summary>
        /// Toggles all functions to return angles in degrees.
        /// </summary>
        public static void UseDegrees()
        {
            Block.UseDegrees();
        }

        /// <summary>
        /// Toggles all functions to returns angles in radians.
        /// </summary>
        public static void UseRadians()
        {
            Block.UseRadians();
        }

        /// <summary>
        /// Returns the mass of the machine.
        /// </summary>
        /// <returns>Float value representing total mass.</returns>
        public static float MachineMass { get { return Machine.Active().Mass; } }

        /// <summary>
        /// Returns the center of mass of the machine in the world.
        /// </summary>
        /// <returns>Vector3 position of world COM.</returns>
        public static Vector3 MachineCenterOfMass
        {
            get
            {
                Vector3 center = Vector3.zero;
                for (int i = 0; i < Machine.Active().Blocks.Count; i++)
                {
                    Rigidbody body = Machine.Active().Blocks[i].GetComponent<Rigidbody>();
                    if (body != null)
                        center += body.worldCenterOfMass * body.mass;
                }
                return center / Machine.Active().Mass;
            }
        }

        /// <summary>
        /// Uses raycast to find out where mouse cursor is pointing.
        /// </summary>
        /// <returns>Returns an x, y, z positional vector of the hit.</returns>
        public static Vector3 GetRaycastHit()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        /// Casts ray defined by origin and direction vectors.
        /// </summary>
        /// <param name="origin">Origin vector of the raycast.</param>
        /// <param name="direction">Direction vector of the raycast.</param>
        /// <returns>Returns position of the hit.</returns>
        public static Vector3 GetRaycastHit(Vector3 origin, Vector3 direction)
        {
            RaycastHit hit;
            Ray ray = new Ray(origin, direction.normalized);
            if (Physics.Raycast(ray, out hit))
            {
                return hit.point;
            }
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        /// Uses raycast to find out what collider the mouse cursor is pointing at.
        /// If not sucessfull, returns zero vector.
        /// </summary>
        /// <returns>Returns an x, y, z positional vector of the hit.</returns>
        public static TrackedCollider GetRaycastCollider()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                return new TrackedCollider(hit.collider, hit.point);
            }
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        /// Casts ray defined by origin and direction vectors.
        /// </summary>
        /// <param name="origin">Origin vector of the raycast.</param>
        /// <param name="direction">Direction vector of the raycast.</param>
        /// <returns>Returns TrackedCollider object of the hit.</returns>
        public static TrackedCollider GetRaycastCollider(Vector3 origin, Vector3 direction)
        {
            RaycastHit hit;
            Ray ray = new Ray(origin, direction.normalized);
            if (Physics.Raycast(ray, out hit))
            {
                return new TrackedCollider(hit.collider, hit.point);
            }
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        /// Creates a mark at a given position.
        /// </summary>
        /// <param name="pos">Vector3 specifying position.</param>
        /// <returns>Reference to the mark.</returns>
        public static void CreateMark(Vector3 pos)
        {
            throw new InvalidOperationException("Mark functionality is unavailable in ACM.");
        }

        /// <summary>
        /// Clears all marks.
        /// Called by user or at the end of the simulation.
        /// </summary>
        public static void ClearMarks(bool manual_call = true)
        {
            throw new InvalidOperationException("Mark functionality is unavailable in ACM.");
        }
    }
}
