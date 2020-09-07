using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeiPlayerAgent : PlayerAgent
{
    public override void BumpWallReward()
    {
        SetReward(-0.1f);
    }

    public override void GetBallReward()
    {
        if (currentBall.lastPlayer == null)
        {
            SetReward(1f);
            sm.SetTeamReward(TeamName, 0.2f);
        }else if (IsTeammate(currentBall.lastPlayer))
        {
            SetReward(0.7f);
            sm.SetTeamReward(TeamName, 0.1f);
        }
        else
        {
            SetReward(1.2f);
        }

    }

    public override void FallReward()
    {
        SetReward(-0.5f);
    }

    public override void ShootReward()
    {
        SetReward(0.2f);
    }

    public override void KeepBallReward()
    {
        SetReward(0.01f);
    }

    public override void GoalReward(Goal g, Ball b)
    {
        if (g.IsRivalGoal(b))
        {
            SetReward(3.6f);
        }
        else
        {
            SetReward(-3.8f);
        }
    }
    public override void IdleReward()
    {
        SetReward(-0.001f);
    }
    public override void ObservationReward(int observeType, Vector3 observePos)
    {
        if(observeType == 1)
        {
            SetReward(0.0006f);
        }
        float distance = Vector3.Distance(observePos, transform.localPosition);
        if (distance > 1.8f && distance<8f)
        {
            SetReward(1 / distance / 1000);
        }else if (distance > 8f)
        {
            SetReward(-distance / 1000);
        }
        
    }
}
