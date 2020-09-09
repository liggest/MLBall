using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public List<Ball> balls = new List<Ball>(); //场地中的球
    public Dictionary<string, List<PlayerAgent>> teams = new Dictionary<string, List<PlayerAgent>>(); //场地中的各个队伍
    public Dictionary<string, Goal> teamGoals = new Dictionary<string, Goal>(); //各个队伍的球门

    [Tooltip("场地对角线长度")]
    public float maxStageLength = 20;
    float stageDiagonalFactor = 0;

    [HideInInspector]
    public float timer = 0; //计时器
    [HideInInspector]
    public int episodes = 0; //场地周期数

    private void Awake()
    {
        Utils.SetStage(this);
        stageDiagonalFactor = 1.0f / maxStageLength;
    }

    private void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;
    }
    /// <summary>
    /// 将所有球初始化/重置
    /// </summary>
    public void InitBalls()
    {
        foreach(Ball b in balls)
        {
            b.InitBall();
        }
    }
    /// <summary>
    /// 将所有玩家初始化/重置
    /// </summary>
    public void InitPlayers()
    {
        foreach (List<PlayerAgent> pal in teams.Values)
        {
            foreach (PlayerAgent pa in pal)
            {
                pa.InitPlayer();
            }
        }
    }

    /// <summary>
    /// 为一个队伍的所有Agent添加奖励
    /// </summary>
    /// <param name="teamName">队名</param>
    /// <param name="reward">奖励值</param>
    public void SetTeamReward(string teamName,float reward)
    {
        if (teams.TryGetValue(teamName, out List<PlayerAgent> pal)) 
        {
            foreach(PlayerAgent pa in pal)
            {
                pa.AddReward(reward);
            }
        }
    }

    public void InitTimer(bool addEpisode = false)
    {
        timer = 0;
        if (addEpisode)
        {
            episodes++;
        }
    }

    public Vector3 NormalizePos(Vector3 pos)
    {
        return pos * stageDiagonalFactor;
    }

    public float NormalizeAngleY(Vector3 eulerAngles)
    {
        return eulerAngles.y / 180.0f - 1;
    }

}
