using UnityEngine;
using System.Collections.Generic;

namespace fluidClasses
{
    public class Solver : MonoBehaviour
    {
        FluidMain fluid;
        public Transform origin {get;set;}

        float cellspace;
        float cellspace2;
        float cellspace2Sq;
        float minDistance;

        poly6SmoothingKernel densityKernel;
        spikySmoothingKernel pressureKernel;
        viscositySmoothingKernel viscosityKernel;

        ParticleClass queryParticle;
        UniformGrid grid;

        public void init(FluidMain _fluid)
        {
            fluid = _fluid;
            //origin = _player;

            setCellSpace(fluid);
        }

        void setCellSpace(FluidMain fluid)
        {
            // initialise cells
            cellspace = fluid.smoothingDistance;
            cellspace2 = cellspace * 2;
            cellspace2Sq = cellspace2 * cellspace2;
            minDistance = 0.5f * cellspace;

            // if first time set up
            if (densityKernel == null)
            {
                GameObject kernels = new GameObject();
                kernels.name = "Kernels";
                kernels.transform.parent = transform;
                densityKernel = kernels.AddComponent<poly6SmoothingKernel>();
                pressureKernel = kernels.AddComponent<spikySmoothingKernel>();
                viscosityKernel = kernels.AddComponent<viscositySmoothingKernel>();
            }
            // initialise the kernels
            densityKernel.initialise(fluid.smoothingDistance);
            pressureKernel.initialise(fluid.smoothingDistance);
            viscosityKernel.initialise(fluid.smoothingDistance);
        }

