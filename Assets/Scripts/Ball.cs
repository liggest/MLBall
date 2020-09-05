using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public PlayerAgent owner;
    //public float rotateSpeed = 360;
    //public float rotateRadius = 1.8f;
    //public float rotateDegree = 0;
    public float safeRadius = 2.8f;
    public float ballDistance = 2;
    public float smoothTime = 0.05f;

    Rigidbody rig;
    //Rigidbody ownerRig;
    Vector3 smoothVelocity = Vector3.zero;

    Vector3 initPos = Vector3.zero;

    // Start is called before the first frame update
    void Start()
    {
        //rotateRadius = GetComponents<SphereCollider>()[1].radius + 0.6f;
        //Debug.Log(rotateRadius);
        rig = GetComponent<Rigidbody>();
        initPos = transform.localPosition;
        Utils.GetStage(transform).balls.Add(this);
    }

    private void FixedUpdate()
    {
        if (owner)
        {
            Vector3 ownerFront = owner.transform.localPosition + owner.transform.forward * ballDistance;
            ownerFront.y = 0;
            float distance = Vector3.Distance(transform.localPosition, ownerFront);
            if (distance > safeRadius)
            {
                //ResetOwner();
                //Debug.Log(distance);
                //transform.localPosition = Vector3.SmoothDamp(transform.localPosition, onwerFront, ref smoothVelocity, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime);
                transform.RotateAround(owner.transform.localPosition, owner.transform.up, 100);
            }
            else
            {
                transform.localPosition = Vector3.SmoothDamp(transform.localPosition, ownerFront, ref smoothVelocity, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime);

            //RotateTo(rotateDegree, distance);
            }

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerAgent target = other.GetComponent<PlayerAgent>();
            if (!IsOwner(target))
            {
                SetOwner(target);
            }
        }
    }

    public void SetOwner(PlayerAgent pa)
    {
        owner = pa;
        pa.SetBall(this);
        //ownerRig = owner.GetComponent<Rigidbody>();
        Debug.Log("Owner了");
    }

    public void ResetOwner()
    {
        if (owner)
        {
            owner.ResetBall();
        }
        owner = null;
        //ownerRig = null;
        Debug.Log("Reset了");
    }
    public bool IsOwner(PlayerAgent pa)
    {
        if (pa.Equals(owner))
        {
            return true;
        }
        return false;
    }

    public void RotateTo(float degree, float distance = -1)
    {
        Vector3 ballpos = Quaternion.AngleAxis(degree, owner.transform.up) * owner.transform.forward;
        if (distance < 0)
        {
            distance = Vector3.Distance(transform.localPosition, owner.transform.localPosition);
        }
        transform.localPosition = owner.transform.localPosition + ballpos * distance;
    }

    public void Shoot(float power)
    {
        Vector3 force = transform.localPosition - owner.transform.localPosition;
        force.y = 0;
        force = force.normalized * power;
        Debug.Log(force);
        rig.AddForce(force,ForceMode.Impulse);
        //owner.rig.AddForce(-force*0.65f, ForceMode.Impulse);
        owner.rig.AddForce(-force, ForceMode.Impulse);
        ResetOwner();
        //Debug.Log("射门！");
    }

    public void InitBall()
    {
        ResetOwner();
        rig.velocity = Vector3.zero;
        transform.localPosition = initPos;
    }
}
