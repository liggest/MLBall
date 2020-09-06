using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    public PlayerAgent owner;
    //public float rotateSpeed = 360;
    //public float rotateRadius = 1.8f;
    //public float rotateDegree = 0;
    public float safeRadius = 1.8f;
    public float ballDistance = 2;
    public float smoothTime = 0.05f;

    Rigidbody rig;
    //Rigidbody ownerRig;
    Vector3 smoothVelocity = Vector3.zero;

    Vector3 initPos = Vector3.zero;

    public PlayerAgent lastPlayer;

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
                //transform.RotateAround(owner.transform.localPosition, owner.transform.up, 100);
                RotateTo();
            }
            else
            {
                rig.MovePosition(Vector3.SmoothDamp(transform.localPosition, ownerFront, ref smoothVelocity, smoothTime, float.PositiveInfinity, Time.fixedDeltaTime));

            //RotateTo(rotateDegree, distance);
            }

        }

        if (transform.localPosition.y < -2)
        {
            InitBall();
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
        /*else if(other.CompareTag("Wall"))
        {
            ResetOwner();
        }*/
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!IsOwner(other.transform))
            {
                PlayerAgent target = other.GetComponent<PlayerAgent>();
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
        lastPlayer = owner;
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

    public bool IsOwner(Transform t)
    {
        if (owner && t.Equals(owner.transform))
        {
            return true;
        }
        return false;
    }

    public void RotateTo()
    {
        Vector3 ownerPos = owner.transform.localPosition;
        ownerPos.y = 0;
        Vector3 ballPos = transform.localPosition;
        ballPos.y = 0;
        Vector3 ownerToBall = ballPos - ownerPos;
        //Debug.DrawRay(owner.transform.localPosition, ownerToBall, Color.cyan);
        float degree = Vector3.SignedAngle(ownerToBall, owner.transform.forward, owner.transform.up);
        int degFactor = 1;
        if (degree < 0)
        {
            degFactor = -1;
            degree *= -1;
        }
        Debug.Log(degree);
        Vector3 ballY = transform.up * transform.localPosition.y;
        while (degree > 90)
        {
            degree -= 90;
            ownerToBall = Quaternion.AngleAxis(90 * degFactor, owner.transform.up) * ownerToBall;
            rig.MovePosition(ownerToBall + ownerPos + ballY);
        }
        if (degree > 0)
        {
            ownerToBall = Quaternion.AngleAxis(degree * degFactor, owner.transform.up) * ownerToBall;
            rig.MovePosition(ownerToBall + ownerPos + ballY);
        }
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
