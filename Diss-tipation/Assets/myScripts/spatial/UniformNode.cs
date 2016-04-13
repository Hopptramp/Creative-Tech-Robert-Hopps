using UnityEngine;
using System.Collections.Generic;

namespace fluidClasses
{
    public class UniformNode : MonoBehaviour
    {
        public List<ParticleClass> objectsHeld = null;
        public Bounds bounds;

        public Vector3 index;

        public UniformNode[] surroundingBounds;
        int debugCounter = 0;

        // Use this for initialization
        void Start()
        {

        }

        public void getNeighbours(ref int[] neighbours, ParticleClass particle, float smoothingDistance, float maxInteractions, float cellspace2Sq)
        {
            debugCounter = 0;
            // get all the particles in this node
            getObjectsHeld(particle, ref neighbours, maxInteractions, cellspace2Sq);
            
            // ensure the correct node is used
            if(this != particle.currentNode)
            {
                Debug.Log("in wrong node");
            }
            
            // check all the surrounding bounds
            for (int i = 0; i < surroundingBounds.Length; ++i)
            {
                if (surroundingBounds[i] != null)
                {
                    // check if the particle is within a detectable range to the node
                    if (Vector3.Distance(particle.position, surroundingBounds[i].bounds.center) < smoothingDistance + surroundingBounds[i].bounds.extents.x)
                    {
                        // if it is, get the particles held in the node
                        surroundingBounds[i].getObjectsHeld(particle, ref neighbours, maxInteractions, cellspace2Sq);
                    }
                }
            }
        }

        void getObjectsHeld(ParticleClass particleOBJ, ref int[] neighbours, float maxInteractions, float cellspace2Sq)
        {
            if (objectsHeld.Count > 0)
            {
                for (int i = 0; i < objectsHeld.Count; ++i)
                {
                    // don't use the current particle as a neighbour
                    if (objectsHeld[i] != particleOBJ)
                    {
                        if (debugCounter + i >= maxInteractions)
                        {
                            // this had to be commented out to prevent frame drop
                            //Debug.Log("too many neighbours");
                        }
                        else
                        {
                            // check if the particle is within the smoothing distance range
                            if((particleOBJ.position - objectsHeld[i].position).sqrMagnitude <= cellspace2Sq)
                            {
                                // add the particle to the neighbour array
                                neighbours[debugCounter + i] = objectsHeld[i].GetComponent<ParticleClass>().ID;
                                ++debugCounter;
                            }

                           
                        }
                    }
                }
            }
        }

        public void createNode(Vector3 center, Vector3 size)
        {
            // initialise the lists/arrays/bounds
            objectsHeld = new List<ParticleClass>();
            bounds = new Bounds(center, size);
            surroundingBounds = new UniformNode[26];
        }


        // Update is called once per frame
        void Update()
        {

        }
    }
}