using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace World
{
    public class VChunk
    {
        /// <summary>
        /// The stored array of chunks at these coordinates
        /// </summary>
        private Chunk[] chunks;
        /// <summary>
        /// The position of the VChunk in chunk coordinates (World / CHUNK_SIZE)
        /// </summary>
        private Vector2Int Position;

        /// <summary>
        /// The prefab used to instantitate the chunks
        /// </summary>
        public GameObject Prefab;

        public Chunk this[int key]
        {
            get { return chunks[key]; }
            set { chunks[key] = value; }
        }

        public uint NumberPlayersLoading = 0;

        public VChunk() : this(new Vector2Int(0, 0)) { }


        public VChunk(Vector2Int position)
        {
            Position = position;
            chunks = new Chunk[ChunkController.VCHUNK_HEIGHT];
            for (int i = 0; i < chunks.Length; i++)
            {
                chunks[i] = new Chunk(new Vector3Int(position.x, position.y, i));
            }
        }

        public void Setup(GameObject Prefab)
        {
            foreach (Chunk chunk in chunks)
            {
                chunk.SetupChunk(Prefab);
            }
        }
        public void TearDown()
        {
            foreach (Chunk chunk in chunks)
            {
                chunk.TearDownChunk();
            }
        }
        public void Load()
        {
            NumberPlayersLoading++;
            throw new NotImplementedException();
        }
        public void Unload()
        {
            foreach (Chunk chunk in chunks)
            {
                if (NumberPlayersLoading == 1)
                {
                    throw new NotImplementedException();
                } else
                {
                    NumberPlayersLoading--;
                }
                
                //chunk.UnloadChunk();
            }
        }
    }

    public class BlockArray
    {
        private IBlock[,,] blocks;
        public IBlock this[int x, int y, int z]
        {
            get { return blocks[x, y, z]; }
            set { blocks[x, y, z] = value; }
        }

        public BlockArray(int x, int y, int z)
        {
            blocks = new IBlock[x, y, z];
        }
    }

    public class Chunk
    {
        /// <summary>
        /// The stored array of blocks for this chunk
        /// </summary>
        private IBlock[,,] Blocks;
        /// <summary>
        /// The position of this Chunk in chunk coordinates (World / CHUNK_SIZE)
        /// </summary>
        private Vector3Int Position;

        /// <summary>
        /// Flag variable to tell whether the chunk has been loaded yet
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Flag variable to tell whether the chunk's mesh has been set up yet
        /// </summary>
        public bool IsSetup { get; private set; }

        /// <summary>
        /// Creates a chunk at 0,0,0
        /// </summary>
        public Chunk() : this(new Vector3Int(0, 0, 0)) { }

        /// <summary>
        /// The game object used by unity to render the mesh
        /// </summary>
        public GameObject go;

        /// <summary>
        /// The mesh used by unity to draw the textures
        /// </summary>
        public Mesh TextureMesh;

        /// <summary>
        /// The mesh used by unity for collision detection
        /// </summary>
        public Mesh CollisionMesh;

        int ChunkSize;

        /// <summary>
        /// Creates a chunk at the given position
        /// </summary>
        /// <param name="position"></param> The position of the chunk in Chunk coordinates (World / CHUNK_SIZE)
        public Chunk(Vector3Int position)
        {
            ChunkSize = ChunkController.CHUNK_SIZE;
            Position = position;
            Blocks = new IBlock[ChunkSize, ChunkSize, ChunkSize];
            IsLoaded = false;
            IsSetup = false;
        }

        #region CHANGE_BLOCK
        public void ChangeBlock(int bx, int by, int bz, uint blockID)
        {
            Blocks[bx, by, bz] = BlockRegistry.Instance.GetBlock(blockID);
        }
        #endregion

        #region SETUP_AND_TEARDOWN
        public void SetupChunk(GameObject Prefab)
        {
            GameObject.Destroy(go);
            go = GameObject.Instantiate(Prefab);
            go.name = "Mesh: " + Position.x + ", " + Position.y + ", " + Position.z;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshCollider>();
            go.AddComponent<MeshRenderer>();
            GenerateMesh(TextureMesh);
            GenerateMesh(CollisionMesh);
        }

        public void TearDownChunk()
        {
            GameObject.Destroy(go);
            Mesh.Destroy(TextureMesh);
            Mesh.Destroy(CollisionMesh);
        }
        #endregion

        #region MESH_GENERATION
        private int[][] triangles;
        private Vector3[] Verts;
        private Vector3[] Normals;
        private Vector2[] UVs;

        public void GenerateMesh(Mesh mesh)
        {
            ClearArrays();
            CreateArrays(CountFaces(mesh));
            int offset = 0;
            for (int x = 0; x < ChunkSize; x++)
            {
                for (int y = 0; y < ChunkSize; y++)
                {
                    for (int z = 0; z < ChunkSize; z++)
                    {
                        if (Blocks[x, y, z].IsTransparent() && mesh == TextureMesh ||
                            !Blocks[x, y, z].IsSolid() && mesh == CollisionMesh) continue;
                        // Top of the block
                        if (y == 0 || (mesh == TextureMesh && Blocks[x, y - 1, z].IsTransparent()) || (mesh == CollisionMesh && !Blocks[x, y - 1, z].IsSolid()))
                        {
                            Verts[offset * 4] = new Vector3(x, y, z + 1);
                            Verts[offset * 4 + 1] = new Vector3(x + 1, y, z + 1);
                            Verts[offset * 4 + 2] = new Vector3(x, y, z);
                            Verts[offset * 4 + 3] = new Vector3(x + 1, y, z);

                            WindRight(offset, Blocks[x, y, z].GetID());
                            SetNormals(offset);
                            SetUVs(offset);
                            offset++;
                        }
                        if (y == ChunkSize - 1 || (mesh == TextureMesh && Blocks[x, y + 1, z].IsTransparent()) || (mesh == CollisionMesh && !Blocks[x, y + 1, z].IsSolid()))
                        {
                            Verts[offset * 4] = new Vector3(x + 1, y, z + 1);
                            Verts[offset * 4 + 1] = new Vector3(x + 1, y + 1, z + 1);
                            Verts[offset * 4 + 2] = new Vector3(x + 1, y, z);
                            Verts[offset * 4 + 3] = new Vector3(x + 1, y + 1, z);

                            WindRight(offset, Blocks[x, y, z].GetID());
                            SetNormals(offset);
                            SetUVs(offset);
                            offset++;
                        }
                        if (x == 0 || (mesh == TextureMesh && Blocks[x - 1, y, z].IsTransparent()) || (mesh == CollisionMesh && !Blocks[x - 1, y, z].IsSolid()))
                        {
                            Verts[offset * 4] = new Vector3(x, y, z + 1);
                            Verts[offset * 4 + 1] = new Vector3(x, y + 1, z + 1);
                            Verts[offset * 4 + 2] = new Vector3(x, y, z);
                            Verts[offset * 4 + 3] = new Vector3(x, y + 1, z);

                            WindLeft(offset, Blocks[x, y, z].GetID());
                            SetNormals(offset);
                            SetUVs(offset);
                            offset++;
                        }
                        if (x == ChunkSize - 1 || (mesh == TextureMesh && Blocks[x + 1, y, z].IsTransparent()) || (mesh == CollisionMesh && !Blocks[x + 1, y, z].IsSolid()))
                        {
                            Verts[offset * 4] = new Vector3(x + 1, y, z + 1);
                            Verts[offset * 4 + 1] = new Vector3(x + 1, y + 1, z + 1);
                            Verts[offset * 4 + 2] = new Vector3(x + 1, y, z);
                            Verts[offset * 4 + 3] = new Vector3(x + 1, y + 1, z);

                            WindRight(offset, Blocks[x, y, z].GetID());
                            SetNormals(offset);
                            SetUVs(offset);
                            offset++;
                        }
                        if (z == 0 || (mesh == TextureMesh && Blocks[x, y, z - 1].IsTransparent()) || (mesh == CollisionMesh && !Blocks[x, y, z - 1].IsSolid()))
                        {
                            Verts[offset * 4] = new Vector3(x + 1, y, z);
                            Verts[offset * 4 + 1] = new Vector3(x + 1, y + 1, z);
                            Verts[offset * 4 + 2] = new Vector3(x, y, z);
                            Verts[offset * 4 + 3] = new Vector3(x, y + 1, z);

                            WindRight(offset, Blocks[x, y, z].GetID());
                            SetNormals(offset);
                            SetUVs(offset);
                            offset++;
                        }
                        if (z == ChunkSize - 1 || (mesh == TextureMesh && Blocks[x, y, z + 1].IsTransparent()) || (mesh == CollisionMesh && !Blocks[x, y, z - 1].IsSolid()))
                        {
                            Verts[offset * 4] = new Vector3(x + 1, y, z + 1);
                            Verts[offset * 4 + 1] = new Vector3(x + 1, y + 1, z + 1);
                            Verts[offset * 4 + 2] = new Vector3(x, y, z + 1);
                            Verts[offset * 4 + 3] = new Vector3(x, y + 1, z + 1);

                            WindLeft(offset, Blocks[x, y, z].GetID());
                            SetNormals(offset);
                            SetUVs(offset);
                            offset++;
                        }
                    }
                }
            }

            mesh = new Mesh();
            mesh.Clear();
            mesh.subMeshCount = triangles.Length;
            for(int i = 0; i < triangles.Length; i++)
            {
                mesh.SetTriangles(triangles[i], i);
            }
            mesh.normals = Normals;
            mesh.uv = UVs;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            if (mesh == TextureMesh)
            {
                go.GetComponent<MeshFilter>().mesh = mesh;
            } else if (mesh == CollisionMesh)
            {
                MeshCollider collider = go.GetComponent<MeshCollider>();
                collider.sharedMesh = null;
                collider.sharedMesh = mesh;
            }
            go.transform.localPosition = new Vector3(Position.x * ChunkSize, Position.y * ChunkSize, Position.z * ChunkSize);
        }

        private int CountFaces(Mesh mesh)
        {
            int faces = 0;
            for(int x = 0; x < ChunkSize; x++)
            {
                for(int y = 0; y < ChunkSize; y++)
                {
                    for (int z = 0; z < ChunkSize; z++)
                    {
                        if (mesh == TextureMesh)
                        {
                            if (!Blocks[x, y, z].IsTransparent())
                            {
                                if (x == 0 || Blocks[x - 1, y, z].IsTransparent()) faces++;
                                if (x == ChunkSize - 1 || Blocks[x + 1, y, z].IsTransparent()) faces++;
                                if (y == 0 || Blocks[x, y - 1, z].IsTransparent()) faces++;
                                if (y == ChunkSize - 1 || Blocks[x, y + 1, z].IsTransparent()) faces++;
                                if (z == 0 || Blocks[x, y, z - 1].IsTransparent()) faces++;
                                if (z < ChunkSize - 1 || Blocks[x, y, z + 1].IsTransparent()) faces++;
                            }
                        } else if (mesh == CollisionMesh && Blocks[x, y, z].IsSolid())
                        {
                            if (x == 0 || !Blocks[x - 1, y, z].IsSolid()) faces++;
                            if (x == ChunkSize - 1 || !Blocks[x + 1, y, z].IsSolid()) faces++;
                            if (y == 0 || !Blocks[x, y - 1, z].IsSolid()) faces++;
                            if (y == ChunkSize - 1 || !Blocks[x, y + 1, z].IsSolid()) faces++;
                            if (z == 0 || !Blocks[x, y, z - 1].IsSolid()) faces++;
                            if (z == ChunkSize - 1 || !Blocks[x, y, z + 1].IsSolid()) faces++;
                        }
                    }
                }
            }
            return faces;
        }

        private void ClearArrays()
        {
            if (Verts != null)
                System.Array.Clear(Verts, 0, Verts.Length);
            if (Normals != null)
                Array.Clear(Normals, 0, Normals.Length);
            if (UVs != null)
                Array.Clear(UVs, 0, UVs.Length);
            if(triangles != null)
            {
                foreach(int[] i in triangles)
                {
                    if (i != null)
                        Array.Clear(i, 0, i.Length);
                }
                Array.Clear(triangles, 0, triangles.Length);
            }
        }

        private void CreateArrays(int count)
        {
            Verts = new Vector3[count * 4];
            Normals = new Vector3[count * 4];
            UVs = new Vector2[count * 4];
            triangles = new int[1][];
            for(int i = 0; i < triangles.Length; i++)
            {
                triangles[i] = new int[count * 6];
            }
        }

        private void WindRight(int offset, uint textureID)
        {
            //int textureKey = texController.GetTextureKey(textureID);
            int textureKey = (int)textureID;
            if (textureKey < 0)
                RightTriangles(triangles[0], offset);
            else
                LeftTriangles(triangles[textureKey], offset);
        }

        private void RightTriangles(int[] tris, int offset)
        {
            tris[offset * 6] = 0 + offset * 4;
            tris[offset * 6 + 1] = 2 + offset * 4;
            tris[offset * 6 + 2] = 3 + offset * 4;
            tris[offset * 6 + 3] = 3 + offset * 4;
            tris[offset * 6 + 4] = 1 + offset * 4;
            tris[offset * 6 + 5] = 0 + offset * 4;
        }

        private void WindLeft(int offset, uint textureID)
        {
            //int textureKey = texController.GetTextureKey(textureID);
            int textureKey = (int)textureID;
            if (textureKey < 0)
                LeftTriangles(triangles[0], offset);
            else
                LeftTriangles(triangles[textureKey], offset);
        }

        private void LeftTriangles(int[] tris, int offset)
        {
            tris[offset * 6] = 0 + offset * 4;
            tris[offset * 6 + 1] = 1 + offset * 4;
            tris[offset * 6 + 2] = 2 + offset * 4;
            tris[offset * 6 + 3] = 2 + offset * 4;
            tris[offset * 6 + 4] = 1 + offset * 4;
            tris[offset * 6 + 5] = 3 + offset * 4;
        }

        private void SetNormals(int offset)
        {
            Normals[offset * 4] = -Vector3.forward;
            Normals[offset * 4 + 1] = -Vector3.forward;
            Normals[offset * 4 + 2] = -Vector3.forward;
            Normals[offset * 4 + 3] = -Vector3.forward;
        }

        private void SetUVs(int offset)
        {
            UVs[offset * 4] = new Vector2(0.0f, 1.0f);
            UVs[offset * 4 + 1] = new Vector2(1.0f, 1.0f);
            UVs[offset * 4 + 2] = new Vector2(0.0f, 0.0f);
            UVs[offset * 4 + 3] = new Vector2(1.0f, 0.0f);
        }
        #endregion
    }
}

