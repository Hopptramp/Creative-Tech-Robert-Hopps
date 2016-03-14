using UnityEngine;
using System.Collections;

namespace fluidClasses
{
    public class viscositySmoothingKernel : MonoBehaviour
    {
        protected float _kernelSize;
        protected float _kernelSizeSq;
        protected float _kernelSize3;
        //protected float _kernelSize6;
        protected float _kernelSize9;
        protected float _minKernelSize;
        protected float _factor;

        public void initialise(float kernelSize)
        {
            _kernelSize = kernelSize;
            _minKernelSize = Vector3.kEpsilon;
            _kernelSizeSq = _kernelSize * _kernelSize;
            _kernelSize3 = _kernelSize * _kernelSize * _kernelSize;
            //_kernelSize6 = _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize;
            _kernelSize9 = _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize * _kernelSize;
            _factor = (15.0f / (2.0f * Mathf.PI * _kernelSize3));
        }

        public float calculateLaplacian(ref Vector3 distance)
        {
            float lenSq = distance.sqrMagnitude;
            if(lenSq > _kernelSizeSq)
            {
                return 0.0f;
            }
            if(lenSq < _minKernelSize)
            {
                lenSq = _minKernelSize;
            }
            float len = Mathf.Sqrt(lenSq);
            return _factor * (6.0f / _kernelSize3) * (_kernelSize - len);
        }
    }
}