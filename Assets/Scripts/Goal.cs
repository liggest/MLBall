using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //public Ball ball;
    [Tooltip("队伍id")]
    public int teamID;
    string teamName;

    StageManager sm;
    MeshRenderer mr;

    private void Start()
    {
        sm = Utils.GetStage(transform);
        sm.teamGoals.Add(GlobalManager.instance.GetTeamName(teamID), this);
        mr = GetComponent<MeshRenderer>();
        mr.material.color = GlobalManager.instance.GetTeamColor(teamID);
        teamName = GlobalManager.instance.GetTeamName(teamID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Ball ball = other.GetComponent<Ball>();
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
        if(ball.lastPlayer && ball.lastPlayer.TeamID != teamID)
        {
            return true;
        }
        return false;
    }

    private void GoalAndScore(Ball ball)
    {
        if (ball.lastPlayer)
        {
            Debug.Log($"{ball.lastPlayer.TeamName}队 进球了！{teamName}队 被破门！");
            if (IsRivalGoal(ball))
            {
                ball.lastPlayer.SetReward(5);
            }
            else
            {
                ball.lastPlayer.SetReward(-5);
            }
        }
        ball.InitBall();
        foreach (List<PlayerAgent> pal in sm.teams.Values){
            foreach(PlayerAgent pa in pal){
                pa.EndEpisode();
            }
        }
    }
}
