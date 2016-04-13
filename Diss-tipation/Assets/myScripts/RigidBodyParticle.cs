using UnityEngine;
using System.Collections;

namespace fluidClasses
{

    public class RigidBodyParticle : MonoBehaviour
    {
        // serialised fields
        [SerializeField] FluidMain fluid = null;
        [SerializeField] float colliderSizeModifier = 1f;
        [SerializeField] PhysicMaterial physicsMaterial = null;
        [SerializeField] Material material = null;
        [SerializeField] Mesh mesh = null;

        // lists
        Rigidbody[] rigidBodies = null;
        SphereCollider[] sphereColliders = null;

        // parent transform
        Transform parentTrans = null;

        int curentMaxParticles = 0;

        // to be called whenever a particle is created
        public void allocate(int currentParticles, int newMaxParticles, GameObject particleGO)
        {
            // if max particles changes
            if(newMaxParticles != curentMaxParticles)
            {
                // update the maxparticles
                curentMaxParticles = newMaxParticles;

                // resize the arrays
                System.Array.Resize<Rigidbody>(ref rigidBodies, curentMaxParticles);
                System.Array.Resize<SphereCollider>(ref sphereColliders, curentMaxParticles);
            }

            // assign the fluid pointer
            if (!fluid)
            {
                fluid = GetComponent<FluidMain>();
            }

            // for the original particle
            if (!parentTrans)
            {
                GameObject parent = new GameObject(fluid.name + " RigidBody Particles");
                parentTrans = parent.transform;
            }

            // if more particles have been made
            if (currentParticles <= curentMaxParticles)
            {
                // start from previous newest rigidbody
                for (int i = currentParticles; i < fluid._particles.Count; ++i)
                {
                    // create a new gameobject
                    GameObject go = particleGO;
                    go.layer = fluid.gameObject.layer;
                    go.transform.parent = parentTrans;
                    go.transform.position = fluid._particles[i].GetComponent<ParticleClass>().position;

                    // add all the components
                    SphereCollider collider = go.AddComponent<SphereCollider>();
                    Rigidbody rigid = go.AddComponent<Rigidbody>();
                    go.AddComponent<MeshRenderer>();
                    go.GetComponent<MeshRenderer>().material = material;
                    go.AddComponent<MeshFilter>().mesh = mesh;
                    rigidBodies[i] = rigid;
                    sphereColliders[i] = collider;

                    // set up the rigid body
                    rigid.freezeRotation = true;
                    rigid.useGravity = false;
                    rigid.velocity = Vector3.zero;
                    rigid.interpolation = RigidbodyInterpolation.Extrapolate;
                    
                }
            }
            Physics.IgnoreLayerCollision(fluid.gameObject.layer, fluid.gameObject.layer, true);
        }

        // used to remove the particle
        public void removeParticles(int particleID)
        {
            if (rigidBodies[particleID] != null)
            {
                DestroyImmediate(rigidBodies[particleID].gameObject);
            }
        }

        public void preSolve(timeStep timestep)
        {
            // prevent function from running without any particles
            if (rigidBodies == null)
                return;

            int min = Mathf.Min(rigidBodies.Length, fluid._particles.Count);

            for (int i = 0; i < min; ++i)
            {
                // get the components
                Rigidbody rigid = rigidBodies[i];
                SphereCollider collider = sphereColliders[i];
                GameObject particleGO = fluid._particles[i];
                ParticleClass particle = particleGO.GetComponent<ParticleClass>();

                if (!rigid)
                    continue;

                // assign variables
                rigid.mass = fluid.particleMass;
                collider.radius = fluid.smoothingDistance * colliderSizeModifier;
                collider.sharedMaterial = physicsMaterial;

                // update particle position to match rigidbody
                particle.position = rigid.position;
                particle.velocity = rigid.velocity * timestep.dt;

                // update fluid particle list
                fluid._particles[i] = particleGO;
            }
        }

        public void postSolve(timeStep timestep)
        {
            if (rigidBodies == null)
                return;

            int min = Mathf.Min(rigidBodies.Length, fluid._particles.Count);

            for (int i = 0; i < min; ++i)
            {
                Rigidbody rigid = rigidBodies[i];
                ParticleClass particle = fluid._particles[i].GetComponent<ParticleClass>();

                if (!rigid)
                    continue;
                if (fluid._particles[i].GetComponent<ParticleClass>().ID == -1)
                    return;

                // mvoe the rigid body to match the particle
                rigid.MovePosition(particle.position);
                rigid.velocity = particle.velocity * timestep.INVdt;
            }
        }

        // Add clean up stuff
    }

}