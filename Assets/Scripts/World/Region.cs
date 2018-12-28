using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace World
{
    public class Region
    {
        /// <summary>
        /// The vertical chunks inside this region
        /// </summary>
        public VChunk[,] VChunks;

        /// <summary>
        /// The position of the region in region coordinates (world coord / (CHUNK_SIZE * REGION_SIZE)n
        /// </summary>
        public Vector2Int Position;

        /// <summary>
        /// The path leading to the region folder
        /// </summary>
        public string Path;

        /// <summary>
        /// The name for this specific file
        /// Does not contain the path
        /// </summary>
        private string Filename;

        /// <summary>
        /// Creates a new Region
        /// </summary>
        /// <param name="position">The position for the region in region coordinates (world coord / (CHUNK_SIZE * REGION_SIZE))</param> 
        /// <param name="path">The path leading to the region folder. The file name will be generated</param>
        public Region(Vector2Int position, string path)
        {
            Position = position;
            VChunks = new VChunk[ChunkController.REGION_SIZE, ChunkController.REGION_SIZE];
            Path = path;
            Filename = position.x + "," + position.y + ".r";
        }
        /// <summary>
        /// Creates a default region with a position at 0,0 and no path
        /// </summary>
        public Region() : this(new Vector2Int(0, 0), "") { }

        public VChunk GetVChunk(int x, int y)
        {
            if (x < 0 || x > ChunkController.REGION_SIZE ||
                y < 0 || y > ChunkController.REGION_SIZE) throw new IndexOutOfRangeException();
            return VChunks[x, y];
        }

        public Chunk GetChunk(int x, int y, int z)
        {
            if (x < 0 || x > ChunkController.REGION_SIZE ||
                y < 0 || y > ChunkController.REGION_SIZE ||
                z < 0 || z > ChunkController.VCHUNK_HEIGHT) throw new IndexOutOfRangeException();
            return VChunks[x, y][z];
        }
    }
}
