using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(transform.position.y < -19f)
        {
            transform.position = new Vector3(transform.position.x, -19f, transform.position.z);
        }
    }
    private void FixedUpdate()
    {
        if (transform.position.y < -19f)
        {
            transform.position = new Vector3(transform.position.x, -19f, transform.position.z);
        }
    }
}
