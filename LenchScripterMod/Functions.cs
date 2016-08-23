using System;
using System.Collections.Generic;
using UnityEngine;
using Lench.Scripter.Blocks;
using Lench.Scripter.Internal;

namespace Lench.Scripter
{
    /// <summary>
    /// Used as a wrapper for all Python accessible functions.
    /// Instantiated at the start of the simulation.
    /// </summary>
    public static class Functions
    {
        // List of placed marks
        private static List<Mark> marks = new List<Mark>();

        // Measuring time since the start of simulation
        private static float startTime = 0;

        /// <summary>
        /// Resets the timer returned by GetTime()
        /// </summary>
        public static void ResetTimer()
        {
            startTime = Time.time;
        }

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
        public static Lench.Scripter.Blocks.BlockHandler GetBlock(string blockId)
        {
            try
            {
                return BlockHandlerController.GetBlock(new Guid(blockId));
            }
            catch (FormatException)
            {
                return BlockHandlerController.GetBlock(blockId);
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
                Lench.Scripter.Blocks.BlockHandler b = GetBlock(blockId);
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
            return Time.time - startTime;
        }

        /// <summary>
        /// Adds a global variable to the watchlist.
        /// </summary>
        /// <param name="name">Name of the global variable.</param>
        public static void Watch(string name)
        {
            Watchlist.Instance.AddToWatchlist(name, null, true);
        }

        /// <summary>
        /// Adds a value to watchlist under the specified display name.
        /// </summary>
        /// <param name="name">Display name of the variable.</param>
        /// <param name="value">Variable value to be reported.</param>
        public static void Watch(string name, object value)
        {
            Watchlist.Instance.AddToWatchlist(name, value, false);
        }

        /// <summary>
        /// Clears all entries from the watchlist.
        /// </summary>
        public static void ClearWatchlist()
        {
            Watchlist.Instance.ClearWatchlist();
        }

        /// <summary>
        /// Toggles all functions to return angles in degrees.
        /// </summary>
        public static void UseDegrees()
        {
            Lench.Scripter.Blocks.BlockHandler.UseDegrees();
        }

        /// <summary>
        /// Toggles all functions to returns angles in radians.
        /// </summary>
        public static void UseRadians()
        {
            Lench.Scripter.Blocks.BlockHandler.UseRadians();
            
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
        public static Mark CreateMark(Vector3 pos)
        {
            Debug.Log("Creating mark at " + pos);
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "Mark";
            obj.transform.parent = Internal.Scripter.Instance.transform;
            Mark m = obj.AddComponent<Mark>();
            m.Move(pos);
            marks.Add(m);
            return m;
        }

        /// <summary>
        /// Clears all marks.
        /// Called by user or at the end of the simulation.
        /// </summary>
        public static void ClearMarks(bool manual_call = true)
        {
            foreach (Mark m in marks)
            {
                m.Clear(manual_call);
            }
            marks.Clear();
        }
    }
}
