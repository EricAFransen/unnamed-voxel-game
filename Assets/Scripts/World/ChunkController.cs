using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace World
{

    public abstract class ChunkController
    // Each player will have an associated chunk controller
    // There will only be one for the client controlling rendering
    // There will be many for the server to keep track of multiple players
    {
        // Some constants
        // Demensions of a chunk
        public const int CHUNK_SIZE = 16;

        // How many chunks are stacked for a VChunk
        public const int VCHUNK_HEIGHT = 16;

        // How big a region is
        public const int REGION_SIZE = 16;

        // The radius to load chunks
        protected int LoadRadius = 5;

        // The radius to render chunks
        protected int RenderRadius = 3;

        // The list of chunks to load
        public Queue<Vector2Int> LoadList;
        public Queue<Vector2Int> SetupList;
        public Queue<Vector2Int> TeardownList;
        public Queue<Vector2Int> UnloadList;
        public Queue<Vector2Int> SaveList;

        public Dictionary<Vector2Int, VChunk> Chunks;

        // The time between autosaves
        protected int AutoSaveInterval;

        public ChunkController()
        {
            LoadList = new Queue<Vector2Int>();
            SetupList = new Queue<Vector2Int>();
            TeardownList = new Queue<Vector2Int>();
            UnloadList = new Queue<Vector2Int>();
            SaveList = new Queue<Vector2Int>();

            Chunks = new Dictionary<Vector2Int, VChunk>();
        }

        #region CONVERSION_FUNCTIONS
        public Vector3Int ConvertWorldToChunk(Vector3 World)
        {
            int x, y, z;
            x = (int)(World.x / CHUNK_SIZE);
            y = (int)(World.y / CHUNK_SIZE);
            z = (int)(World.z / CHUNK_SIZE);
            return new Vector3Int(x, y, z);
        }
        public Vector3Int ConvertChunkToRegion(Vector3Int Chunk)
        {
            int x, y, z;
            x = Chunk.x / REGION_SIZE;
            y = Chunk.y / REGION_SIZE;
            z = Chunk.z / REGION_SIZE;
            return new Vector3Int(x, y, z);
        }
        public Vector3Int ConvertWorldToRegion(Vector3 World)
        {
            return ConvertChunkToRegion(ConvertWorldToChunk(World));
        }
        public Vector3Int ConvertRegionToChunk(Vector3Int Region)
        {
            return Region * REGION_SIZE;
        }
        public Vector3 ConvertChunkToWorld(Vector3Int Chunk)
        {
            return Chunk * CHUNK_SIZE;
        }
        public Vector3 ConvertRegionToWorld(Vector3Int Region)
        {
            return ConvertChunkToWorld(ConvertRegionToChunk(Region));
        }
        #endregion

        public abstract void Update(List<KeyValuePair<int, Vector2Int>> PlayerLocations);
        public abstract void UpdatePlayerLocations(List<KeyValuePair<int, Vector2Int>> PlayerLocations);
        public abstract void LoadChunks();
        public abstract void SetupChunks();
        public abstract void TeardownChunks();
        public abstract void UnloadChunks();
    }

    public class ClientChunkController : ChunkController
    {
        // The chunks that the player has laoded
        private VChunk[,] Chunks;

        // The unique integer for the player
        private int PlayerID;

        // The last location of the player in Chunk Coordinates
        private Vector2Int LastPlayerLocation;

        
        public ClientChunkController()
        {
            Chunks = new VChunk[LoadRadius, LoadRadius];
            LastPlayerLocation = new Vector2Int(0, 0);
        }
        public override void Update(List<KeyValuePair<int, Vector2Int>> PlayerLocations)
        {
            throw new NotImplementedException();
        }

        public override void UpdatePlayerLocations(List<KeyValuePair<int, Vector2Int>> PlayerLocations)
        {
            throw new NotImplementedException();
        }

        public override void LoadChunks()
        {
            throw new NotImplementedException();
        }

        public override void SetupChunks()
        {
            throw new NotImplementedException();
        }

        public override void TeardownChunks()
        {
            throw new NotImplementedException();
        }

        public override void UnloadChunks()
        {
            throw new NotImplementedException();
        }
    }

    public class DuplicatePlayerIDException : Exception
    {

    }

    public class ServerChunkController : ChunkController
    {
        private Dictionary<int, Vector2Int> PlayerLocations;


        public ServerChunkController()
        {
            PlayerLocations = new Dictionary<int, Vector2Int>();
        }
        public override void Update(List<KeyValuePair<int, Vector2Int>> PlayerLocations)
        {
            UpdatePlayerLocations(PlayerLocations);

        }

        public override void LoadChunks()
        {
            while(LoadList.Count != 0)
            {
                Vector2Int ChunkPos = LoadList.Dequeue();
                if(Chunks.ContainsKey(ChunkPos))
                {
                    Chunks[ChunkPos].Load();
                } else
                {
                    Vector3Int RegionPos = ConvertChunkToRegion(new Vector3Int(ChunkPos.x, ChunkPos.y, 0));
                    throw new NotImplementedException();
                }
            }
        }

        public override void SetupChunks()
        {
            throw new NotImplementedException();
        }

        public override void TeardownChunks()
        {
            throw new NotImplementedException();
        }

        public override void UnloadChunks()
        {
            throw new NotImplementedException();
        }

        public override void UpdatePlayerLocations(List<KeyValuePair<int, Vector2Int>> NewPlayerLocations)
        {
            foreach (KeyValuePair<int, Vector2Int> NewPair in NewPlayerLocations)
            {
                Vector2Int NewPlayerLocation = NewPair.Value;
                Vector2Int OldPlayerLocation = PlayerLocations[NewPair.Key];
                // Add the old chunks to the UnloadList, Add the new chunks to the LoadList
                for(int x = -LoadRadius; x < LoadRadius; x++)
                {
                    for(int y = -LoadRadius; y < LoadRadius; y++)
                    {
                        UnloadList.Enqueue(new Vector2Int(x, y) + OldPlayerLocation);
                        LoadList.Enqueue(new Vector2Int(x, y) + NewPlayerLocation);
                    }
                }
            }
        }

        public void AddPlayer(int PlayerID, Vector2Int PlayerLocation)
        {
            if (PlayerLocations.ContainsKey(PlayerID))
            {
                // We got a duplicate player ID here, we'll throw an exception
                throw new DuplicatePlayerIDException();
            }
            PlayerLocations.Add(PlayerID, PlayerLocation);
            for(int x = -LoadRadius; x < LoadRadius; x++)
            {
                for(int y = -LoadRadius; y < LoadRadius; y++)
                {
                    LoadList.Enqueue(PlayerLocation + new Vector2Int(x, y));
                }
            }
        }
        public void RemovePlayer(int PlayerID)
        {
            if (PlayerLocations.ContainsKey(PlayerID))
            {
                Vector2Int PlayerLocation = PlayerLocations[PlayerID];
                PlayerLocations.Remove(PlayerID);
                for(int x = -LoadRadius; x < LoadRadius; x++)
                {
                    for(int y = -LoadRadius; y < LoadRadius; y++)
                    {
                        UnloadList.Enqueue(PlayerLocation + new Vector2Int(x, y));
                    }
                }
            }
        }
    }
}