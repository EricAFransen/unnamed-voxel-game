using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class DuplicateBlockIDException : Exception
{

}

namespace World
{
    public class BlockRegistry
    {
        // Specifies that we're using a lazy implementation for a singleton pattern
        // Just means that we won't actually construct the class until we need it
        // And that there is only ever one instance.
        private static readonly Lazy<BlockRegistry> lazy =
            new Lazy<BlockRegistry>(() => new BlockRegistry());

        // Static method used to access the single instance of this class
        public static BlockRegistry Instance { get { return lazy.Value; } }

        // A dictionary that maps blocks to integer IDs
        private Dictionary<uint, IBlock> Blocks;

        private BlockRegistry()
        {
            Blocks = new Dictionary<uint, IBlock>();
        }

        public void AddBlock(IBlock block)
        {
            if (Blocks.ContainsKey(block.GetID()))
            {
                throw new DuplicateBlockIDException();
            }

            Blocks.Add(block.GetID(), block);
        }

        public IBlock GetBlock(uint ID)
        {
            return Blocks[ID];
        }
    }
}
