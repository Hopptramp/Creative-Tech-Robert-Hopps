using UnityEngine;
using System.Collections;

namespace fluidClasses
{
    public class poly6SmoothingKernel : MonoBehaviour
    {
        protected float _kernelSize;
        protected float _kernelSizeSq;
        //protected float _kernelSize6;
        protected float _kernelSize9;
        protected float _minKernelSize;
        protected float _factor;

        public void initialise(float kernelSize)
        {
            _kernelSize = kernelSize;
            _minKernelSize = Vector3.kEpsilon;
            _kernelSizeSq = _kernelSize * _kernelSize;
            //_kernelSize6 = _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize;
            _kernelSize9 = _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize;
            _factor = (50.0f / (64.0f * Mathf.PI * _kernelSize9));
        }

        // calculates the effect of 1 particle on another for density
        public float CalculateDensity(ref Vector3 distance)
        {
            float lenSq = distance.sqrMagnitude;
            // if the particle is further than the smoothing distance -> return 0
            if (lenSq > _kernelSizeSq)
                return 0.0f;
            // if the distance is less than the min distance
            if (lenSq < _minKernelSize)
                lenSq = _minKernelSize;

            float diffSq = _kernelSizeSq - lenSq;
            float toReturn = _factor * diffSq * diffSq * diffSq;
            return toReturn;
        }
    }
}