        // clear the particles of any force
        public void clearForces()
        {
            ParticleClass particle;

            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i];
                particle.force = Vector3.zero;
                fluid._particles[i] = particle;
            }
        }


        // find the particles within cellspace range of the query particle (very inefficient)
        public void findNeighbours()
        {
            if (fluid.uniformOBJ != null)
            {
                if (grid == null)
                    grid = fluid.uniformOBJ.GetComponent<UniformGrid>();
            }
            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                queryParticle = fluid._particles[i];

                if (queryParticle.isMovedNode)
                {

                    // reset all the neighbours to null
                    for (int j = 0; j < queryParticle.neighbours.Length; ++j)
                    {
                        queryParticle.neighbours[j] = -1;
                    }
                    // find the neighbours
                    grid.getNeighbours(ref queryParticle.neighbours, queryParticle, fluid.smoothingDistance, fluid.maxInteractions, cellspace2Sq);

                    queryParticle.isMovedNode = false;
                    fluid._particles[i] = queryParticle;
                }


                if (fluid.octree == null && fluid.uniformOBJ == null)
                {
                    int k = 0;
                    for (int j = 0; j < fluid._particles.Count; ++j)
                    {
                        if (j != i)
                        {
                            if ((queryParticle.position - fluid._particles[j].position).sqrMagnitude <= cellspace2Sq)
                            {
                                queryParticle.neighbours[k++] = j;
                            }
                            else
                            {
                                queryParticle.neighbours[k++] = -1;
                            }
                        }
                    }
                }


                //else
                //{
                //    Ray ray = new Ray(queryParticle.position, queryParticle.velocity);

                //    //ParticleClass[] neighbours = fluid.octree.getNearby(queryParticle.ID, ray, fluid.smoothingDistance);
                //    for (int j = 0; j < neighbours.Length; ++j)
                //    {
                //        queryParticle.neighbours[i] = neighbours[j].ID;
                //    }
                //}
                fluid._particles[i] = queryParticle;
            }
        }

        // check max distance/particlelife/enforce minimum distances
        public void PreSolve(ref TimeStep timestep)
        {
            ParticleClass particle;

            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                // get the particle
                particle = fluid._particles[i];

                // if the particle moves far enough away from the player, remove it
                //if (particle.emitter.maxParticleDistance < Mathf.Infinity)
                //{
                //    if (Vector3.Distance(particle.position, origin.position) > particle.emitter.maxParticleDistance)
                //    {
                //        //fluid.removeParticle(particle, particle.ID);
                //        particle.isDead = true;
                //        continue;
                //    }
                //}

                // if using lifetime, remove the particle after a certain time
                if (particle.timed)
                {
                    particle.particleLife -= timestep.dt;

                    if (particle.particleLife <= 0)
                    { 
                        //fluid.removeParticle(particle, particle.ID);
                        particle.isDead = true;
                        continue;
                    }
                }

                // if using uniform grid, update the particle's node
                if (fluid.useUniform)
                    fluid.updateGridLocation(particle.gameObject);

                // update particle list
                fluid._particles[i] = particle;
            }

            // remove the particles marked for dead
            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i];

                if(particle.isDead)
                {
                    fluid.removeParticle(particle, particle.ID);
                }
            }
      }

        public void Solve(TimeStep timestep)
        {
            // calls functions
            calculateDensityandPressure();
            calculateForces(ref timestep);
        }

        // self explanatory
        void calculateDensityandPressure()
        {
            Vector3 distance;
            ParticleClass particle;
            ParticleClass neighbour;

            // for each particle
            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i];
                particle.density = 0.0f;
                // for each of their neighbours
                for (int j = 0; j < particle.neighbours.Length; ++j)
                {
                    if (particle.neighbours[j] < 0)
                        continue;
                    if (particle.neighbours[j] == -1)
                        continue;
                    neighbour = fluid._particles[particle.neighbours[j]];
                    // calculate the distance between
                    distance = particle.position - neighbour.position;
                    // add the calculated density onto the particle
                    particle.density += particle.mass * densityKernel.CalculateDensity(ref distance);
                }
                // decide between calculated density or minimum density
                particle.density = Mathf.Max(particle.density, fluid.minimumDensity);

                // calculate pressure
                particle.pressure = fluid.gasConstant * (particle.density * fluid.initialDensity);
                // apply changes to fluid particles
                fluid._particles[i] = particle;
            }
        }

        void calculateForces(ref TimeStep timestep)
        {
            ParticleClass particle;
            ParticleClass neighbour;

            //for each particle
            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i];
                // for each of their neighbours
                for (int j = 0; j < particle.neighbours.Length; ++j)
                {
                    if (particle.neighbours[j] == -1)
                        continue;

                    neighbour = fluid._particles[particle.neighbours[j]];

                    solveSPH(ref particle, ref neighbour);

                    fluid._particles[particle.neighbours[j]] = neighbour;
                }
            }
        }

        void solveSPH(ref ParticleClass particle, ref ParticleClass neighbour)
        {
            Vector3 distance;
            Vector3 force;
            float scalar;

            //calculate distance between the particles
            distance = particle.position - neighbour.position;
            // calculate the scalar
            scalar = neighbour.mass * (particle.pressure + neighbour.pressure) / (2.0f * neighbour.density);
            // calculate the force
            force = pressureKernel.CalculateGradient(ref distance);
            force *= scalar;

            particle.force -= force;
            neighbour.force += force;

            // viscosity
            scalar = neighbour.mass * viscosityKernel.calculateLaplacian(ref distance) * fluid.viscosity * (1.0f / neighbour.density);

            force = neighbour.velocity - particle.velocity;
            force *= scalar;

            particle.force += force;
            neighbour.force -= force;
        }

        public void integrateVelocities(ref TimeStep timestep, ref Vector3 gravity)
        {
            // create local variables
            Vector3 acceleration;
            Vector3 temp;
            float distSqMag;
            float distMag;
            ParticleClass particle;
            ParticleClass neighbour;

            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                // get particle
                particle = fluid._particles[i];

                // assign variables
                acceleration = particle.force * (1.0f / particle.mass);
                acceleration += gravity;
                temp = timestep.dt2_iter * acceleration;
                particle.velocity += temp;
                particle.velocity *= Mathf.Clamp01(1.0f - timestep.dt_iter * fluid.damping);

                // limit the velocity
                if (particle.velocity.sqrMagnitude > fluid.velocityLimit * fluid.velocityLimit)
                {
                    particle.velocity = Vector3.ClampMagnitude(particle.velocity, fluid.velocityLimit + ((fluid.velocityLimit - particle.velocity.magnitude) * (1.0f - Mathf.Clamp01(fluid.velocityDampen))));
                }

                for (int j = 0; j < particle.neighbours.Length; ++j)
                {
                    // if the neighbour is null
                    if (particle.neighbours[j] == -1)
                        continue;

                    // get the neighbour particle matching the ID
                    neighbour = fluid._particles[particle.neighbours[j]];

                    if (particle.neighbours[j] != 0)
                    {
                        // find the distance between the particles
                        temp = neighbour.position - particle.position;
                        distSqMag = temp.sqrMagnitude;

                        // if in range
                        if (distSqMag < minDistance)
                        {
                            // if above minimum distance
                            if (distSqMag > Vector3.kEpsilon)
                            {
                                // create force
                                distMag = Mathf.Sqrt(distSqMag);
                                temp *= (0.5f * (distMag - minDistance) / distMag);
                                // apply force
                                neighbour.position -= temp;
                                neighbour.velocity -= temp;
                                particle.position += temp;
                                particle.velocity += temp;
                            }
                            else
                            {
                                // create own force variable
                                distMag = 0.5f * minDistance;
                                // apply forces
                                neighbour.position.y -= distMag;
                                neighbour.velocity.y -= distMag;
                                particle.position.y += distMag;
                                particle.velocity.y += distMag;
                            }
                        }
                        // apply changes to neighbour
                        fluid._particles[particle.neighbours[j]] = neighbour;
                    }
                }
                //apply changes to particle
                fluid._particles[i] = particle;
            }
        }
    }
}