using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TianRaySensorAgent : PlayerAgent
{
    float maxStepFactor;
    float timeMinus = 0;

    public override void Initialize()
    {
        maxStepFactor = 1.0f / MaxStep;
    }

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();
        timeMinus = 0;
        //transform.localPosition /= 2.0f;
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        base.OnActionReceived(vectorAction);

        timeMinus -= maxStepFactor;
    }

    public override void GetBallReward()
    {
        AddReward(0.2f);
    }

    public override void GoalReward(Goal g, Ball b)
    {
        if (g.IsRivalGoal(b))
        {
            AddReward(1 + timeMinus);
        }
        else
        {
            AddReward(-1);
        }
    }

    public override void FallReward()
    {
        SetReward(-1);
    }

    public override void BumpWallReward()
    {
        AddReward(-maxStepFactor);
    }

}
