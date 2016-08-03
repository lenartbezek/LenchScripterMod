using System;
using System.Collections.Generic;
using Lench.AdvancedControls.Blocks;

namespace Lench.AdvancedControls
{
    /// <summary>
    /// Block Handlers API of the scripting mod.
    /// </summary>
    public static class BlockHandlers
    {
        /// <summary>
        /// Event invoked when simulation block handlers are initialised.
        /// Use this instead of OnSimulation if you're relying on block handlers.
        /// </summary>
        public static InitialisationDelegate OnInitialisation;

        /// <summary>
        /// Returns True if block handlers are initialised.
        /// </summary>
        public static bool Initialised { get; private set; }

        // Map: Building GUID -> Sequential ID
        internal static Dictionary<Guid, string> buildingBlocks;

        // Map: BlockBehaviour -> Block handler
        internal static Dictionary<BlockBehaviour, Block> bbToBlockHandler;

        // Map: GUID -> Block handler
        internal static Dictionary<Guid, Block> guidToBlockHandler;

        // Map: ID -> Block handler
        internal static Dictionary<string, Block> idToBlockHandler;

        // Map: BlockType -> BlockHandler type
        internal static Dictionary<int, Type> Types = new Dictionary<int, Type>
        {
            {(int)BlockType.Cannon, typeof(Cannon)},
            {(int)BlockType.ShrapnelCannon, typeof(Cannon)},
            {(int)BlockType.CogMediumPowered, typeof(Cog)},
            {(int)BlockType.Wheel, typeof(Cog)},
            {(int)BlockType.LargeWheel, typeof(Cog)},
            {(int)BlockType.Drill, typeof(Cog)},
            {(int)BlockType.Decoupler, typeof(Decoupler)},
            {(int)BlockType.Flamethrower, typeof(Flamethrower)},
            {(int)BlockType.FlyingBlock, typeof(FlyingSpiral)},
            {(int)BlockType.Grabber, typeof(Grabber)},
            {(int)BlockType.Grenade, typeof(Grenade)},
            {(int)BlockType.Piston, typeof(Piston)},
            {59, typeof(Rocket) },
            {(int)BlockType.Spring, typeof(Spring)},
            {(int)BlockType.RopeWinch, typeof(Spring)},
            {(int)BlockType.SteeringHinge, typeof(Steering)},
            {(int)BlockType.SteeringBlock, typeof(Steering)},
            {(int)BlockType.WaterCannon, typeof(WaterCannon)},
            {410, typeof(Automatron)},
            {790, typeof(VectorThruster) }
        };

        /// <summary>
        /// Events invoked on updates.
        /// </summary>
        internal delegate void UpdateEventHandler();
        internal static event UpdateEventHandler OnUpdate;

        internal delegate void LateUpdateEventHandler();
        internal static event LateUpdateEventHandler OnLateUpdate;

        internal delegate void FixedUpdateEventHandler();
        internal static event FixedUpdateEventHandler OnFixedUpdate;

        /// <summary>
        /// Event invoked when simulation block handlers are initialised.
        /// </summary>
        public delegate void InitialisationDelegate();

        /// <summary>
        /// Calls Update method of all initialised Block handlers.
        /// </summary>
        public static void CallUpdate()
        {
            OnUpdate?.Invoke();
        }

        /// <summary>
        /// Calls LateUpdate method of all initialised Block handlers.
        /// </summary>
        public static void CallLateUpdate()
        {
            OnLateUpdate?.Invoke();
        }

        /// <summary>
        /// Calls FixedUpdate method of all initialised Block handlers.
        /// </summary>
        public static void CallFixedUpdate()
        {
            OnFixedUpdate?.Invoke();
        }

        /// <summary>
        /// Initializes and returns new Block handler object.
        /// Each block should only have one Block handler.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        /// <returns>LenchScripterMod.Block object.</returns>
        private static Block CreateBlock(BlockBehaviour bb)
        {
            Block block;
            if (Types.ContainsKey(bb.GetBlockID()))
                block = (Block)Activator.CreateInstance(Types[bb.GetBlockID()], new object[] { bb });
            else
                block = new Block(bb);
            bbToBlockHandler[bb] = block;
            return block;
        }

        /// <summary>
        /// Return Block handler with given Guid.
        /// </summary>
        /// <param name="blockGuid">Block's GUID.</param>
        /// <returns>Returns reference to blocks Block handler object.</returns>
        public static Block GetBlock(Guid blockGuid)
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            if (guidToBlockHandler.ContainsKey(blockGuid))
                return guidToBlockHandler[blockGuid];
            throw new BlockNotFoundException("Block " + blockGuid + " not found.");
        }

