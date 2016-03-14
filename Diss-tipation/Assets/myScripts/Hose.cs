using UnityEngine;
using System.Collections;

namespace fluidClasses
{

    public class Hose : MonoBehaviour
    {
        public FluidMain fluid = null;
        Emitter emitter = null;
        Rigidbody rb = null;

        [SerializeField] float particleVelocity = 10;

        // Use this for initialization
        void Start()
        {
            emitter = fluid.GetComponentInChildren<Emitter>();
            rb = GetComponent<Rigidbody>();
        }

        // Update is called once per frame
        void Update()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            float rotateHorizontal = Input.GetAxis("rightHorizontal");
            float rotateVertical = Input.GetAxis("rightVertical");

            Vector3 rotate = new Vector3(0, rotateVertical, 0) / 2;
            Vector3 movement = (new Vector3(horizontal, 0.0f, vertical))/2;

            transform.Rotate(rotate);
            transform.Translate(movement, Space.Self);

            Vector3 distance = transform.position + transform.forward * particleVelocity;
            Vector3 origin = transform.position + transform.forward * 3;

            if (Input.GetButton("Fire1"))
            {
                
                emitter.position = origin;
                emitter.endPosition =  distance;
                emitter.emit = true;
            }
            if (Input.GetButtonUp("Fire1"))
            {
                emitter.emit = false;
            }
        }
    }
}