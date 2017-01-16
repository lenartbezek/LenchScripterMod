using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Lench.Scripter.Blocks;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleNullReferenceException

namespace Lench.Scripter
{
    /// <summary>
    ///     Block handlers API of the scripting mod.
    /// </summary>
    public partial class Block
    {
        /// <summary>
        ///     Event invoked when simulation block handlers are initialised.
        ///     Use this instead of OnSimulation if you're relying on block handlers.
        /// </summary>
        public static event Action OnInitialisation;

        internal static event Action OnUpdate;

        internal static event Action OnLateUpdate;

        internal static event Action OnFixedUpdate;

        private static bool _rebuildIDs;

        // Map: Building GUID -> Sequential ID
        internal static Dictionary<Guid, string> BuildingBlocks;

        // Map: BlockBehaviour -> Block handler
        internal static Dictionary<BlockBehaviour, Block> BbToBlockHandler;

        // Map: GUID -> Block handler
        internal static Dictionary<Guid, Block> GUIDToBlockHandler;

        // Map: ID -> Block handler
        internal static Dictionary<string, Block> IDToBlockHandler;

        // Map: BlockType -> BlockHandler type
        internal static Dictionary<int, Type> Types = new Dictionary<int, Type>
        {
            {(int) BlockType.Cannon, typeof(Cannon)},
            {(int) BlockType.ShrapnelCannon, typeof(Cannon)},
            {(int) BlockType.CogMediumPowered, typeof(Cog)},
            {(int) BlockType.Wheel, typeof(Cog)},
            {(int) BlockType.LargeWheel, typeof(Cog)},
            {(int) BlockType.Drill, typeof(Cog)},
            {(int) BlockType.Crossbow, typeof(Crossbow)},
            {(int) BlockType.Decoupler, typeof(Decoupler)},
            {(int) BlockType.Flamethrower, typeof(Flamethrower)},
            {(int) BlockType.FlyingBlock, typeof(FlyingSpiral)},
            {(int) BlockType.Grabber, typeof(Grabber)},
            {(int) BlockType.Grenade, typeof(Grenade)},
            {(int) BlockType.Piston, typeof(Piston)},
            {59, typeof(Rocket)},
            {(int) BlockType.Spring, typeof(Spring)},
            {(int) BlockType.RopeWinch, typeof(Spring)},
            {(int) BlockType.SteeringHinge, typeof(Steering)},
            {(int) BlockType.SteeringBlock, typeof(Steering)},
            {(int) BlockType.Vacuum, typeof(Vacuum)},
            {(int) BlockType.WaterCannon, typeof(WaterCannon)},
            {410, typeof(Automatron)},
            {790, typeof(VectorThruster)}
        };

        /// <summary>
        ///     Returns True if block handlers are initialised.
        /// </summary>
        public static bool Initialised { get; private set; }

        private static BlockHandlerControllerComponent _component;

        // ReSharper disable once ClassNeverInstantiated.Local
        private class BlockHandlerControllerComponent : MonoBehaviour
        {
            private void Update()
            {
                // Initialize block identifiers
                if (!StatMaster.isSimulating && _rebuildIDs)
                {
                    InitializeIDs();
                }

                OnUpdate?.Invoke();
            }

            private void LateUpdate()
            {
                OnLateUpdate?.Invoke();
            }

            private void FixedUpdate()
            {
                OnFixedUpdate?.Invoke();
            }
        }

        /// <summary>
        ///     Initializes and returns new Block handler object.
        ///     Each block should only have one Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        /// <returns>LenchScripterMod.Block object.</returns>
        private static Block Create(BlockBehaviour bb)
        {
            Block block;
            if (Types.ContainsKey(bb.GetBlockID()))
                block = (Block) Activator.CreateInstance(Types[bb.GetBlockID()], new object[] {bb});
            else
                block = new Block(bb);
            BbToBlockHandler[bb] = block;
            return block;
        }

        /// <summary>
        ///     Return Block handler with given Guid.
        /// </summary>
        /// <param name="blockGuid">Block's GUID.</param>
        /// <returns>Returns reference to blocks Block handler object.</returns>
        public static Block Get(Guid blockGuid)
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            if (GUIDToBlockHandler.ContainsKey(blockGuid))
                return GUIDToBlockHandler[blockGuid];
            throw new BlockNotFoundException("Block " + blockGuid + " not found.");
        }

