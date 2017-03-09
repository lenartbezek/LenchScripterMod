using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
// ReSharper disable SpecifyStringComparison

namespace Lench.Scripter
{
    /// <summary>
    ///     Base class for all block handlers.
    /// </summary>
    public partial class Block
    {
        /// <summary>
        ///     BlockBehaviour object of this handler.
        /// </summary>
        protected readonly BlockBehaviour Bb;

        /// <summary>
        ///     BlockLoaders BlockScript object.
        /// </summary>
        protected readonly MonoBehaviour Bs;

        /// <summary>
        ///     Creates a Block handler.
        /// </summary>
        public Block(BlockBehaviour bb)
        {
            Bb = bb;
            Bs = bb.GetComponent<BlockScript>();

            OnUpdate += Update;
            OnLateUpdate += LateUpdate;
            OnFixedUpdate += FixedUpdate;
        }

        /// <summary>
        ///     Name of the block. Corresponds to MyBlockInfo.BlockName.
        /// </summary>
        public string BlockName
        {
            get { return Bb.GetComponent<MyBlockInfo>().blockName; }
            set { Bb.GetComponent<MyBlockInfo>().blockName = value; }
        }

        /// <summary>
        ///     Type of the block.
        /// </summary>
        public BlockType BlockType => (BlockType) Bb.GetBlockID();

        /// <summary>
        ///     Returns true if the block has RigidBody.
        /// </summary>
        public virtual bool Exists => Bb != null && Bb.GetComponent<Rigidbody>() != null;

        public virtual Vector3 Forward => Bb.transform.forward;
        public virtual Vector3 Up => Bb.transform.up;
        public virtual Vector3 Right => Bb.transform.right;

        /// <summary>
        ///     Position of the block.
        /// </summary>
        public virtual Vector3 Position => Bb.transform.position;

        /// <summary>
        ///     Velocity of the block in units per second.
        ///     Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        public virtual Vector3 Velocity
        {
            get
            {
                var body = Bb.GetComponent<Rigidbody>();
                if (body != null)
                    return body.velocity;
                throw new NoRigidBodyException("Block " + BlockName + " has no rigid body.");
            }
        }

        /// <summary>
        ///     Mass of the block.
        /// </summary>
        public virtual float Mass
        {
            get
            {
                var body = Bb.GetComponent<Rigidbody>();
                if (body != null)
                    return body.mass;
                throw new NoRigidBodyException("Block " + BlockName + " has no rigid body.");
            }
        }

        /// <summary>
        ///     Center of mass of the block, relative to the block's position.
        /// </summary>
        public virtual Vector3 CenterOfMass
        {
            get
            {
                var body = Bb.GetComponent<Rigidbody>();
                if (body != null)
                    return body.centerOfMass;
                throw new NoRigidBodyException("Block " + BlockName + " has no rigid body.");
            }
        }

        /// <summary>
        ///     Blocks rotation as euler angles.
        /// </summary>
        public virtual Vector3 Rotation => Bb.transform.eulerAngles;
        public virtual Vector3 RotationRad => Rotation * (Mathf.PI / 180f);
        public virtual Vector3 RotationDeg => Rotation;

        [Obsolete("Use Block.Rotation instead.")]
        public Vector3 EulerAngles => Rotation;

        /// <summary>
        ///     Blocks rotation as a quaternion.
        /// </summary>
        public virtual Quaternion RotationQuaternion => Bb.transform.rotation;

        /// <summary>
        ///     Blocks angular velocity in radians per second.
        ///     Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        public virtual Vector3 AngularVelocity
        {
            get
            {
                var body = Bb.GetComponent<Rigidbody>();
                if (body == null) throw new NoRigidBodyException("Block " + BlockName + " has no rigid body.");

                var angularVelocity = body.angularVelocity;
                return angularVelocity;
            }
        }
        public virtual Vector3 AngularVelocityDeg => AngularVelocity * (180f / Mathf.PI);
        public virtual Vector3 AngularVelocityRad => AngularVelocity;

        /// <summary>
        ///     Blocks inertia tensor.
        ///     Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        public virtual Vector3 InertiaTensor
        {
            get
            {
                var body = Bb.GetComponent<Rigidbody>();
                if (body == null) throw new NoRigidBodyException("Block " + BlockName + " has no rigid body.");

                return body.inertiaTensor;
            }
        }

        /// <summary>
        ///     Blocks inertia tensor rotation.
        ///     Throws NoRigidBodyException if the block has no RigidBody.
        /// </summary>
        public virtual Quaternion InertiaTensorRotationQuaternion
        {
            get
            {
                var body = Bb.GetComponent<Rigidbody>();
                if (body == null) throw new NoRigidBodyException("Block " + BlockName + " has no rigid body.");

                return body.inertiaTensorRotation;
            }
        }
        public virtual Vector3 InertiaTensorRotation => InertiaTensorRotationQuaternion.eulerAngles;
        public virtual Vector3 InertiaTensorRotationDeg => InertiaTensorRotation;
        public virtual Vector3 InertiaTensorRotationRad => InertiaTensorRotation * (Mathf.PI / 180f);

        /// <summary>
        ///     Unsubscribes block handler from Update events.
        /// </summary>
        public virtual void Dispose()
        {
            OnUpdate -= Update;
            OnLateUpdate -= LateUpdate;
            OnFixedUpdate -= FixedUpdate;
        }

        /// <summary>
        ///     Is called at every Update.
        /// </summary>
        protected virtual void Update()
        {
        }

        /// <summary>
        ///     Is called at every LateUpdate.
        /// </summary>
        protected virtual void LateUpdate()
        {
        }

        /// <summary>
        ///     Is called at every FixedUpdate.
        /// </summary>
        protected virtual void FixedUpdate()
        {
        }

