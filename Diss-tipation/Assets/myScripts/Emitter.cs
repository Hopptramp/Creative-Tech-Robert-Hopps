using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace fluidClasses
{
    public class Emitter : MonoBehaviour
    {
        // list of particles
        internal List<ParticleClass> particlesList = new List<ParticleClass>(100);

        // serialised fields
        [SerializeField] internal FluidMain fluid;
        [SerializeField] internal bool emit = false;
        [SerializeField] internal int maxParticles = 50; 
        [SerializeField] internal float maxParticleDistance = Mathf.Infinity;
        [SerializeField] internal float particleLife = Mathf.Infinity;
        [SerializeField] float radius = 60.0f;
        [SerializeField] internal Vector3 position = Vector3.down; // to be changed
        [SerializeField] internal Vector3 endPosition = Vector3.zero; // to be changed

        internal int particleCount = 0;
        Vector3 randomVector;
        
        void Awake()
        {
            fluid = gameObject.transform.parent.GetComponent<FluidMain>();
        }

        void Update()
        {
            if (emit)
            {
                if (particleCount < maxParticles)
                {
                    emitParticle();
                }
            }
        }

        // call this to start emitting particles
        public void Emit (bool _emit)
        {
            emit = _emit;
        }

        public void emitParticle()
        {
            fluid.spawnParticle(this, changeEmitPosition(), changeEmitForce());
        }

        Vector3 changeEmitPosition()
        {
            // so that particles do not all spawn together
            randomVector = Random.insideUnitSphere * radius;
            Vector3 Position = (gameObject.transform.TransformPoint(position) + randomVector);
            return Position;
        }
    
        Vector3 changeEmitForce()
        {
            //vary the force
            Vector3 force = gameObject.transform.TransformDirection(endPosition - position);
            return force;
        }

        internal void removeParticles(ParticleClass particle)
        {
            if (particleCount <= 0)
                return;

            for (int i = 0; i < particleCount; ++i)
            {
                if (particlesList[i].ID == particle.ID)
                {
                    particlesList.RemoveAt(i);
                    return;
                }
            }
        }
    }
}