        /// <summary>
        /// Returns Block handler for a given BlockBehaviour.
        /// </summary>
        /// <param name="bb"></param>
        /// <returns></returns>
        public static Block GetBlock(BlockBehaviour bb)
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            if (bbToBlockHandler.ContainsKey(bb))
                return bbToBlockHandler[bb];
            throw new BlockNotFoundException("Given BlockBehaviour has no corresponding Block handler.");
        }

        /// <summary>
        /// Finds blockId identifier in dictionary of simulation blocks.
        /// </summary>
        /// <param name="blockId">Block's sequential identifier.</param>
        /// <returns>Returns reference to blocks Block handler object.</returns>
        public static Block GetBlock(string blockId)
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            if (idToBlockHandler.ContainsKey(blockId.ToUpper()))
                return idToBlockHandler[blockId.ToUpper()];
            throw new BlockNotFoundException("Block " + blockId + " not found.");
        }

        /// <summary>
        /// Returns sequential identifier of a block during building.
        /// </summary>
        /// <param name="block">Block object.</param>
        /// <returns></returns>
        public static string GetID(GenericBlock block)
        {
            if (buildingBlocks == null || !buildingBlocks.ContainsKey(block.Guid))
                InitializeBuildingBlockIDs();
            return buildingBlocks[block.Guid];
        }

        /// <summary>
        /// Returns sequential identifier of a block with given guid during building.
        /// </summary>
        /// <param name="guid">Guid of the block.</param>
        /// <returns></returns>
        public static string GetID(Guid guid)
        {
            if (buildingBlocks == null || !buildingBlocks.ContainsKey(guid))
                InitializeBuildingBlockIDs();
            return buildingBlocks[guid];
        }

        /// <summary>
        /// Populates dictionary with references to building blocks.
        /// Used for dumping block IDs while building.
        /// Called at first DumpBlockID after machine change.
        /// </summary>
        public static void InitializeBuildingBlockIDs()
        {
            var typeCount = new Dictionary<string, int>();
            buildingBlocks = new Dictionary<Guid, string>();
            for (int i = 0; i < Machine.Active().BuildingBlocks.Count; i++)
            {
                GenericBlock block = Machine.Active().BuildingBlocks[i].GetComponent<GenericBlock>();
                string name = Machine.Active().BuildingBlocks[i].GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                buildingBlocks[block.Guid] = name + " " + typeCount[name];
            }
        }

        /// <summary>
        /// Populates dictionary with references to  block handlers.
        /// Used for accessing blocks with GetBlock() while simulating.
        /// If mod is loaded, it gets called automatically at the start of simulation.
        /// Invokes OnInitialisation event.
        /// </summary>
        public static void InitializeBlockHandlers()
        {
            idToBlockHandler = new Dictionary<string, Block>();
            guidToBlockHandler = new Dictionary<Guid, Block>();
            bbToBlockHandler = new Dictionary<BlockBehaviour, Block>();
            var typeCount = new Dictionary<string, int>();
            for (int i = 0; i < Machine.Active().BuildingBlocks.Count; i++)
            {
                string name = Machine.Active().BuildingBlocks[i].GetComponent<MyBlockInfo>().blockName.ToUpper();
                typeCount[name] = typeCount.ContainsKey(name) ? typeCount[name] + 1 : 1;
                string id = name + " " + typeCount[name];
                Guid guid = Machine.Active().BuildingBlocks[i].Guid;
                Block b = CreateBlock(Machine.Active().Blocks[i]);
                idToBlockHandler[id] = b;
                guidToBlockHandler[guid] = b;
            }

            Initialised = true;
            OnInitialisation?.Invoke();
        }

        /// <summary>
        /// Destroys dictionary of block handler references.
        /// Called at the end of the simulation.
        /// </summary>
        public static void DestroyBlockHandlers()
        {
            idToBlockHandler = null;
            guidToBlockHandler = null;
            bbToBlockHandler = null;
            Initialised = false;
        }

        /// <summary>
        /// Retrieve all initialized Block handlers.
        /// </summary>
        public static List<Block> GetBlocks()
        {
            if (!Initialised) throw new InvalidOperationException("Block handlers are not initialised.");
            List<Block> blocks = new List<Block>();
            foreach (KeyValuePair<string, Block> entry in idToBlockHandler)
            {
                blocks.Add(entry.Value);
            }
            return blocks;
        }

        /// <summary>
        /// Add custom BlockHandler to be initialized for blocks of the specified BlockType.
        /// Must derive from LenchScripter.Blocks.Block base class.
        /// </summary>
        /// <param name="BlockType">Block type ID.</param>
        /// <param name="BlockHandler">Type of your Block handler.</param>
        public static void AddBlockHandler(int BlockType, Type BlockHandler)
        {
            if (!BlockHandler.IsSubclassOf(typeof(Block)))
            {
                throw new ArgumentException(BlockHandler.ToString() + " is not a subclass of Block.");
            }
            if (Types.ContainsKey(BlockType))
            {
                Types[BlockType] = BlockHandler;
            }
            else
            {
                Types.Add(BlockType, BlockHandler);
            }
        }
    }


    /// <summary>
    /// Exception to be thrown when a block is not found.
    /// </summary>
    public class BlockNotFoundException : Exception
    {
        /// <summary>
        /// Creates new BlockNotFoundException with given message.
        /// </summary>
        /// <param name="message"></param>
        public BlockNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception to be thrown when trying to call an action that does not exist for the current block.
    /// </summary>
    public class ActionNotFoundException : Exception
    {
        /// <summary>
        /// Creates new ActionNotFoundException with given message.
        /// </summary>
        /// <param name="message"></param>
        public ActionNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception to be thrown when referencing the block's property that does not exist for the current block.
    /// </summary>
    public class PropertyNotFoundException : Exception
    {
        /// <summary>
        /// Creates new PropertyNotFoundException with given message.
        /// </summary>
        /// <param name="message"></param>
        public PropertyNotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// Exception to be thrown when trying to access the block's rigid body if it does not have one.
    /// </summary>
    public class NoRigidBodyException : Exception
    {
        /// <summary>
        /// Creates new NoRigidBody with given message.
        /// </summary>
        /// <param name="message"></param>
        public NoRigidBodyException(string message) : base(message) { }
    }
}
