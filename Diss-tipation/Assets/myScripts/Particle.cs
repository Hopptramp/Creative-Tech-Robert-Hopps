using UnityEngine;
using System.Collections;

namespace fluidClasses
{ 

    public class ParticleClass : MonoBehaviour
    {
        // particle properties

        // floats
        internal float mass;
        internal float density;
        internal float smoothingDistance;
        internal float pressure;
        internal float particleLife;

        // vector3
        internal Vector3 force;
        internal Vector3 position;
        internal Vector3 velocity;
        internal Vector3 gridID;
        internal Vector3 lastGridPos;

        // ints
        internal int[] neighbours;
        internal int ID;

        //bools
        internal bool isDead;
        internal bool isMovedNode;
        internal bool timed;

        // pointers
        internal Emitter emitter;
        internal UniformNode currentNode;
       

        
        //add force to the particle
        public void addForce(Vector3 _force)
        {
            force += _force;
        }

        // get neighbours
        public void getNeighbours(ref int[] _neighbours)
        {
            // resize the referenced neighbour array to match this particles neighbour array
            System.Array.Resize<int>(ref _neighbours, neighbours.Length);
            // copy this neighbour array over the referenced array
            System.Array.Copy(neighbours, _neighbours, _neighbours.Length);
        }

        // intitialisation
        internal void intitialise(Emitter _emitter, ref Vector3 _position, ref Vector3 _force)
        {
            // assign emitter
            emitter = _emitter;
            // assign vectors
            position = _position;
            velocity = Vector3.zero;
            force = _force;
            
            // assign floats          
            density = emitter.fluid.initialDensity;
            mass = emitter.fluid.particleMass;
            smoothingDistance = emitter.fluid.smoothingDistance;
            particleLife = emitter.particleLife;

            emitter.particlesList.Add(this);
            emitter.particleCount++;

            
            
        }


    }
}
