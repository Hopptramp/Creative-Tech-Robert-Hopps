using UnityEngine;
using System.Collections;

namespace fluidClasses
{
    //ensure a particle system is included in the gameobject
    [RequireComponent(typeof(ParticleSystem))]

    public class fluidRenderer : MonoBehaviour
    {
        ParticleSystem particleSystem;
        ParticleSystem.Particle[] renderParticles;
        int count;

        
    }
}