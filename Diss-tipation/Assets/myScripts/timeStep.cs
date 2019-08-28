using UnityEngine;
using System.Collections;

namespace fluidClasses
{
    public struct TimeStep
    {
        public readonly float iter;
        public readonly float dt;
        public readonly float dt2;
        public readonly float INVdt;
        public readonly float dt2_iter;
        public readonly float INViter;
        public readonly float dt_iter;

        public TimeStep(float deltaTime, float iterations)
        {
            iter = iterations;
            INViter = 1 / iter;
            dt = deltaTime;
            dt2 = deltaTime * deltaTime;
            INVdt = deltaTime == 0 ? 0 : 1 / deltaTime;
            dt_iter = dt * INViter;
            dt2_iter = dt2 * INVdt;
        }

    }
} 