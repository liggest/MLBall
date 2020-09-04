using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public Transform owner;
    public float rotateSpeed = 360;
    public float rotateRadius = 1.8f;
    public float rotateDegree = 0;
    public float safeRadius = 0.96f;

    public float smoothTime = 0.3f;
    Rigidbody rig;
    Rigidbody ownerRig;
    Vector3 smoothVelocity = Vector3.zero;



    // Start is called before the first frame update
    void Start()
    {
        rotateRadius = GetComponents<SphereCollider>()[1].radius + 0.6f;
        //Debug.Log(rotateRadius);
        rig = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            Vector3 onwerFront = owner.localPosition + owner.forward;
            float distance = Vector3.Distance(transform.localPosition, onwerFront);
            if (distance > rotateRadius)
            {
                ResetOwner();
                //Debug.Log(distance);
                //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, onwerFront, ref smoothVelocity, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime);
            }
            else
            {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, onwerFront, ref smoothVelocity, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime);

                //RotateTo(rotateDegree, distance);
            }
            //transform.RotateAround(owner.localPosition, owner.up, rotateSpeed * Time.fixedDeltaTime);

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!IsOwner(other.transform))
            {
                SetOwner(other.transform);
            }
        }
    }

    public void SetOwner(Transform t)
    {
        owner = t;
        ownerRig = owner.GetComponent<Rigidbody>();
        Debug.Log("Owner了");
    }

    public void ResetOwner()
    {
        owner = null;
        ownerRig = null;
        Debug.Log("Reset了");
    }
    public bool IsOwner(Transform t)
    {
        if (t.Equals(owner)) 
        {
            return true;
        }
        return false;
    }

    public void RotateTo(float degree, float distance = -1)
    {
        Vector3 ballpos = Quaternion.AngleAxis(degree, owner.up) * owner.forward;
        if (distance < 0)
        {
            distance = Vector3.Distance(transform.localPosition, owner.localPosition);
        }
        transform.localPosition = owner.localPosition + ballpos * distance;
    }

    public void Shoot(float power)
    {
        Vector3 force = transform.localPosition - owner.localPosition;
        force = force.normalized * power;
        Debug.Log(force);
        rig.AddForce(force);
        ownerRig.AddForce(force);
        ResetOwner();
        //Debug.Log("射门！");
    }
}
