using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Transform owner;
    public float rotateSpeed = 360;
    public float rotateRadius = 1.8f;

    public float smoothTime = 0.3f;
    Rigidbody rig;
    Vector3 smoothVelocity = Vector3.zero;



    // Start is called before the first frame update
    void Start()
    {
        rotateRadius = GetComponents<SphereCollider>()[1].radius + 0.6f;
        Debug.Log(rotateRadius);
        rig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            float distance = Vector3.Distance(transform.localPosition, owner.localPosition);
            if (distance > rotateRadius)
            {
                Debug.Log(distance);
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, owner.localPosition, ref smoothVelocity, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime);
            }
            else
            {
                transform.RotateAround(owner.localPosition, owner.up, rotateSpeed * Time.fixedDeltaTime);
            }
            //transform.Rotate(owner.up + owner.position, rotateSpeed * Time.fixedDeltaTime, Space.Self);
            

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!IsOwner(other.transform))
            {
                owner = other.transform;
            }
        }
    }

    public bool IsOwner(Transform self)
    {
        if( self.Equals(owner))
        {
            return true;
        }
        return false;
    }

    public void Shoot(float degree,float power)
    {
        Debug.DrawRay(owner.transform.position, Quaternion.AngleAxis(degree, owner.transform.up) * owner.transform.forward, Color.red, 1.5f);

    }
}
