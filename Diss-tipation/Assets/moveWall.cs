using UnityEngine;
using System.Collections;

public class moveWall : MonoBehaviour {
float difference = 0.1f;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update ()
    {
        float xPosition = gameObject.transform.position.x;
        if (Input.GetKey(KeyCode.B))
        {
            xPosition -= difference;
            gameObject.transform.position = new Vector3(xPosition, gameObject.transform.position.y, gameObject.transform.position.z);
        }

        if (Input.GetKey(KeyCode.V))
        {
            xPosition += difference;
            gameObject.transform.position = new Vector3(xPosition, gameObject.transform.position.y, gameObject.transform.position.z);
        }
    }
}
