using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Transform owner;
    public float rotateSpeed = 10;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            //transform.Rotate(owner.up + owner.position, rotateSpeed * Time.fixedDeltaTime, Space.Self);
            transform.RotateAround(owner.position, owner.up, rotateSpeed * Time.fixedDeltaTime);
            Debug.Log($"{ owner.position},{owner.up}");

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        
    }
}
