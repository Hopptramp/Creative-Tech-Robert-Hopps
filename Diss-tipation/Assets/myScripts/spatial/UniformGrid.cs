using UnityEngine;
using System.Collections.Generic;
namespace fluidClasses
{
    public class UniformGrid : MonoBehaviour
    {
        Vector3 initialPos;
        float minNodeSize;
        float resolution;
        float height;
        Vector3 offset;

        GameObject[,,] nodes;

        // Use this for initialization
        void Start()
        {

        }

        public void init(Vector3 force, Vector3 initialPosition, float _minNodeSize, float _resolution, float _height)
        {
            nodes = new GameObject[(int)_resolution, (int)_height, (int)_resolution];
            minNodeSize = _minNodeSize;
            resolution = _resolution;
            initialPos = initialPosition;
            height = _height;
            createGrid(_resolution, _minNodeSize, initialPosition, force, height);
        }

        void createGrid(float resolution, float minNodeSize, Vector3 initialPos, Vector3 force, float height)
        {
            for (int x = 0; x < resolution; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int z = 0; z < resolution; ++z)
                    {
                        GameObject nodeOBJ = new GameObject("Node " + x + ", " + y + ", " + z);
                        UniformNode node = nodeOBJ.AddComponent<UniformNode>();
                        Vector3 center = new Vector3(x * minNodeSize, y * minNodeSize, z * minNodeSize);
                        Vector3 size = new Vector3(minNodeSize, minNodeSize, minNodeSize);
                        float gridSize = (resolution * minNodeSize) / 2;
                        float gridHeight = (height * minNodeSize) / 2;
                        offset = new Vector3(gridSize, gridHeight, gridSize);
                        node.createNode((initialPos + center) - offset, size);
                        node.index = new Vector3(x, y, z);
                        nodeOBJ.transform.parent = gameObject.transform;


                        nodes[x, y, z] = nodeOBJ;
                    }
                }
            }
            createGridNeighbours();
        }

        void createGridNeighbours()
        {
            //value = data[ i * height * depth + j * depth + k ];
            for (int x = 1; x < resolution - 1; ++x)
            {
                for (int y = 1; y < height - 1; ++y)
                {
                    for (int z = 1; z < resolution - 1; ++z)
                    {
                        UniformNode node = nodes[x, y, z].GetComponent<UniformNode>();
                        Vector3 ind = node.index;
                        
                        node.surroundingBounds[0] = nodes[x + 1, y + 1, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[1] = nodes[x + 1, y + 1, z].GetComponent<UniformNode>();
                        node.surroundingBounds[2] = nodes[x + 1, y, z].GetComponent<UniformNode>();
                        node.surroundingBounds[3] = nodes[x, y + 1, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[4] = nodes[x, y + 1, z].GetComponent<UniformNode>();
                        node.surroundingBounds[5] = nodes[x, y, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[6] = nodes[x, y + 1, z].GetComponent<UniformNode>();
                        node.surroundingBounds[7] = nodes[x - 1, y - 1, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[8] = nodes[x - 1, y - 1, z].GetComponent<UniformNode>();
                        node.surroundingBounds[9] = nodes[x - 1, y, z].GetComponent<UniformNode>();
                        node.surroundingBounds[10] = nodes[x, y - 1, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[11] = nodes[x, y, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[12] = nodes[x, y - 1, z].GetComponent<UniformNode>();
                        node.surroundingBounds[13] = nodes[x - 1, y + 1, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[14] = nodes[x + 1, y - 1, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[15] = nodes[x + 1, y + 1, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[16] = nodes[x - 1, y - 1, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[17] = nodes[x - 1, y, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[18] = nodes[x - 1, y + 1, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[19] = nodes[x - 1, y, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[20] = nodes[x + 1, y, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[21] = nodes[x, y - 1, z + 1].GetComponent<UniformNode>();
                        node.surroundingBounds[22] = nodes[x, y + 1, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[23] = nodes[x - 1, y + 1, z].GetComponent<UniformNode>();
                        node.surroundingBounds[24] = nodes[x + 1, y, z - 1].GetComponent<UniformNode>();
                        node.surroundingBounds[25] = nodes[x + 1, y - 1, z].GetComponent<UniformNode>();
                    }
                }
            }
        
        }

        public void addParticleToGrid(ParticleClass particle, bool remove)
        {
            if (remove)
            {
                removeParticle(particle);
                Vector3 ID = particle.gridID;

                UniformNode[] surrounding = nodes[(int)ID.x, (int)ID.y, (int)ID.z].GetComponent<UniformNode>().surroundingBounds;

                for (int i = 0; i < surrounding.Length; ++i)
                {
                    if (surrounding[i] != null)
                    {
                        if (surrounding[i].bounds.Contains(particle.transform.position))
                        {
                            surrounding[i].objectsHeld.Add(particle);
                            particle.gridID = surrounding[i].index;
                            particle.lastGridPos = surrounding[i].bounds.center;
                            particle.currentNode = surrounding[i];
                            break;
                        }
                    }
                }
            }
            else
            {
                for (int x = 0; x < resolution; ++x)
                {
                    for (int y = 0; y < height; ++y)
                    {
                        for (int z = 0; z < resolution; ++z)
                        {
                            UniformNode node = nodes[x, y, z].GetComponent<UniformNode>();
                            // so inefficient soz
                            if (node.bounds.Contains(particle.transform.position))
                            {
                                node.objectsHeld.Add(particle);
                                particle.gridID = node.index;
                                particle.lastGridPos = node.bounds.center;
                                particle.currentNode = node;

                                x = (int)resolution;
                                y = (int)height;
                                z = (int)resolution;
                            }
                        }
                    }
                }
            }
        }

        public void removeParticle(ParticleClass particle)
        {
            Vector3 ID = particle.GetComponent<ParticleClass>().gridID;
            nodes[(int)ID.x, (int)ID.y, (int)ID.z].GetComponent<UniformNode>().objectsHeld.Remove(particle);
        }

        public void getNeighbours(ref int[] neighbours, ParticleClass particle, float smoothingDistance, float maxInteractions, float cellspace2Sq)
        {
            Vector3 ID = particle.gridID;

            nodes[(int)ID.x, (int)ID.y, (int)ID.z].GetComponent<UniformNode>().getNeighbours(ref neighbours, particle, smoothingDistance, maxInteractions, cellspace2Sq);

        }

        void clearGrid()
        {
            for (int x = 0; x < resolution; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int z = 0; z < resolution; ++z)
                    {
                        Destroy(nodes[x,y,z]);
                    }
                }
            }
        }


        // Update is called once per frame
        void Update()
        {

        }

        public void drawBounds()
        {
            for (int x = 0; x < resolution; ++x)
            {
                for (int y = 0; y < height; ++y)
                {
                    for (int z = 0; z < resolution; ++z)
                    {
                        Gizmos.color = Color.green;
                        UniformNode node = nodes[x,y,z].GetComponent<UniformNode>();
                        Gizmos.DrawWireCube(node.bounds.center, node.bounds.size);
                    }
                }
            }
        }
    }
}
