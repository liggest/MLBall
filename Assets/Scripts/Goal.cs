using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //public Ball ball;
    [Tooltip("队伍id")]
    public int teamID;
    string teamName;

    [Tooltip("在进球时结束Episode")]
    public bool EndAtGoal = true;

    StageManager sm;
    MeshRenderer mr;

    private void Start()
    {
        sm = Utils.GetStage(transform);
        sm.teamGoals.Add(GlobalManager.instance.GetTeamName(teamID), this);
        mr = GetComponent<MeshRenderer>();
        ChangeTeam();
    }

    public void ChangeTeam()
    {
        mr.material.color = GlobalManager.instance.GetTeamColor(teamID);
        teamName = GlobalManager.instance.GetTeamName(teamID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Ball ball = other.GetComponent<Ball>();
            //ball.ResetOwner();
            GoalAndScore(ball);
            //sm.ball.ResetBall();
        }
    }

    /// <summary>
    /// 判断是否是对手的进球
    /// </summary>
    /// <param name="ball">进球的球</param>
    /// <returns>是否是对手的进球</returns>
    public bool IsRivalGoal(Ball ball)
    {
        if(ball.owner && ball.owner.TeamID != teamID)
        {
            return true;
        }
        else if(ball.lastPlayer && ball.lastPlayer.TeamID != teamID)
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// 进球并得分（获得奖励）
    /// </summary>
    /// <param name="ball">进球的那个球</param>
    private void GoalAndScore(Ball ball)
    {
        if (ball.owner)
        {
            Debug.Log($"{ball.owner.TeamName}队 进球了！{teamName}队 被破门！");
            ball.owner.GoalReward(this, ball);
        }
        else if (ball.lastPlayer)
        {
            Debug.Log($"{ball.lastPlayer.TeamName}队 进球了！{teamName}队 被破门！");
            ball.lastPlayer.GoalReward(this, ball);
        }
        sm.InitBalls();

        if (EndAtGoal)
        {
            if (ball.lastPlayer && ball.lastPlayer.BP.BehaviorType == Unity.MLAgents.Policies.BehaviorType.HeuristicOnly)
            {
                sm.InitPlayers();
                sm.InitTimer(true);
            }
            else
            {
                sm.EndEpisodes();
            }
        }

    }
}
