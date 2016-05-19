using System;
using System.Collections.Generic;
using LenchScripter.Blocks;
using LenchScripter.Internal;

namespace LenchScripter
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
        public static Scripter.InitialisationEventHandler OnInitialisation;

        /// <summary>
        /// Returns True if block handlers are initialised.
        /// </summary>
        public static bool Initialised
        {
            get
            {
                return Scripter.Instance.handlersInitialised;
            }
        }

        /// <summary>
        /// Retrieve the Block handler for the block with given identifier.
        /// </summary>
        /// <param name="BlockID">GUID or sequential identifier string.</param>
        /// <returns>Block object.</returns>
        public static Block GetBlock(Guid BlockID)
        {
            if (Scripter.Instance.isSimulating)
            {
                return Scripter.Instance.GetBlock(BlockID);
            }
            else
            {
                throw new InvalidOperationException("Cannot get Block handler while not simulating.");
            }
        }

        /// <summary>
        /// Retrieve the Block handler for the given BlockBehaviour script.
        /// </summary>
        /// <param name="bb">BlockBehaviour object.</param>
        /// <returns>Block handler.</returns>
        public static Block GetBlock(BlockBehaviour bb)
        {
            if (Scripter.Instance.isSimulating)
            {
                return Scripter.Instance.GetBlock(bb);
            }
            else
            {
                throw new InvalidOperationException("Cannot get Block handler while not simulating.");
            }
        }

        /// <summary>
        /// Retrieve all initialized Block handlers.
        /// </summary>
        public static List<Block> GetBlocks()
        {
            List<Block> blocks = new List<Block>();
            if (Scripter.Instance.isSimulating)
            {
                if (Scripter.Instance.idToSimulationBlock == null)
                    Scripter.Instance.InitializeSimulationBlockHandlers();
                foreach (KeyValuePair<string, Block> entry in Scripter.Instance.idToSimulationBlock)
                {
                    blocks.Add(entry.Value);
                }
                return blocks;
            }
            else
            {
                throw new InvalidOperationException("Cannot get Block handlers while not simulating.");
            }
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
            if (Scripter.Instance.HandlerTypes.ContainsKey(BlockType))
            {
                Scripter.Instance.HandlerTypes[BlockType] = BlockHandler;
            }
            else
            {
                Scripter.Instance.HandlerTypes.Add(BlockType, BlockHandler);
            }
        }
    }
}
