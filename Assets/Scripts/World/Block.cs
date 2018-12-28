using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public interface IBlock
    {
        uint GetID();
        string GetShortName();
        void RegisterBlock();
        bool IsSolid();
        bool IsTransparent();
    }

    public class AirBlock : IBlock
    {
        public uint GetID() { return 0; }
        public string GetShortName() { return "BlockAir"; }
        public void RegisterBlock()
        {
            IBlock block = new AirBlock();
            BlockRegistry.Instance.AddBlock(block);
        }
        public bool IsSolid() { return false; }
        public bool IsTransparent() { return true; }
    }

    public class StoneBlock : IBlock
    {
        public uint GetID() { return 1; }
        public string GetShortName() { return "BlockStone"; }
        public void RegisterBlock()
        {
            IBlock block = new StoneBlock();
            BlockRegistry.Instance.AddBlock(block);
        }
        public bool IsSolid() { return true; }
        public bool IsTransparent() { return false; }
    }

    public class DirtBlock : IBlock
    {
        public uint GetID() { return 2; }
        public string GetShortName() { return "BlockDirt"; }
        public void RegisterBlock()
        {
            IBlock block = new StoneBlock();
            BlockRegistry.Instance.AddBlock(block);
        }
        public bool IsSolid() { return true; }
        public bool IsTransparent() { return false; }
    }
}
