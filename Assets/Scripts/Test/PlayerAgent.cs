using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class PlayerAgent : Agent // <- 注意这里是Agent
{
    //public Ball ball;
    [Tooltip("各个射线的角度")]
    public float[] viewDegrees;
    [Tooltip("射线视野距离")]
    public float maxViewDistance = 10;

    private Rigidbody rig;
    BehaviorParameters bp;
    StageManager sm;
    Ball currentBall;

    //Vector2 dir; //右摇杆 xy 方向
    //float dirAngle = 0; //右摇杆角度
    float joyForce = 0; //右摇杆力度
    [Tooltip("射出力度的系数")]
    public float joyForceFactor = 10;
    [Tooltip("移动速度")]
    public float moveSpeed = 10.0f;

    [Tooltip("队伍的球门")]
    public Goal teamGoal;
    private int teamID = -1;
    private string teamName = "煤";
    Color teamColor = Color.black;
    Color teamHatColor = Color.gray;
    MeshRenderer mr;
    MeshRenderer hatMR;

    Vector3 initPos = Vector3.zero;
    Quaternion iniRotation = Quaternion.identity;

    public int TeamID { get => teamID; private set => teamID = value; }
    public string TeamName { get => teamName; private set => teamName = value; }
    public Rigidbody Rig { get => rig;private set => rig = value; }

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
        Rig = GetComponent<Rigidbody>();

        sm = Utils.GetStage(transform);

        TeamID = teamGoal.teamID;
        TeamName = GlobalManager.instance.GetTeamName(TeamID);
        teamColor = GlobalManager.instance.GetTeamColor(TeamID);
        teamHatColor = GlobalManager.instance.GetTeamColor(TeamID, true);
        mr = GetComponent<MeshRenderer>();
        hatMR = transform.GetChild(0).GetComponent<MeshRenderer>();
        mr.material.color = teamColor;
        hatMR.material.color = teamHatColor;
        AddTeam();

        initPos = transform.localPosition;
        iniRotation = transform.localRotation;
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
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Rig.velocity.x);
        sensor.AddObservation(Rig.velocity.z);

        foreach (float degree in viewDegrees)
        {
            
            int hitType = -1;
            Vector3 hitPos = Vector3.zero;
            Vector3 direction = Quaternion.AngleAxis(degree, transform.up) * transform.forward;
            Vector3 playerPos = transform.localPosition;
            playerPos.y = 0;
            if (Physics.Raycast(playerPos, direction, out RaycastHit info, maxViewDistance))  
            {   
                if (info.collider.CompareTag("Player"))
                {
                    PlayerAgent target = info.collider.GetComponent<PlayerAgent>();
                    if (IsTeammate(target))
                    {
                        hitType = 2;
                    }
                    else
                    {
                        hitType = -2;
                    }
                    
                }
                else if (info.collider.CompareTag("Ball"))
                {
                    hitType = 1;
                }
                else if (info.collider.CompareTag("Wall"))
                {
                    hitType = 0;
                }
                else if (info.collider.CompareTag("Goal"))
                {
                    Goal goal = info.collider.GetComponent<Goal>();
                    if (IsTeamGoal(goal))
                    {
                        hitType = 3;
                    }
                    else
                    {
                        hitType = -3;
                    }
                }
                hitPos = info.transform.InverseTransformPoint(info.point);
                Debug.Log(info.collider.tag);
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

        Rig.AddForce(moveVector * moveSpeed);

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

        if(JoyName.Length == 0 || JoyName[0].Length == 0)
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
        Rig.velocity = Vector3.zero;
        transform.localPosition = initPos;
        transform.localRotation = iniRotation;
    }

    public void AddTeam()
    {
        if(sm.teams.TryGetValue(TeamName,out List<PlayerAgent> teamList))
        {
            teamList.Add(this);
        }
        else
        {
            teamList = new List<PlayerAgent>();
            teamList.Add(this);
            sm.teams.Add(TeamName, teamList);
        }
    }
    /// <summary>
    /// 判断目标Agent是否为当前Agent的队友
    /// </summary>
    /// <param name="pa">目标Agent</param>
    /// <returns>目标Agent是否为当前Agent的队友</returns>
    public bool IsTeammate(PlayerAgent pa)
    {
        return sm.teams[TeamName].Contains(pa);
    }

    /// <summary>
    /// 判断目标球门是否为当前Agent队伍的球门
    /// </summary>
    /// <param name="g">目标球门</param>
    /// <returns>目标球门是否为当前Agent队伍的球门</returns>
    public bool IsTeamGoal(Goal g)
    {
        return teamGoal.Equals(g);
    }
}
