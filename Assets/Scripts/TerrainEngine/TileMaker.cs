using UnityEngine;

namespace TerrainEngine{

    public class TileMaker : MonoBehaviour {
        public int tileSize = 64; //width and height of tile (in number of vertices)
        public int numTiles = 5; //number of tiles in X and Y dimensions e.g., 5x5
        public bool rescale;

        private int[] triangles;
        private Vector3[] vertices;
        private Vector2[] newUV;
        private Vector3[] normals;

        GameObject[,] tiles;
        public Material material;
        
        void MakeVerticesTriangles(float scale) {
            triangles = new int[2 * (tileSize - 1) * (tileSize - 1) * 3 * 2]; //last *2 is added for the wall
            vertices = new Vector3[tileSize * tileSize + tileSize * tileSize];
            normals = new Vector3[tileSize * tileSize + tileSize * tileSize];
            for (var j = 0; j < tileSize; j++) {
                for (var i = 0; i < tileSize; i++) {
                    vertices[j * tileSize + i] = new Vector3((i / (tileSize - 1.0f) - 0.5f) * scale, 0,
                        (j / (tileSize - 1.0f) - 0.5f) * scale);
                    normals[j * tileSize + i] = Vector3.up;
                }
            }

            for (var j = 0; j < tileSize - 1; j++) {
                for (var i = 0; i < tileSize - 1; i++) {
                    var trisIndex = j * (tileSize - 1) + i;
                    var index = j * (tileSize) + i;
                    triangles[trisIndex * 2 * 3 + 0] = index;
                    triangles[trisIndex * 2 * 3 + 1] = index + 1;
                    triangles[trisIndex * 2 * 3 + 2] = index + tileSize;
                    triangles[trisIndex * 2 * 3 + 3] = index + 1;
                    triangles[trisIndex * 2 * 3 + 4] = index + tileSize + 1;
                    triangles[trisIndex * 2 * 3 + 5] = index + tileSize;
                }
            }

            for (var k = 0; k < tileSize; k++) {
                for (var i = 0; i < tileSize; i++) {
                    var centerJ = tileSize / 2;
                    var jLoc = (centerJ / (tileSize - 1.0f) - 0.5f) * scale;
                    vertices[tileSize * tileSize + k * tileSize + i] = new Vector3((i / (tileSize - 1.0f) - 0.5f) * scale,
                        (k / (tileSize - 1.0f) - 0.5f) * 5 * scale, jLoc);
                    normals[tileSize * tileSize + k * tileSize + i] = Vector3.up;
                }
            }

            var triangleOffset = 2 * (tileSize - 1) * (tileSize - 1) * 3;
            for (var j = 0; j < tileSize - 1; j++) {
                for (var i = 0; i < tileSize - 1; i++) {
                    var trisIndex = j * (tileSize - 1) + i + (tileSize - 1) * (tileSize - 1);
                    var index = j * (tileSize) + i + tileSize * tileSize;
                    triangles[trisIndex * 2 * 3 + 0] = index;
                    triangles[trisIndex * 2 * 3 + 1] = index + 1;
                    triangles[trisIndex * 2 * 3 + 2] = index + tileSize;
                    triangles[trisIndex * 2 * 3 + 3] = index + 1;
                    triangles[trisIndex * 2 * 3 + 4] = index + tileSize + 1;
                    triangles[trisIndex * 2 * 3 + 5] = index + tileSize;
                }
            }
        }

        /// <summary>
        /// create a new tile game object
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        GameObject MakeTile(float x, float y, float width, float height, string label = "Tile") {
            var newMesh = new Mesh { name = "Terrain Mesh" };
            newMesh.SetVertices(vertices);
            newMesh.triangles = triangles;
            //newMesh.RecalculateNormals();
            newMesh.normals = normals;
            if (newUV == null) {
                newUV = new Vector2[tileSize * tileSize + tileSize * tileSize];
            }

            float newU, newV;
            var index = 0;
            for (var j = 0; j < tileSize; j++) {
                newV = y + j / (tileSize - 1.0f) * height;

                for (var i = 0; i < tileSize; i++) {
                    newU = x + i / (tileSize - 1.0f) * width;
                    newUV[index] = new Vector2(newU, newV);
                    index++;
                }
            }

            index = 0;
            float centerJ = tileSize / 2;
            for (var k = 0; k < tileSize; k++) {
                newV = y + centerJ / (tileSize - 1.0f) * height;

                for (var i = 0; i < tileSize; i++) {
                    newU = x + i / (tileSize - 1.0f) * width;
                    newUV[index + tileSize * tileSize] = new Vector2(newU, newV);
                    index++;
                }
            }

            newMesh.SetUVs(0, newUV);

            var newMeshChild = new GameObject(label);
            newMeshChild.transform.parent = this.transform;
            newMeshChild.transform.localRotation = Quaternion.identity;
            newMeshChild.AddComponent<MeshFilter>();
            newMeshChild.AddComponent<MeshRenderer>();
            newMeshChild.GetComponent<MeshFilter>().mesh = newMesh;


            return newMeshChild;
        }

        private void MakeCollider(GameObject tile) {
            var heightTexture = material.GetTexture("_HeightMap");

            tile.AddComponent<MeshCollider>();
            tile.GetComponent<MeshCollider>().sharedMesh = tile.GetComponent<MeshFilter>().mesh;
        }

        void Awake() {
            tiles = new GameObject[numTiles, numTiles];
            MakeVerticesTriangles(1f / numTiles);
            for (var j = 0; j < numTiles; j++) {
                for (var i = 0; i < numTiles; i++) {
                    //float x, float y, float width, float height, string label = "Tile"
                    var tile = MakeTile(i * 1.0f / numTiles, j * 1.0f / numTiles, 1.0f / numTiles,
                        1.0f / numTiles, "Tile_" + i + "_" + j);

                    tile.transform.localPosition =
                        new Vector3((i + 0.5f) / numTiles - 0.5f, 0, (j + 0.5f) / numTiles - 0.5f);
                    tile.GetComponent<MeshRenderer>().material = material;
                    tile.transform.localScale = Vector3.one;

                    MakeCollider(tile);

                    tiles[j, i] = tile;
                }
            }
        }

        private void Update() {
            var heightTexture = material.GetTexture("_HeightMap"); //zData
            float scale = 1;
            if (heightTexture == null) return;
            transform.localScale = heightTexture.width > heightTexture.height
                ? new Vector3(scale, -scale, heightTexture.height * scale / heightTexture.width)
                : new Vector3(heightTexture.width * scale / heightTexture.height, -scale, scale);
        }
    }

}