using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class PlayerAgent : Agent // <- 注意这里是Agent
{
    [Tooltip("是否可以通过输入控制（手操）")]
    public bool isControl = false;

    //public Ball ball;
    [Tooltip("是否画射线")]
    public bool drawRays = false;
    [Tooltip("各个射线的角度")]
    public float[] viewDegrees;
    [Tooltip("射线视野距离")]
    public float maxViewDistance = 10;

    private Rigidbody rig;
    private BehaviorParameters bp;
    public StageManager sm;
    public Ball currentBall;

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

    float oldKeepBallTime = 0; // 曾持球时间
    float startKeepBall = 0; // 开始持球时间
    Vector3 lastPos = Vector3.zero;
    private float keepBallDistance = 0; // （曾）持球距离

    bool isUnbreakable=false;
    int unbreakbaleNum = 20;

    public int TeamID { get => teamID; private set => teamID = value; }
    public string TeamName { get => teamName; private set => teamName = value; }
    public Rigidbody Rig { get => rig; private set => rig = value; }
    public BehaviorParameters Bp { get => bp; private  set => bp = value; }
    public bool IsKeepingBall { get => currentBall != null && currentBall.IsOwner(this); }

    public float KeepBallTime {
        get {
            if (IsKeepingBall)
            {
                return sm.timer - startKeepBall;
            }
            return oldKeepBallTime;
        }
    }

    public float KeepBallDistance { get => keepBallDistance; }
    public bool IsUnbreakable { get => isUnbreakable; private  set => isUnbreakable = value; }

    private void Awake()
    {
        Bp = GetComponent<BehaviorParameters>();

        int defaultSize = Bp.BrainParameters.VectorObservationSize;
        int actualSize = 5 + 4 * viewDegrees.Length;
        if (defaultSize != actualSize)
        {
            Bp.BrainParameters.VectorObservationSize = actualSize;
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
        if (drawRays)
        {
            foreach (float degree in viewDegrees)
            {
                Vector3 direction = Quaternion.AngleAxis(degree, transform.up) * transform.forward;
                Debug.DrawRay(transform.position, direction * maxViewDistance, Color.white);
            }
            //Vector3 dirDirection = Quaternion.AngleAxis(dirAngle, transform.up) * transform.forward;
            Debug.DrawRay(transform.position, transform.forward * joyForce * joyForceFactor, Color.red);
        }
    }

    public override void OnEpisodeBegin()  // 每个周期开始时 重置场景
    {
        InitPlayer();
        sm.InitBalls();
        //foreach(Ball b in sm.balls){
        //    b.InitBall();
        //}
    }

    public override void CollectObservations(VectorSensor sensor) // 向网络提供数据
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(Rig.velocity.x);
        sensor.AddObservation(Rig.velocity.z);
        //可能需要再提供角度
        //sensor.AddObservation(transform.localRotation);

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
                    // Debug.Log($"{hitPos},{hitType}" );
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
                //Debug.Log(info.collider.tag);
                ObservationReward(hitType, hitPos);
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

        if (IsKeepingBall)
        {
            keepBallDistance += Vector3.Distance(transform.localPosition, lastPos);
            //Debug.Log($"{KeepBallDistance},{KeepBallTime}");

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
            if (shoot >= 0.5)
            {
                //sm.ball.Shoot(joyForce * joyForceFactor);
                currentBall.Shoot(joyForce * joyForceFactor);
                ShootReward();
            }
        }
        else
        {
            joyForce = 0;
            IdleReward();
        }

        if (transform.localPosition.y < -2)
        {
            FallReward();
            InitPlayer();
        }

        lastPos = transform.localPosition;

        /*
        if (false)
        {
            SetReward(0.0f); // 设置奖励
            EndEpisode();  //结束当前周期
        }
        */
    }

    public override void Heuristic(float[] actionsOut) // 手操
    {
        if (!isControl)
        {
            return;
        }

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
        startKeepBall = sm.timer;
        keepBallDistance = 0;
        GetBallReward();
    }

    public void ResetBall()
    {
        oldKeepBallTime = KeepBallTime;
        Ball b = currentBall;
        currentBall = null;
        LoseBallReward(b);
    }

    public void InitPlayer()
    {
        ResetBall();
        Rig.velocity = Vector3.zero;
        transform.localPosition = initPos;
        transform.localRotation = iniRotation;
    }

    public IEnumerator Unbreakable(){
        isUnbreakable = true;
        for (int i = 0; i < unbreakbaleNum; i++)
        {
            yield return new WaitForFixedUpdate();
        }
        isUnbreakable = false;
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

    /// <summary>
    /// 球进门得到奖励
    /// </summary>
    /// <param name="g">进的球门</param>
    /// <param name="b">进球门的球</param>
    public virtual void GoalReward(Goal g,Ball b)
    {
        //g.IsRivalGoal
        //b.lastPlayer
        //SetReward
    }

    /// <summary>
    /// Agent得到球的奖励
    /// </summary>
    public virtual void GetBallReward()
    {
        //currentBall.lastPlayer
        //sm.SetTeamReward
    }

    /// <summary>
    /// Agent持球奖励
    /// </summary>
    public virtual void KeepBallReward()
    {
        //currentBall
    }

    /// <summary>
    /// Agent丢球奖励
    /// </summary>
    /// <param name="b">丢的球</param>
    public virtual void LoseBallReward(Ball b)
    {

    }

    /// <summary>
    /// Agent射门奖励
    /// </summary>
    public virtual void ShootReward()
    {
        //joyForce
    }

    /// <summary>
    /// Agent撞墙奖励
    /// </summary>
    public virtual void BumpWallReward()
    {

    }

    /// <summary>
    /// Agent掉下Stage的奖励
    /// </summary>
    public virtual void FallReward()
    {

    }

    /// <summary>
    /// Agent闲逛（未持球状态）的奖励
    /// </summary>
    public virtual void IdleReward()
    {

    }

    /// <summary>
    /// 射线观察到物体的奖励
    /// </summary>
    /// <param name="observeType">观察到物体的类型 0：墙 1：球 2：队友 -2：对手 3：己方球门 -3：对方球门</param>
    /// <param name="observePos">观察到物体的位置</param>
    public virtual void ObservationReward(int observeType,Vector3 observePos)
    {

    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            BumpWallReward();
        }
    }

}
