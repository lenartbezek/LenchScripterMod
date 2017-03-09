using System;
using System.Collections.Generic;
using System.Linq;
using Lench.Scripter.Internal;
using UnityEngine;

namespace Lench.Scripter
{
    /// <summary>
    ///     Used as a wrapper for all Python accessible functions.
    ///     Instantiated at the start of the simulation.
    /// </summary>
    public static class Functions
    {
        // List of placed marks
        private static readonly List<Mark> Marks = new List<Mark>();

        // Measuring time since the start of simulation
        private static float _startTime;

        /// <summary>
        ///     Returns the mass of the machine.
        /// </summary>
        /// <returns>Float value representing total mass.</returns>
        public static float MachineMass => Machine.Active().Mass;

        /// <summary>
        ///     Returns the center of mass of the machine in the world.
        /// </summary>
        /// <returns>Vector3 position of world COM.</returns>
        public static Vector3 MachineCenterOfMass
        {
            get
            {
                var center = Machine.Active().Blocks
                    .Select(bb => bb.GetComponent<Rigidbody>())
                    .Where(body => body != null)
                    .Aggregate(Vector3.zero, (current, body) => current + body.worldCenterOfMass * body.mass);
                return center / Machine.Active().Mass;
            }
        }

        /// <summary>
        ///     Resets the timer returned by GetTime()
        /// </summary>
        public static void ResetTimer()
        {
            _startTime = Time.time;
        }

        /// <summary>
        ///     Logs message into console.
        /// </summary>
        /// <param name="message"></param>
        public static void Log(object message)
        {
            Debug.Log(message.ToString());
        }

        /// <summary>
        ///     Returns the block's handler.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>Block object.</returns>
        public static Block GetBlock(string blockId)
        {
            try
            {
                return Block.Get(new Guid(blockId));
            }
            catch (FormatException)
            {
                return Block.Get(blockId);
            }
        }

        /// <summary>
        ///     Returns true if the block exists and has RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>Boolean value.</returns>
        public static bool Exists(string blockId)
        {
            try
            {
                var b = GetBlock(blockId);
                return b.Exists;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        ///     Returns the the time in seconds from the start of the simulation.
        ///     Consistent with the in-game time-scale slider.
        ///     Useful for calculating the rate of change (speed).
        /// </summary>
        /// <returns>Float value.</returns>
        public static float GetTime()
        {
            return Time.time - _startTime;
        }

        /// <summary>
        ///     Adds a global variable to the watchlist.
        /// </summary>
        /// <param name="name">Name of the global variable.</param>
        public static void Watch(string name)
        {
            Watchlist.Add(name, null, true);
        }

        /// <summary>
        ///     Adds a value to watchlist under the specified display name.
        /// </summary>
        /// <param name="name">Display name of the variable.</param>
        /// <param name="value">Variable value to be reported.</param>
        public static void Watch(string name, object value)
        {
            Watchlist.Add(name, value);
        }

        /// <summary>
        ///     Clears all entries from the watchlist.
        /// </summary>
        public static void ClearWatchlist()
        {
            Watchlist.Clear();
        }

        /// <summary>
        ///     Uses raycast to find out where mouse cursor is pointing.
        /// </summary>
        /// <returns>Returns an x, y, z positional vector of the hit.</returns>
        public static Vector3 GetRaycastHit()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                return hit.point;
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        ///     Casts ray defined by origin and direction vectors.
        /// </summary>
        /// <param name="origin">Origin vector of the raycast.</param>
        /// <param name="direction">Direction vector of the raycast.</param>
        /// <returns>Returns position of the hit.</returns>
        public static Vector3 GetRaycastHit(Vector3 origin, Vector3 direction)
        {
            var ray = new Ray(origin, direction.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit))
                return hit.point;
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        ///     Uses raycast to find out what collider the mouse cursor is pointing at.
        ///     If not sucessfull, returns zero vector.
        /// </summary>
        /// <returns>Returns an x, y, z positional vector of the hit.</returns>
        public static TrackedCollider GetRaycastCollider()
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
                return new TrackedCollider(hit.collider, hit.point);
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        ///     Casts ray defined by origin and direction vectors.
        /// </summary>
        /// <param name="origin">Origin vector of the raycast.</param>
        /// <param name="direction">Direction vector of the raycast.</param>
        /// <returns>Returns TrackedCollider object of the hit.</returns>
        public static TrackedCollider GetRaycastCollider(Vector3 origin, Vector3 direction)
        {
            var ray = new Ray(origin, direction.normalized);
            if (Physics.Raycast(ray, out RaycastHit hit))
                return new TrackedCollider(hit.collider, hit.point);
            throw new Exception("Your raycast does not intersect with a collider.");
        }

        /// <summary>
        ///     Creates a mark at a given position.
        /// </summary>
        /// <param name="pos">Vector3 specifying position.</param>
        /// <returns>Reference to the mark.</returns>
        public static Mark CreateMark(Vector3 pos)
        {
            var obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "Mark";
            obj.transform.parent = Mod.Controller.transform;
            var m = obj.AddComponent<Mark>();
            m.Move(pos);
            Marks.Add(m);
            return m;
        }

        /// <summary>
        ///     Clears all marks.
        ///     Called by user or at the end of the simulation.
        /// </summary>
        public static void ClearMarks(bool manualCall = true)
        {
            foreach (var m in Marks)
                m.Clear(manualCall);
            Marks.Clear();
        }
    }
}