        /// <summary>
        ///     Returns Block handler for a given BlockBehaviour.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static Block Get(BlockBehaviour bb)
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            if (BbToBlockHandler.ContainsKey(bb))
                return BbToBlockHandler[bb];
            throw new BlockNotFoundException("Given BlockBehaviour has no corresponding Block handler.");
        }

        /// <summary>
        ///     Finds blockId identifier in dictionary of simulation blocks.
        /// </summary>
        /// <param name="blockId">Block's sequential identifier.</param>
        /// <returns>Returns reference to blocks Block handler object.</returns>
        public static Block Get(string blockId)
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            if (IDToBlockHandler.ContainsKey(blockId.ToUpper()))
                return IDToBlockHandler[blockId.ToUpper()];
            throw new BlockNotFoundException("Block " + blockId + " not found.");
        }

        /// <summary>
        ///     Returns sequential identifier of a block during building.
        /// </summary>
        /// <param name="block">Block object.</param>
        /// <returns></returns>
        public static string GetID(GenericBlock block)
        {
            if (BuildingBlocks == null || !BuildingBlocks.ContainsKey(block.Guid))
                InitializeIDs();
            return BuildingBlocks[block.Guid];
        }

        /// <summary>
        ///     Returns sequential identifier of a block with given guid during building.
        /// </summary>
        /// <param name="guid">Guid of the block.</param>
        /// <returns></returns>
        public static string GetID(Guid guid)
        {
            if (BuildingBlocks == null || !BuildingBlocks.ContainsKey(guid))
                InitializeIDs();
            return BuildingBlocks[guid];
        }

        /// <summary>
        ///     Notifies the block handler controller of change to rebuild block IDs.
        /// </summary>
        public static void FlagForIDRebuild()
        {
            _rebuildIDs = true;
        }

        /// <summary>
        ///     Populates dictionary with references to building blocks.
        ///     Used for dumping block IDs while building.
        /// </summary>
        public static void InitializeIDs()
        {
            var typeCount = new Dictionary<string, int>();
            BuildingBlocks = new Dictionary<Guid, string>();
            foreach (var t in ReferenceMaster.BuildingBlocks)
            {
                var block = t.GetComponent<GenericBlock>();
                var name = t.GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                BuildingBlocks[block.Guid] = name + " " + typeCount[name];
            }
            _rebuildIDs = false;
        }

        /// <summary>
        ///     Populates dictionary with references to  block handlers.
        ///     Used for accessing blocks with Get() while simulating.
        ///     If mod is loaded, it gets called automatically at the start of simulation.
        ///     Invokes OnInitialisation event when done.
        /// </summary>
        public static void Initialize()
        {
            IDToBlockHandler = new Dictionary<string, Block>();
            GUIDToBlockHandler = new Dictionary<Guid, Block>();
            BbToBlockHandler = new Dictionary<BlockBehaviour, Block>();
            var typeCount = new Dictionary<string, int>();
            for (var i = 0; i < ReferenceMaster.BuildingBlocks.Count; i++)
            {
                var name = ReferenceMaster.BuildingBlocks[i].GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                var id = name + " " + typeCount[name];
                var guid = ReferenceMaster.BuildingBlocks[i].Guid;
                var b = Create(ReferenceMaster.SimulationBlocks[i]);
                IDToBlockHandler[id] = b;
                GUIDToBlockHandler[guid] = b;
            }

            Initialised = true;
            OnInitialisation?.Invoke();
        }

        private static IEnumerator WaitAndInitialize()
        {
            while (!StatMaster.isSimulating || ReferenceMaster.SimulationBlocks.Count < ReferenceMaster.BuildingBlocks.Count)
                yield return null;
            Initialize();
        }

        /// <summary>
        ///     Destroys dictionary of block handler references.
        ///     Called at the end of the simulation.
        /// </summary>
        public static void Destroy()
        {
            if (BbToBlockHandler != null)
                foreach (var entry in BbToBlockHandler)
                    entry.Value.Dispose();
            IDToBlockHandler = null;
            GUIDToBlockHandler = null;
            BbToBlockHandler = null;
            Initialised = false;

            Object.Destroy(_component);
        }

        /// <summary>
        ///     Retrieve all initialized Block handlers.
        /// </summary>
        public static List<Block> GetAll()
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            return IDToBlockHandler.Select(entry => entry.Value).ToList();
        }

        /// <summary>
        ///     Add custom BlockHandler to be initialized for blocks of the specified BlockType.
        ///     Must derive from LenchScripter.Blocks.Block base class.
        /// </summary>
        /// <param name="blockType">Block type ID.</param>
        /// <param name="blockHandler">Type of your Block handler.</param>
        public static void MapTypeToID(int blockType, Type blockHandler)
        {
            if (!blockHandler.IsSubclassOf(typeof(global::Block)))
                throw new ArgumentException(blockHandler + " is not a subclass of Block.");
            if (Types.ContainsKey(blockType))
                Types[blockType] = blockHandler;
            else
                Types.Add(blockType, blockHandler);
        }

        internal static void OnSimulationToggle(bool active)
        {
            if (active)
            {
                _component = Mod.Controller.AddComponent<BlockHandlerControllerComponent>();
                _component.StartCoroutine(WaitAndInitialize());
            }
            else
            {
                Destroy();
            } 
        }
    }


    /// <summary>
    ///     Exception to be thrown when a block is not found.
    /// </summary>
    public class BlockNotFoundException : Exception
    {
        /// <summary>
        ///     Creates new BlockNotFoundException with given message.
        /// </summary>
        /// <param name="message"></param>
        public BlockNotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Exception to be thrown when trying to call an action that does not exist for the current block.
    /// </summary>
    public class ActionNotFoundException : Exception
    {
        /// <summary>
        ///     Creates new ActionNotFoundException with given message.
        /// </summary>
        /// <param name="message"></param>
        public ActionNotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Exception to be thrown when referencing the block's property that does not exist for the current block.
    /// </summary>
    public class PropertyNotFoundException : Exception
    {
        /// <summary>
        ///     Creates new PropertyNotFoundException with given message.
        /// </summary>
        /// <param name="message"></param>
        public PropertyNotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    ///     Exception to be thrown when trying to access the block's rigid body if it does not have one.
    /// </summary>
    public class NoRigidBodyException : Exception
    {
        /// <summary>
        ///     Creates new NoRigidBody with given message.
        /// </summary>
        /// <param name="message"></param>
        public NoRigidBodyException(string message) : base(message)
        {
        }
    }
}