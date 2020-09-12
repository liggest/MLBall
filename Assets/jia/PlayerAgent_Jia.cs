using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

public class PlayerAgent_Jia : PlayerAgent
{
    public override void ShootReward(float forceValue)
    {
        AddReward(0.3f);
    }

    public override void LoseBallReward(Ball b)
    {
        AddReward(-0.3f);
    }

    public override void GoalReward(Goal g,Ball b)
    {
        if (g.IsRivalGoal(b))
        {
            AddReward(1.0f);
        } else
        {
            AddReward(-1.0f);
        }
    }

    public override void IdleReward()
    {
        AddReward(0.001f);
    }

    public override void GetBallReward()
    {
        AddReward(1f);
    }

    public override void ObservationReward(int observeType, Vector3 observePos, float distance)
    {
        if (observeType == 1)
        {
            AddReward(0.01f*distance);
        }
    }
}
