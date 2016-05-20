using System;
using System.Collections.Generic;
using UnityEngine;
using LenchScripter.Blocks;
using LenchScripter.Internal;

namespace LenchScripter
{
    /// <summary>
    /// Used as a wrapper for all Lua accessible functions.
    /// Instantiated at the start of the simulation.
    /// </summary>
    public static class Functions
    {
        // Measuring time
        private static float startTime;

        // List of placed marks
        private static List<Mark> marks;

        /// <summary>
        /// Instantiates the interface that is passed to Lua as besiege object.
        /// </summary>
        static Functions()
        {
            marks = new List<Mark>();
            startTime = Time.time;
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
                return Scripter.Instance.GetBlock(new Guid(blockId));
            }
            catch (FormatException)
            {
                return Scripter.Instance.GetBlock(blockId);
            }
        }

        /// <summary>
        /// Returns true if the block has RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>Boolean value.</returns>
        public static bool Exists(string blockId)
        {
            Block b = GetBlock(blockId);
            return b.Exists;
        }

        /// <summary>
        /// Returns the the time in seconds from the start of the simulation.
        /// Consistent with the in-game time-scale slider.
        /// Useful for calculating the rate of change (speed).
        /// </summary>
        /// <returns>Float value.</returns>
        public static float GetTime()
        {
            return (Time.time - startTime);
        }

        /// <summary>
        /// Adds a global variable to the watchlist.
        /// </summary>
        /// <param name="name">Name of the global variable.</param>
        public static void Watch(string name)
        {
            ScripterMod.Watchlist.AddToWatchlist(name, null, true);
        }

        /// <summary>
        /// Adds a value to watchlist under the specified display name.
        /// </summary>
        /// <param name="name">Display name of the variable.</param>
        /// <param name="value">Variable value to be reported.</param>
        public static void Watch(string name, System.Object value)
        {
            ScripterMod.Watchlist.AddToWatchlist(name, value, false);
        }

        /// <summary>
        /// Clears all entries from the watchlist.
        /// </summary>
        public static void ClearWatchlist()
        {
            ScripterMod.Watchlist.ClearWatchlist();
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
        /// Invokes the block's action.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="actionName">Display name of the action.</param>
        public static void Action(string blockId, string actionName)
        {
            Block b = GetBlock(blockId);
            b.Action(actionName);
        }

        /// <summary>
        /// Sets the toggle mode of the block, specified by the toggle display name.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="toggleName">Toggle property to be set.</param>
        /// <param name="value">Boolean value to be set.</param>
        public static void SetToggleMode(string blockId, string toggleName, bool value)
        {
            Block b = GetBlock(blockId);
            b.SetToggleMode(toggleName, value);
        }

        /// <summary>
        /// Sets the slider value of the block, specified by the slider display name.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="sliderName">Slider value to be set.</param>
        /// <param name="value">Float value to be set.</param>
        public static void SetSliderValue(string blockId, string sliderName, float value)
        {
            Block b = GetBlock(blockId);
            b.SetSliderValue(sliderName, value);
        }

        /// <summary>
        /// Returns the toggle mode of the block, specified by the toggle display name.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="toggleName">Toggle property to be returned.</param>
        /// <returns>Boolean value.</returns>
        public static bool GetToggleMode(string blockId, string toggleName)
        {
            Block b = GetBlock(blockId);
            return b.GetToggleMode(toggleName);
        }

        /// <summary>
        /// Returns the slider value of the block, specified by the slider display name.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="sliderName">Toggle property to be returned.</param>
        /// <returns>Float value.</returns>
        public static float GetSliderValue(string blockId, string sliderName)
        {
            Block b = GetBlock(blockId);
            return b.GetSliderValue(sliderName);
        }

        /// <summary>
        /// Returns the key mapper's minimum slider value, specified by the slider display name.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="sliderName">Minimum slider value to be returned.</param>
        /// <returns>Float value.</returns>
        public static float GetSliderMin(string blockId, string sliderName)
        {
            Block b = GetBlock(blockId);
            return b.GetSliderMin(sliderName);
        }

        /// <summary>
        /// Returns the key mapper's maximum slider value, specified by the slider display name.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="sliderName">Maximum slider value to be returned.</param>
        /// <returns>Float value.</returns>
        public static float GetSliderMax(string blockId, string sliderName)
        {
            Block b = GetBlock(blockId);
            return b.GetSliderMax(sliderName);
        }

        /// <summary>
        /// Adds key to the specified key bind.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="keyName">Key bind to add the key to.</param>
        /// <param name="key">Key value to be added.</param>
        public static void AddKey(string blockId, string keyName, KeyCode key)
        {
            Block b = GetBlock(blockId);
            b.AddKey(keyName, key);
        }

        /// <summary>
        /// Replaces the first key bound to the specified key bind.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="keyName">Key bind to be replaced.</param>
        /// <param name="key">Key value to be replaced with.</param>
        public static void ReplaceKey(string blockId, string keyName, KeyCode key)
        {
            Block b = GetBlock(blockId);
            b.ReplaceKey(keyName, key);
        }

        /// <summary>
        /// Returns the first key value bound of the specified key bind.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="keyName">Key bind to be returned.</param>
        /// <returns>Integer value.</returns>
        public static KeyCode GetKey(string blockId, string keyName)
        {
            Block b = GetBlock(blockId);
            return b.GetKey(keyName);
        }

        /// <summary>
        /// Clears all keys of the specified key bind.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <param name="keyName"></param>
        public static void ClearKeys(string blockId, string keyName)
        {
            Block b = GetBlock(blockId);
            b.ClearKeys(keyName);
        }

        /// <summary>
        /// Returns the block's forward vector.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetForward(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.Forward;
        }

