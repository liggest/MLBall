using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

[System.Serializable]
public class ViewRay {
    [Tooltip("射线角度"), SerializeField]
    public float degree;
    [Tooltip("是否使用Agent上的射线设置，会覆盖下面的设置（除了射线角度）")]
    public bool useAgentSettings = true;
    [Tooltip("射线高度"), SerializeField]
    public float height = 0.5f;
    [Tooltip("射线球半径，为0时不使用球射线"), SerializeField]
    public float radius = 0;
    [Tooltip("射线发射距离"), SerializeField]
    public float distance = 10;
    [Tooltip("不向神经网络提交射线击中的位置，而是提交击中时射线的距离")]
    public bool feedDistance = false;
    [HideInInspector]
    public Vector3 hitPos = -Vector3.one;
}

public class PlayerAgent : Agent // <- 注意这里是Agent
{
    [Tooltip("是否可以通过输入控制（手操）")]
    public bool isControl = false;

    //public Ball ball;
    [Tooltip("是否画射线")]
    public bool drawRays = false;
    [Tooltip("各个射线")]
    public ViewRay[] viewRays = new ViewRay[1];
    public float rayHeight = 0.5f;
    [Tooltip("射线球半径")]
    public float raySphereRadius = 0;
    //[Tooltip("各个射线的角度")]
    //public float[] viewDegrees;
    [Tooltip("射线视野距离")]
    public float maxViewDistance = 10;
    [Tooltip("不向神经网络提交射线击中的位置，而是提交击中时射线的距离")]
    public bool rayFeedDistance = false;
    float defaultHitType = -1;
    Vector3 defaultHitPos = -Vector3.one;

    [HideInInspector, Tooltip("HitType的最大值")]
    public float maxHitType = 5;
    float maxHitTypeFactor = 0;

    [Tooltip("是否向神经网络提交本地位置、角度、速度等信息")]
    public bool feedLocalInfo = true;

    private Rigidbody rig;
    private BehaviorParameters bp;
    private StageManager sm;
    private Ball currentBall;

    //Vector2 dir; //右摇杆 xy 方向
    //float dirAngle = 0; //右摇杆角度
    float joyForce = 0; //右摇杆力度
    [Tooltip("射出力度的系数")]
    public float joyForceFactor = 10;
    [Tooltip("移动速度")]
    public float moveSpeed = 10.0f;

    [HideInInspector,Tooltip("归一化用的最大速度，并非实际能达到的最大速度")]
    public float maxSpeed = 25;
    float maxSpeedFactor = 0;


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

    private bool isUnbreakable=false;
    int unbreakbaleNum = 20;

    public int TeamID { get => teamID; private set => teamID = value; }
    public string TeamName { get => teamName; private set => teamName = value; }
    public Rigidbody Rig { get => rig; private set => rig = value; }
    public BehaviorParameters BP { get => bp; private  set => bp = value; }
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
    public StageManager SM { get => sm; private  set => sm = value; }
    public Ball CurrentBall { get => currentBall; private set => currentBall = value; }
    public Vector3 LastPos { get => lastPos; private set => lastPos = value; }

