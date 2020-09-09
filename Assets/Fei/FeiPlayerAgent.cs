using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeiPlayerAgent : PlayerAgent
{
    public override void BumpWallReward()
    {
        AddReward(-0.1f);
    }

    public override void GetBallReward()
    {
        if (CurrentBall.lastPlayer == null)
        {
            AddReward(0.25f);
            SM.AddTeamReward(TeamName, 0.02f);
        }else if (IsTeammate(CurrentBall.lastPlayer))
        {
            AddReward(0.2f);
            SM.AddTeamReward(TeamName, 0.018f);
        }
        else
        {
            AddReward(0.3f);
        }

    }

    public override void FallReward()
    {
        SetReward(-1f);
    }

    public override void ShootReward(float forceValue)
    {
        AddReward(0.2f);
    }

    public override void KeepBallReward()
    {
        AddReward(0.01f);
    }

    public override void GoalReward(Goal g, Ball b)
    {
        if (g.IsRivalGoal(b))
        {
            AddReward(1f);
        }
        else
        {
            AddReward(-1f);
        }
    }
    public override void IdleReward()
    {
        AddReward(-0.001f);
    }
    public override void ObservationReward(int observeType, Vector3 observePos)
    {
        if (observeType == 1)
        {
            AddReward(1);
        }
        float distance = Vector3.Distance(observePos, transform.localPosition);
        if (distance > 1.8f && distance < 8f)
        {
            AddReward(1 / distance / 1000);
        }
        else if (distance > 8f)
        {
            AddReward(-distance / 1000);
        }

    }
    public override void BumpPlayerReward(Transform playerTransform)
    {
            AddReward(-0.001f);
    }
}
