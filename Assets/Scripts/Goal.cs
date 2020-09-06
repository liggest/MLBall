using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //public Ball ball;
    public int teamID;

    StageManager sm;
    MeshRenderer mr;

    private void Start()
    {
        sm = Utils.GetStage(transform);
        sm.teamGoals.Add(GlobalManager.instance.GetTeamName(teamID), this);
        mr = GetComponent<MeshRenderer>();
        mr.material.color = GlobalManager.instance.GetTeamColor(teamID);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Ball ball = other.GetComponent<Ball>();
            ball.InitBall();
            //sm.ball.ResetBall();
        }
    }

    public bool IsRivalGoal(Ball ball)
    {
        if(ball.lastPlayer && ball.lastPlayer.teamID != teamID)
        {
            return true;
        }
        return false;
    }
}