    private void Awake()
    {
        bp = GetComponent<BehaviorParameters>();
        InitRays();

        int defaultSize = bp.BrainParameters.VectorObservationSize;
        //int actualSize = 6 + 4 * viewDegrees.Length;
        int actualSize = 0;
        if (feedLocalInfo)
        {
            actualSize+=6;
        }
        foreach (ViewRay vray in viewRays)
        {
            if (vray.feedDistance)
            {
                actualSize += 2;
            }
            else
            {
                actualSize += 4;
            }
        }
        if (defaultSize != actualSize)
        {
            bp.BrainParameters.VectorObservationSize = actualSize;
            Debug.Log($"正在观测的参数数量与设置中不符，已改正：{defaultSize} -> {actualSize}");
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

        maxSpeedFactor = 1.0f / maxSpeed;
        maxHitTypeFactor = 1.0f / maxHitType;
    }


    private void FixedUpdate()
    {
        if (drawRays)
        {
            Debug.DrawRay(transform.position, transform.forward * joyForce * joyForceFactor, Color.red);
        }

        /*if (Physics.Raycast(transform.localPosition, transform.forward, out RaycastHit info1, maxViewDistance))
        {
            if (isControl)
            {
                Debug.Log(info1.transform);
            }
        }*/
    }

    public override void OnEpisodeBegin()  // 每个周期开始时 重置场景
    {
        InitPlayer();
        sm.InitBalls();
        sm.InitTimer();
        //foreach(Ball b in sm.balls){
        //    b.InitBall();
        //}
    }

    public override void CollectObservations(VectorSensor sensor) // 向网络提供数据
    {
        if (feedLocalInfo)
        {
            // 这些都需要归一化到[-1,1]
            sensor.AddObservation(sm.NormalizePos(transform.localPosition));
            sensor.AddObservation(Rig.velocity.x * maxSpeedFactor);
            sensor.AddObservation(Rig.velocity.z * maxSpeedFactor);
            sensor.AddObservation(sm.NormalizeAngleY(transform.localEulerAngles));
            //float temp = sm.NormalizeAngleY(transform.localEulerAngles);
        }
        //foreach (float degree in viewDegrees)
        foreach (ViewRay vray in viewRays)
        {
            int hitType = (int)defaultHitType;
            Vector3 hitPos = defaultHitPos;
            float hitDistance = -1;
            Vector3 direction = Quaternion.AngleAxis(vray.degree, transform.up) * transform.forward;
            Vector3 playerPos = transform.position;
            playerPos.y = vray.height;
            //playerPos.y = 0; <- 基本是罪魁祸首
            //Debug.Log(direction);
            bool ishit;
            RaycastHit info;
            if (vray.radius > 0)
            {
                ishit = Physics.SphereCast(playerPos, vray.radius, direction, out info, vray.distance);
            }
            else
            {
                ishit = Physics.Raycast(playerPos, direction, out info, vray.distance);
            }
            //if (Physics.Raycast(playerPos, direction, out RaycastHit info, maxViewDistance))
            if(ishit)
            {
                /*
                if (isControl)
                {
                    Debug.Log(info.transform);
                }
                */
                if (info.collider.CompareTag("Agent"))
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
                    //Debug.Log("执行了");
                    //Debug.Log($"Player{IsTeammate(target)}");
                    //Debug.Log(info.collider.tag);
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
                hitPos = info.transform.parent.InverseTransformPoint(info.point);
                hitDistance = info.distance;
                //if (isControl)
                //{
                //    Debug.Log($"{hitType},{info.point},{hitPos}");
                //}
                ObservationReward(hitType, hitPos, hitDistance);
                //检测到的话
                vray.hitPos = info.point;// 画球用的
                sensor.AddObservation(hitType * maxHitTypeFactor); //这里是 hitType/maxHitType
                if (vray.feedDistance)
                {
                    sensor.AddObservation(hitDistance / vray.distance); //这里是归一化后的 hitDistance
                }
                else
                {
                    sensor.AddObservation(sm.NormalizePos(hitPos)); //这里是归一化后的 hitPos
                }
            }
            else
            {
                vray.hitPos = hitPos;
                // 没检测到的话
                sensor.AddObservation(hitType); // 这里是 -1
                if (vray.feedDistance)
                {
                    sensor.AddObservation(hitDistance); //这里是 -1
                }
                else
                {
                    sensor.AddObservation(hitPos); // 这里是 (-1,-1,-1)
                }
            }
            
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
            KeepBallReward();
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
            if (shoot >= 0)
            {
                //sm.ball.Shoot(joyForce * joyForceFactor);
                float forceValue = joyForce * joyForceFactor;
                currentBall.Shoot(forceValue);
                ShootReward(forceValue);
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

        actionsOut[4] = (Input.GetAxis("Fire2") - 0.5f) * 2;
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

    public void InitRays()
    {
        foreach (ViewRay viewRay in viewRays)
        {
            if (viewRay.useAgentSettings)
            {
                viewRay.distance = maxViewDistance;
                viewRay.height = rayHeight;
                viewRay.radius = raySphereRadius;
                viewRay.feedDistance = rayFeedDistance;
            }
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

    #region 各种奖励函数
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
    /// 射出球的奖励
    /// </summary>
    /// <param name="forceValue">射出球的力道</param>
    public virtual void ShootReward(float forceValue)
    {
        //forceValue
    }

    /// <summary>
    /// Agent撞墙奖励
    /// </summary>
    public virtual void BumpWallReward()
    {
      
    }
    /// <summary>
    /// 撞人奖励
    /// </summary>
    /// <param name="playerTransform">撞到的人的transform</param>
    public virtual void BumpPlayerReward(Transform playerTransform)
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
    /// 射线观察到物体的奖励 observeType 0：墙 1：球 2：队友 -2：对手 3：己方球门 -3：对方球门
    /// </summary>
    /// <param name="observeType">观察到物体的类型 0：墙 1：球 2：队友 -2：对手 3：己方球门 -3：对方球门</param>
    /// <param name="observePos">观察到物体的位置</param>
    public virtual void ObservationReward(int observeType,Vector3 observePos,float distance)
    {

    }
    #endregion
    private void OnCollisionStay(Collision collision)
    {
        if (collision.collider.CompareTag("Wall"))
        {
            BumpWallReward();
        }
        else if (collision.collider.CompareTag("Agent"))
        {
            BumpPlayerReward(collision.transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (drawRays)
        {
            foreach (ViewRay vray in viewRays)
            {
                Vector3 direction = Quaternion.AngleAxis(vray.degree, transform.up) * transform.forward;
                Vector3 pos = transform.position;
                pos.y = vray.height;
                Gizmos.DrawRay(pos, direction * vray.distance);
                if (vray.radius > 0 && vray.hitPos.y > -1)
                {
                    Gizmos.DrawWireSphere(vray.hitPos, vray.radius);
                    vray.hitPos = -Vector3.one;
                }
            }
        }
    }

}
