using UnityEngine;
using System.Collections.Generic;

namespace fluidClasses
{
    public class Solver : MonoBehaviour
    {
        FluidMain fluid;

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
                kernels.AddComponent<poly6SmoothingKernel>();
                densityKernel = kernels.GetComponent<poly6SmoothingKernel>();
                kernels.AddComponent<spikySmoothingKernel>();
                pressureKernel = kernels.GetComponent<spikySmoothingKernel>();
                kernels.AddComponent<viscositySmoothingKernel>();
                viscosityKernel = kernels.GetComponent<viscositySmoothingKernel>();
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
                particle = fluid._particles[i].GetComponent<ParticleClass>();
                particle.force = Vector3.zero;
                fluid._particles[i] = particle.gameObject;
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
                queryParticle = fluid._particles[i].GetComponent<ParticleClass>();

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
                    fluid._particles[i] = queryParticle.gameObject;
                }


                if (fluid.octree == null && fluid.uniformOBJ == null)
                {
                    int k = 0;
                    for (int j = 0; j < fluid._particles.Count; ++j)
                    {
                        if (j != i)
                        {
                            if ((queryParticle.position - fluid._particles[j].GetComponent<ParticleClass>().position).sqrMagnitude <= cellspace2Sq)
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
                fluid._particles[i] = queryParticle.gameObject;
            }
        }

        // check max distance/particlelife/enforce minimum distances
        public void preSolve(ref timeStep timestep)
        {
            ParticleClass particle;
            int particlesRemoved = 0;

            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i].GetComponent<ParticleClass>();

                if (particle.currentNode.surroundingBounds[0] == null)
                {
                    //fluid.removeParticle(particle, particle.ID);
                    particle.isDead = true;
                    continue;
                }

                if (particle.emitter.maxParticleDistance < Mathf.Infinity)
                {
                    if (Vector3.Distance(particle.position, GameObject.Find("Player").transform.position) > particle.emitter.maxParticleDistance)
                    {

                        //fluid.removeParticle(particle, particle.ID);
                        particle.isDead = true;
                        continue;
                    }
                }

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

                if (fluid.useUniform)
                    fluid.updateGridLocation(particle.gameObject);

                fluid._particles[i] = particle.gameObject;
            }
            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i].GetComponent<ParticleClass>();

                if(particle.isDead)
                {
                    fluid.removeParticle(particle, particle.ID);
                }
            }
      }

        public void Solve(timeStep timestep)
        {

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
                particle = fluid._particles[i].GetComponent<ParticleClass>();
                particle.density = 0.0f;
                // for each of their neighbours
                for (int j = 0; j < particle.neighbours.Length; ++j)
                {
                    if (particle.neighbours[j] < 0)
                        continue;
                    if (particle.neighbours[j] == -1)
                        continue;
                    neighbour = fluid._particles[particle.neighbours[j]].GetComponent<ParticleClass>();
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
                fluid._particles[i] = particle.gameObject;
            }
        }

        void calculateForces(ref timeStep timestep)
        {
            ParticleClass particle;
            ParticleClass neighbour;

            //for each particle
            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i].GetComponent<ParticleClass>();
                // for each of their neighbours
                for (int j = 0; j < particle.neighbours.Length; ++j)
                {
                    if (particle.neighbours[j] == -1)
                        continue;

                    neighbour = fluid._particles[particle.neighbours[j]].GetComponent<ParticleClass>();

                    solveSPH(ref particle, ref neighbour);

                    fluid._particles[particle.neighbours[j]] = neighbour.gameObject;
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

        public void integrateVelocities(ref timeStep timestep, ref Vector3 gravity)
        {
            Vector3 acceleration;
            Vector3 temp;
            float distSqMag;
            float distMag;
            ParticleClass particle;
            ParticleClass neighbour;

            for (int i = 0; i < fluid._particles.Count; ++i)
            {
                particle = fluid._particles[i].GetComponent<ParticleClass>();

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
                    if (particle.neighbours[j] == -1)
                        continue;
                    neighbour = fluid._particles[particle.neighbours[j]].GetComponent<ParticleClass>();

                    if (particle.neighbours[j] != 0)
                    {
                        temp = neighbour.position - particle.position;
                        distSqMag = temp.sqrMagnitude;

                        if (distSqMag < minDistance)
                        {
                            if (distSqMag > Vector3.kEpsilon)
                            {
                                distMag = Mathf.Sqrt(distSqMag);
                                temp *= (0.5f * (distMag - minDistance) / distMag);
                                neighbour.position -= temp;
                                neighbour.velocity -= temp;
                                particle.position += temp;
                                particle.velocity += temp;
                            }
                            else
                            {
                                distMag = 0.5f * minDistance;

                                neighbour.position.y -= distMag;
                                neighbour.velocity.y -= distMag;
                                particle.position.y += distMag;
                                particle.velocity.y += distMag;
                            }
                        }
                        // apply changes to neighbour
                        fluid._particles[particle.neighbours[j]] = neighbour.gameObject;
                    }
                }
                //apply changes to particle
                fluid._particles[i] = particle.gameObject;
            }
        }
    }
}