        /// <summary>
        /// Returns the block's up vector.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetUp(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.Up;
        }

        /// <summary>
        /// Returns the block's right vector.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 getRight(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.Right;
        }

        /// <summary>
        /// Returns the block's position vector.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetPosition(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.Position;
        }

        /// <summary>
        /// Returns the block's velocity vector.
        /// Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetVelocity(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.Velocity;
        }

        /// <summary>
        /// Returns the block's mass.
        /// Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static float GetMass(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.Mass;
        }

        /// <summary>
        /// Returns the center of mass of the block, relative to the block's position.
        /// Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetCenterOfMass(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.CenterOfMass;
        }

        /// <summary>
        /// Returns the mass of the machine.
        /// </summary>
        /// <returns>Float value representing total mass.</returns>
        public static float GetMachineMass()
        {
            return Machine.Active().Mass;
        }

        /// <summary>
        /// Returns the center of mass of the machine in the world.
        /// </summary>
        /// <returns>Vector3 position of world COM.</returns>
        public static Vector3 GetMachineCenterOfMass()
        {
            Vector3 center = new Vector3(0, 0, 0);
            for (int i = 0; i < Machine.Active().Blocks.Count; i++)
            {
                Rigidbody body = Machine.Active().Blocks[i].GetComponent<Rigidbody>();
                if(body != null)
                    center += body.worldCenterOfMass * body.mass;
            }
            return center / Machine.Active().Mass;
        }

        /// <summary>
        /// Returns the block's rotation in the form of it's Euler angles.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetEulerAngles(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.EulerAngles;
        }

        /// <summary>
        /// Returns the block's angular velocity.
        /// Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        /// <param name="blockId">Block identifier string.</param>
        /// <returns>UnityEngine.Vector3 vector.</returns>
        public static Vector3 GetAngularVelocity(string blockId = "STARTING BLOCK 1")
        {
            Block b = GetBlock(blockId);
            return b.AngularVelocity;
        }

        /// <summary>
        /// Uses raycast to find out where mouse cursor is pointing.
        /// </summary>
        /// <returns>Returns an x, y, z positional vector of the hit.</returns>
        public static Vector3 GetRaycastHit()
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
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
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.name = "Mark";
            obj.transform.parent = Scripter.Instance.transform;
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
