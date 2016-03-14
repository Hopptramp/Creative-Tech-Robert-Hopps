using UnityEngine;
using System.Collections;

public class cameraControl : MonoBehaviour {

    public GameObject cameraTarget = null;

	// Use this for initialization
	void Start ()
    {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        Vector3 targetForward = cameraTarget.transform.up;
        Vector3 targetUp = cameraTarget.transform.forward;

        transform.rotation = Quaternion.LookRotation(targetForward, targetUp);

        transform.LookAt(cameraTarget.transform);
        gameObject.transform.position = cameraTarget.transform.position + new Vector3(-10f, 5f, 0f);
	}
}
