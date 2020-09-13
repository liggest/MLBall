using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeiPlayerAgent : PlayerAgent
{
    //public override void BumpWallReward()
    //{
    //    AddReward(-0.1f);
    //}

    public override void GetBallReward()
    {
        if (CurrentBall.lastPlayer == null)
        {
            AddReward(0.01f);
        }
        else if (IsTeammate(CurrentBall.lastPlayer))
        {
            AddReward(0.012f);
            SM.AddTeamReward(TeamName, 0.002f);
        }
        else
        {
            AddReward(0.02f);
        }

    }

    public override void FallReward()
    {
        SetReward(-1f);
    }

    public override void ShootReward(float forceValue)
    {
        AddReward(0.38f);
    }

    public override void KeepBallReward()
    {
        if (KeepBallTime > 3f)
        {
            AddReward(-0.000001f);
        }
        else
        {
            AddReward(0.001f);
        }
    }

    public override void GoalReward(Goal g, Ball b)
    {
        if (g.IsRivalGoal(b))
        {
            AddReward(1f);
        }
        else
        {
            AddReward(-0.8f);
        }
    }
    //public override void IdleReward()
    //{
    //    AddReward(-0.0009f);
    //}

    public override void ObservationReward(int observeType, Vector3 observePos, float distance)
    {
        if (observeType == 1)
        {
            AddReward(0.0005f);
        }
        if (distance > 1.8f && distance < 8f)
        {
            AddReward(1 / distance / 5000);
        }
        //else if (distance > 8f)
        //{
        //    AddReward(-distance / 1000);
        //}

        //    //}
        //    //public override void BumpPlayerReward(Transform playerTransform)
        //    //{
        //    //        AddReward(-0.001f);
        //    //}
    }
}
