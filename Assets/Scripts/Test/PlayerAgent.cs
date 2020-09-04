﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class PlayerAgent : Agent // <- 注意这里是Agent
{

    public float[] viewDegrees;
    public float maxDistance;

    Rigidbody rig;
    BehaviorParameters bp;

    Vector2 dir; //右摇杆 xy 方向
    float dirAngle; //右摇杆角度
    float joyForce; //右摇杆力度
    float moveSpeed = 10.0f;

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
    }

    private void FixedUpdate()
    {
        foreach (float degree in viewDegrees)
        {
            Vector3 direction = Quaternion.AngleAxis(degree, transform.up) * transform.forward;
            Debug.DrawRay(transform.position, direction * maxDistance, Color.white);
        }

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
            if (Physics.Raycast(transform.localPosition, direction, out RaycastHit info, maxDistance)) 
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

        rig.AddForce(moveVector * moveSpeed);

        //右摇杆角度计算相关
        dir = new Vector2(shootX, shootY);
        Vector2 v2 = (dir - new Vector2(0.0f, 0.0f)).normalized;
        float angle = Mathf.Atan2(v2.y, v2.x) * Mathf.Rad2Deg;
        angle = -angle + 90;
        if (angle < 0)
        {
            angle += 360;
        }
        Debug.Log("角度"+angle);
        //右摇杆角度计算相关结束
        joyForce = dir.magnitude;
        Debug.Log("力度" + joyForce);


        if (false)
        {
            SetReward(0.0f); // 设置奖励
            EndEpisode();  //结束当前周期
        }
    }

    public override void Heuristic(float[] actionsOut) // 手操
    {
        actionsOut[0] = Input.GetAxis("JoyL_Horizontal");
        actionsOut[1] = Input.GetAxis("JoyL_Vertical");

        actionsOut[2] = Input.GetAxis("JoyR_Horizontal");
        actionsOut[3] = Input.GetAxis("JoyR_Vertical");

  

    }


}
