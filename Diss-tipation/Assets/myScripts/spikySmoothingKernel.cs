using UnityEngine;
using System.Collections;

namespace fluidClasses
{
    public class spikySmoothingKernel : MonoBehaviour
    {
        protected float _kernelSize;
        protected float _kernelSizeSq;
        protected float _kernelSize6;
        protected float _minKernelSize;
        protected float _factor;

        public void initialise(float kernelSize)
        {
            _kernelSize = kernelSize;
            _minKernelSize = Vector3.kEpsilon;
            _kernelSizeSq = _kernelSize * _kernelSize;
            _kernelSize6 = _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize;
            _factor = (5f / (Mathf.PI * _kernelSize6));
        }

        public Vector3 CalculateGradient(ref Vector3 distance)
        {
            float lenSq = distance.sqrMagnitude;

            // if the distance between particles is bigger than the kernel
            if (lenSq > _kernelSizeSq)
                return Vector3.zero;

            if (lenSq < _minKernelSize)
                lenSq = _minKernelSize;

            float len = Mathf.Sqrt(lenSq);
            float f = -_factor * 3.0f * (_kernelSize - len) * (_kernelSize - len) / len;

            return new Vector3(distance.x * f, distance.y * f, distance.z * f);
        }
    }
}