        /// <summary>
        ///     Invokes the block's action.
        ///     Throws ActionNotFoundException if the block does not poses such action.
        /// </summary>
        /// <param name="actionName">Display name of the action.</param>
        public virtual void Action(string actionName)
        {
            actionName = actionName.ToUpper();
            throw new ActionNotFoundException("Block " + BlockName + " has no " + actionName + " action.");
        }

        /// <summary>
        ///     Returns a list of all available toggles.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetToggles()
        {
            return Bb.Toggles.Select(m => m.DisplayName.ToUpper()).ToList();
        }

        /// <summary>
        ///     Returns a list of all available sliders.
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetSliders()
        {
            return Bb.Sliders.Select(m => m.DisplayName.ToUpper()).ToList();
        }

        /// <summary>
        ///     Sets the toggle mode of the block, specified by the toggle display name.
        /// </summary>
        /// <param name="toggleName">Toggle property to be set.</param>
        /// <param name="value">Boolean value to be set.</param>
        public virtual void SetToggleMode(string toggleName, bool value)
        {
            foreach (var m in Bb.Toggles)
                if (m.DisplayName.ToUpper() == toggleName.ToUpper())
                {
                    m.IsActive = value;
                    return;
                }
            throw new PropertyNotFoundException("Toggle " + toggleName + " not found.");
        }

        /// <summary>
        ///     Sets the slider value of the block, specified by the slider display name.
        /// </summary>
        /// <param name="sliderName">Slider value to be set.</param>
        /// <param name="value">Float value to be set.</param>
        public virtual void SetSliderValue(string sliderName, float value)
        {
            if (float.IsNaN(value))
                throw new ArgumentException("Value is not a number (NaN).");
            foreach (var m in Bb.Sliders)
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                {
                    m.Value = value;
                    return;
                }
            throw new PropertyNotFoundException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        ///     Returns the toggle mode of the block, specified by the toggle display name.
        /// </summary>
        /// <param name="toggleName">Toggle property to be returned.</param>
        /// <returns>Boolean value.</returns>
        public virtual bool GetToggleMode(string toggleName)
        {
            foreach (var m in Bb.Toggles)
                if (m.DisplayName.ToUpper() == toggleName.ToUpper())
                    return m.IsActive;
            throw new PropertyNotFoundException("Toggle " + toggleName + " not found.");
        }

        /// <summary>
        ///     Returns the slider value of the block, specified by the slider display name.
        /// </summary>
        /// <param name="sliderName">Toggle property to be returned.</param>
        /// <returns>Float value.</returns>
        public virtual float GetSliderValue(string sliderName)
        {
            foreach (var m in Bb.Sliders)
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                    return m.Value;
            throw new PropertyNotFoundException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        ///     Returns the key mapper's minimum slider value, specified by the slider display name.
        /// </summary>
        /// <param name="sliderName">Minimum slider value to be returned.</param>
        /// <returns>Float value.</returns>
        public virtual float GetSliderMin(string sliderName)
        {
            foreach (var m in Bb.Sliders)
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                    return m.Min;
            throw new PropertyNotFoundException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        ///     Returns the key mapper's maximum slider value, specified by the slider display name.
        /// </summary>
        /// <param name="sliderName">Maximum slider value to be returned.</param>
        /// <returns>Float value.</returns>
        public virtual float GetSliderMax(string sliderName)
        {
            foreach (var m in Bb.Sliders)
                if (m.DisplayName.ToUpper() == sliderName.ToUpper())
                    return m.Max;
            throw new PropertyNotFoundException("Slider " + sliderName + " not found.");
        }

        /// <summary>
        ///     Adds key to the specified key bind.
        /// </summary>
        /// <param name="keyName">Key bind to add the key to.</param>
        /// <param name="key">Key value to be added.</param>
        public virtual void AddKey(string keyName, KeyCode key)
        {
            foreach (var m in Bb.Keys)
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    for (var i = 0; i < m.KeyCode.Count; i++)
                        if (m.KeyCode[i] == KeyCode.None)
                        {
                            m.AddOrReplaceKey(i, key);
                            return;
                        }
                    m.AddKey(key);
                    return;
                }
            throw new PropertyNotFoundException("Key " + keyName + " not found.");
        }

        /// <summary>
        ///     Replaces the first key bound to the specified key bind.
        /// </summary>
        /// <param name="keyName">Key bind to be replaced.</param>
        /// <param name="key">Key value to be replaced with.</param>
        public virtual void ReplaceKey(string keyName, KeyCode key)
        {
            foreach (var m in Bb.Keys)
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                    m.AddOrReplaceKey(0, key);
            throw new PropertyNotFoundException("Key " + keyName + " not found.");
        }

        /// <summary>
        ///     Returns the first key value bound of the specified key bind.
        /// </summary>
        /// <param name="keyName">Key bind to be returned.</param>
        /// <returns>Integer value.</returns>
        public virtual KeyCode GetKey(string keyName)
        {
            foreach (var m in Bb.Keys)
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                    return m.KeyCode[0];
            throw new PropertyNotFoundException("Key " + keyName + " not found.");
        }

        /// <summary>
        ///     Clears all keys of the specified key bind.
        /// </summary>
        /// <param name="keyName"></param>
        public virtual void ClearKeys(string keyName)
        {
            foreach (var m in Bb.Keys)
                if (m.DisplayName.ToUpper() == keyName.ToUpper())
                {
                    for (var i = 0; i < m.KeyCode.Count; i++)
                        m.AddOrReplaceKey(i, KeyCode.None);
                    return;
                }
            throw new PropertyNotFoundException("Key " + keyName + " not found.");
        }
    }
}