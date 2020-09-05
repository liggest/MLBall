using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class PlayerAgent : Agent // <- 注意这里是Agent
{
    //public Ball ball;
    public float[] viewDegrees;
    public float maxViewDistance = 10;

    public Rigidbody rig;
    BehaviorParameters bp;
    StageManager sm;
    Ball currentBall;

    Vector2 dir; //右摇杆 xy 方向
    //float dirAngle = 0; //右摇杆角度
    float joyForce = 0; //右摇杆力度
    public float joyForceFactor = 10; // 力度的系数
    public float moveSpeed = 10.0f;

    Vector3 initPos = Vector3.zero;

    private void Awake()
    {
        bp = GetComponent<BehaviorParameters>();

        int defaultSize = bp.BrainParameters.VectorObservationSize;
        int actualSize = 5 + 4 * viewDegrees.Length;
        if (defaultSize != actualSize)
        {
            bp.BrainParameters.VectorObservationSize = actualSize;
            Debug.Log("正在观测的参数数量与设置中不符，已改正");
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        rig = GetComponent<Rigidbody>();
        initPos = transform.localPosition;
        sm = Utils.GetStage(transform);
    }

    private void FixedUpdate()
    {
        foreach (float degree in viewDegrees)
        {
            Vector3 direction = Quaternion.AngleAxis(degree, transform.up) * transform.forward;
            Debug.DrawRay(transform.position, direction * maxViewDistance, Color.white);
        }
        //Vector3 dirDirection = Quaternion.AngleAxis(dirAngle, transform.up) * transform.forward;
        Debug.DrawRay(transform.position, transform.forward * joyForce * joyForceFactor, Color.red);
    }

    public override void OnEpisodeBegin()  // 每个周期开始时 重置场景
    {
            
    }

    public override void CollectObservations(VectorSensor sensor) // 向网络提供数据
    {
        sensor.AddObservation(transform.position);
        sensor.AddObservation(rig.velocity.x);
        sensor.AddObservation(rig.velocity.z);

        foreach (float degree in viewDegrees)
        {
            
            int hitType = -1;
            Vector3 hitPos = Vector3.zero;
            Vector3 direction = Quaternion.AngleAxis(degree, transform.up) * transform.forward;
            if (Physics.Raycast(transform.localPosition, direction, out RaycastHit info, maxViewDistance)) 
            {   
                if (info.collider.CompareTag("Player"))
                {
                    hitType = 1;
                }
                else if (info.collider.CompareTag("Ball"))
                {
                    hitType = 2;
                }
                else if (info.collider.CompareTag("Wall"))
                {
                    hitType = 0;
                }
                hitPos = info.transform.InverseTransformPoint(info.point);
            }
            sensor.AddObservation(hitType);
            sensor.AddObservation(hitPos);
        }

    }

    public override void OnActionReceived(float[] vectorAction) // 得到网络输出 指定权重
    {
        float moveX = vectorAction[0];
        float moveY = vectorAction[1];
        Vector3 moveVector = new Vector3(moveX, 0, moveY);

        float shootX = vectorAction[2];
        float shootY = vectorAction[3];

        float shoot = vectorAction[4];

        rig.AddForce(moveVector * moveSpeed);

        Vector3 rDir = new Vector3(shootX, 0, shootY);
        if (rDir != Vector3.zero)
        {
            Vector3 nDir = rDir.normalized;
            transform.forward = nDir;
        }

        if (currentBall && currentBall.IsOwner(this))
        {
            #region 右摇杆角度计算相关
            /*
            dir = new Vector2(shootX, shootY);
            Vector2 v2 = (dir - new Vector2(0.0f, 0.0f)).normalized;
            float angle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
            if (angle != 0){
                angle = -angle + 90;
                if (angle < 0)
                {
                    angle += 360;
                }
            }
            
            //Debug.Log("角度" + angle);
            dirAngle = angle;
            */

            //ball.rotateDegree = dirAngle;
            joyForce = rDir.magnitude;
            //Debug.Log("力度" + joyForce);
            #endregion
            if (shoot == 1)
            {
                //sm.ball.Shoot(joyForce * joyForceFactor);
                currentBall.Shoot(joyForce * joyForceFactor);
            }
        }
        else
        {
            joyForce = 0;
        }

        if (transform.localPosition.y < -2)
        {
            InitPlayer();
        }


        if (false)
        {
            SetReward(0.0f); // 设置奖励
            EndEpisode();  //结束当前周期
        }
    }

    public override void Heuristic(float[] actionsOut) // 手操
    {
        string[] JoyName = Input.GetJoystickNames();

        actionsOut[0] = Input.GetAxis("JoyL_Horizontal");
        actionsOut[1] = Input.GetAxis("JoyL_Vertical");

        actionsOut[2] = Input.GetAxis("JoyR_Horizontal");
        actionsOut[3] = Input.GetAxis("JoyR_Vertical");

        if(JoyName.Length > 0 && JoyName[0].Length == 0)
        {
            //鼠标控制
            Vector3 v3 = Camera.main.WorldToScreenPoint(transform.position);
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = v3.z;
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
            actionsOut[2] = (worldPos.x - transform.localPosition.x) * 0.1f;
            actionsOut[3] = (worldPos.z - transform.localPosition.z) * 0.1f;
        }

        actionsOut[4] = Input.GetAxis("Fire2");
    }

    public void SetBall(Ball b)
    {
        currentBall = b;
    }

    public void ResetBall()
    {
        currentBall = null;
    }

    public void InitPlayer()
    {
        ResetBall();
        rig.velocity = Vector3.zero;
        transform.position = initPos;
    }
}
