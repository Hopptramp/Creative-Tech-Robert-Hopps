using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace fluidClasses
{
    public class FluidMain : MonoBehaviour
    {

        internal List<GameObject> _particles = new List<GameObject>();

        [SerializeField]
        internal float smoothingDistance = 0.38125f;
        [SerializeField]
        internal float initialDensity = 100f;
        [SerializeField]
        internal float particleMass = 7f;
        [SerializeField]
        internal float minimumDensity = 50f;
        [SerializeField]
        internal float viscosity = 50f;
        [SerializeField]
        internal int maxInteractions = 50;
        [SerializeField]
        internal float gasConstant = 0.1f;
        [SerializeField]
        internal float damping = 0.1f;
        [SerializeField]
        internal float velocityLimit = 5f;
        [SerializeField]
        internal float velocityDampen = 0.5f;
        [SerializeField]
        int solverIterationCount = 1;
        [SerializeField] bool useOctree = false;
        [SerializeField] internal bool useUniform = true;
        //the no. of active particles
        internal int particleCount = 0;
        Emitter emitter;
        Solver solver;
        RigidBodyParticle rigid;
        internal Octree<ParticleClass> octree;
        int counter = 0;

        internal GameObject uniformOBJ;
        [SerializeField] float uniformMinNodeSize = 3;
        [SerializeField] float uniformResolution = 15;
        [SerializeField] float uniformHeight = 5;
        [SerializeField] float particleGridDistance = 2;
        


        public void spawnParticle(Emitter _emitter, Vector3 position, Vector3 force)
        {
            // create the uniform grid
            if (uniformOBJ == null)
            {
                if (useUniform)
                {
                    uniformOBJ = new GameObject();
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

                System.Array.Resize<int>(ref particle.neighbours, maxInteractions);

                for (int i = 0; i < particle.neighbours.Length; ++i)
                {
                    particle.neighbours[i] = -1;
                }

                _particles.Add(particleGO);

                // initialise the particle
                particle.intitialise(emitter, ref position, ref force);
                // allocate it's rigid body
                rigid.allocate(oldCount, count, particleGO);

                if(useUniform)
                   uniformOBJ.GetComponent<UniformGrid>().addParticleToGrid(particle, false);                
            }
        }

        public void updateGridLocation(GameObject ParticleOBJ)
        {

            ParticleClass particle = ParticleOBJ.GetComponent<ParticleClass>();
            if (particle.currentNode != null)
            {
                if (!particle.currentNode.bounds.Contains(particle.position))
                {
                    uniformOBJ.GetComponent<UniformGrid>().addParticleToGrid(particle, true);
                    particle.isMovedNode = true;
                }
            }
        }

        public void removeParticle(ParticleClass particle, int arrayPos)
        {


            --particle.emitter.particleCount;
            --particleCount;

            if (useOctree)
            {
                octree.Remove(particle, particle.ID);
            }
            if(useUniform)
            {
                uniformOBJ.GetComponent<UniformGrid>().removeParticle(particle);
            }

            _particles.Remove(particle.gameObject);
            DestroyImmediate(particle.gameObject);
           
        }

        void Awake()
        {
            solver = GetComponent<Solver>();
            solver.init(this);
            rigid = gameObject.GetComponent<RigidBodyParticle>();

        }

        void FixedUpdate()
        {
            timeStep timestep = new timeStep(Time.deltaTime, solverIterationCount);
            Vector3 gravity = new Vector3(0.0f, -0.98f, 0.0f);

            if (_particles.Count > 0)
            {

                rigid.preSolve(timestep);


                if (useOctree)
                {
                    for (int i = 0; i < _particles.Count; ++i)
                    {
                        octree.moveParticle(_particles[i].GetComponent<ParticleClass>(), _particles[i].GetComponent<ParticleClass>().position, smoothingDistance, _particles[i].GetComponent<ParticleClass>().ID);
                    }
                }

                solver.preSolve(ref timestep);
                solver.findNeighbours();
                solver.Solve(timestep);
                solver.integrateVelocities(ref timestep, ref gravity);

                solver.clearForces();

                rigid.postSolve(timestep);
            }
        }


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
