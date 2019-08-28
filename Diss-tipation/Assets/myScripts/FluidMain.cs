using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace fluidClasses
{
    public class FluidMain : MonoBehaviour
    {
        // particle list
        internal List<ParticleClass> _particles = new List<ParticleClass>();

        // serialised fields
        [SerializeField] internal float smoothingDistance = 0.38125f;
        [SerializeField] internal float initialDensity = 100f;
        [SerializeField] internal float particleMass = 7f;
        [SerializeField] internal float minimumDensity = 50f;
        [SerializeField] internal float viscosity = 50f;
        [SerializeField] internal int maxInteractions = 50;
        [SerializeField] internal float gasConstant = 0.1f;
        [SerializeField] internal float damping = 0.1f;
        [SerializeField] internal float velocityLimit = 5f;
        [SerializeField] internal float velocityDampen = 0.5f;
        [SerializeField] int solverIterationCount = 1;
        [SerializeField] bool useOctree = false;
        [SerializeField] internal bool useUniform = true;
        
        
        internal int particleCount = 0;
        Emitter emitter;
        Solver solver;
        RigidBodyParticle rigid;
        internal Octree<ParticleClass> octree;
        public int counter = 0;

        // uniform grid variables
        internal GameObject uniformOBJ;
        [SerializeField] float uniformMinNodeSize = 3;
        [SerializeField] float uniformResolution = 15;
        [SerializeField] float uniformHeight = 5;

        Vector3 gravity = new Vector3(0.0f, -0.98f, 0.0f);


        public void spawnParticle(Emitter _emitter, Vector3 position, Vector3 force)
        {
            // create the uniform grid
            if (uniformOBJ == null)
            {
                if (useUniform)
                {
                    uniformOBJ = new GameObject("Uniform Grid");
                    UniformGrid uniform = uniformOBJ.AddComponent<UniformGrid>();
                    uniform.init(force, position, uniformMinNodeSize, uniformResolution, uniformHeight);
                }
            }
            //if using an octree
            if (useOctree)
            {
                if (octree == null)
                {
                    octree = new Octree<ParticleClass>(7f, emitter.transform.position, 1);
                }
            }

            emitter = _emitter;
            int count = emitter.maxParticles;
            int oldCount = _particles.Count;

            // if not using uniform 
            if (!useUniform)
            {
                // change max interactions to match max particles
                maxInteractions = emitter.maxParticles;
            }

            // if the maximum number of particles has not been reached
            if (oldCount < count)
            {
                // create new game object with the particle component
                GameObject particleGO = new GameObject("particle" + counter);
                particleGO.AddComponent<ParticleClass>();
                // increment counter
                

                // create a new one
                ParticleClass particle = particleGO.GetComponent<ParticleClass>();
                particle.ID = counter++;
                particle.isMovedNode = false;
                particle.isDead = false; 

                // assign the neighbours array
                System.Array.Resize<int>(ref particle.neighbours, maxInteractions);

                // default all the neighbours to -1 (null)
                for (int i = 0; i < particle.neighbours.Length; ++i)
                {
                    particle.neighbours[i] = -1;
                }

                // add the particle to the list
                _particles.Add(particle);

                // initialise the particle
                particle.intitialise(emitter, ref position, ref force);
                // allocate it's rigid body
                rigid.allocate(oldCount, count, particleGO);

                // add the particle to the uniform grid
                if(useUniform)
                   uniformOBJ.GetComponent<UniformGrid>().addParticleToGrid(particle, false);                
            }
        }

        public void updateGridLocation(GameObject ParticleOBJ)
        {
            // get the particle
            ParticleClass particle = ParticleOBJ.GetComponent<ParticleClass>();
            // check it's in a node
            if (particle.currentNode != null)
            {
                // check the current node does not actually contain the particle
                if (!particle.currentNode.bounds.Contains(particle.position))
                {
                    // if not, find a new node to fit it
                    uniformOBJ.GetComponent<UniformGrid>().addParticleToGrid(particle, true);
                    // tell the particle it has been moved
                    particle.isMovedNode = true;
                }
            }
        }

        public void removeParticle(ParticleClass particle, int arrayPos)
        {
            // decrement the counters
            --particle.emitter.particleCount;
            --particleCount;
            --counter;

            // if using octree remove from it
            if (useOctree)
            {
                octree.Remove(particle, particle.ID);
            }

            // if useing uniform grid remove from it
            if(useUniform)
            {
                uniformOBJ.GetComponent<UniformGrid>().removeParticle(particle);
            }

            // remove particle from the gameobject list
            _particles.Remove(particle);
            // destroy the particle
            DestroyImmediate(particle.gameObject);
           
        }

        void Awake()
        {
            // initialise other scripts
            solver = GetComponent<Solver>();
            solver.init(this);
            rigid = gameObject.GetComponent<RigidBodyParticle>();

            
        }

        // this is the core of the simulation
        void FixedUpdate()
        {
            // create a new timestep + init gravity
            TimeStep timestep = new TimeStep(Time.deltaTime, solverIterationCount);

            // if there are any particles
            if (_particles.Count > 0)
            {
                // presolve
                rigid.preSolve(timestep);

                // solver function (contains SPH method)
                solver.PreSolve(ref timestep);
                solver.findNeighbours();
                solver.Solve(timestep);
                solver.integrateVelocities(ref timestep, ref gravity);

                // clear forces 
                solver.clearForces();

                // post solve
                rigid.postSolve(timestep);
            }
        }

        // render the bounding boxes for spatial partitioning systems
        void OnDrawGizmos()
        {
            if(useUniform)
                if(uniformOBJ != null)
                    uniformOBJ.GetComponent<UniformGrid>().drawBounds();

            if (useOctree)
                octree.DrawAllObjects();

        }


    }
}
