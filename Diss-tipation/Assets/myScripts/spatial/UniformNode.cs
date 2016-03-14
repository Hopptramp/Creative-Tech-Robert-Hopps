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

            getObjectsHeld(particle, ref neighbours, maxInteractions, cellspace2Sq);

            //ParticleClass particle = particleOBJ.GetComponent<ParticleClass>();
            
            if(this != particle.currentNode)
            {
                Debug.Log("in wrong node");
            }
            
            for (int i = 0; i < surroundingBounds.Length; ++i)
            {
                if (surroundingBounds[i] != null)
                {
                    if (Vector3.Distance(particle.position, surroundingBounds[i].bounds.center) < smoothingDistance + surroundingBounds[i].bounds.extents.x)
                    {
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
                    if (objectsHeld[i] != particleOBJ)
                    {
                        if (debugCounter + i >= maxInteractions)
                        {
                           // Debug.Log("too many neighbours");
                        }
                        else
                        {
                            if((particleOBJ.position - objectsHeld[i].position).sqrMagnitude <= cellspace2Sq)
                            {
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