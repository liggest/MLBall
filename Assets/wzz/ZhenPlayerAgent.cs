using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class ZhenPlayerAgent : PlayerAgent // <- 注意这里是Agent
{
    public float idleDis = 0;
    public override void GoalReward(Goal g, Ball b)
    {
        //g.IsRivalGoal
        //b.lastPlayer
        //SetReward
        float factor = g.IsRivalGoal(b) ? 1f : -1f;
        SetCompareReward(factor, -factor);
    }
    public override void GetBallReward()
    {
        idleDis = 0;
        CompareReward(0.5f, -0.1f);
    }
    public override void KeepBallReward()
    {
        CompareReward(0.005f, 0);
    }
    public override void LoseBallReward(Ball b)
    {
        idleDis = 0;
        CompareReward(-0.1f, 0);
    }
    public override void ShootReward(float forceValue)
    {
        AddReward(forceValue / 15);
    }
    public override void BumpWallReward()
    {
        AddReward(-0.05f);
    }
    public override void BumpPlayerReward(Transform playerTransform)
    {
        if (IsTeammate(playerTransform.GetComponent<PlayerAgent>()))
        {
            AddReward(-0.1f);
        }
        else
        {
            AddReward(-0.1f);
        }
    }
    public override void FallReward()
    {
        SetReward(-1f);
    }
    public override void IdleReward()
    {
        idleDis += Vector3.Distance(transform.localPosition, LastPos);
        AddReward(Vector3.Distance(transform.localPosition, LastPos) / 1e5f);
    }
    public override void ObservationReward(int observeType, Vector3 observePos)
    {
        if (observeType == -3)
        {
            AddReward(0.001f);
        }
        else if (observeType == 1)
        {
            AddReward(0.001f);
        }
    }
    private void CompareReward(float main, float other)
    {
        foreach (string key in SM.teams.Keys)
        {
            if (key == TeamName)
            {
                foreach (PlayerAgent pa in SM.teams[key])
                {
                    pa.AddReward(main);
                }
            }
            else
            {
                foreach (PlayerAgent pa in SM.teams[key])
                {
                    pa.AddReward(other);
                }
            }
        }
    }
    private void SetCompareReward(float main, float other)
    {
        foreach (string key in SM.teams.Keys)
        {
            if (key == TeamName)
            {
                foreach (PlayerAgent pa in SM.teams[key])
                {
                    pa.SetReward(main);
                }
            }
            else
            {
                foreach (PlayerAgent pa in SM.teams[key])
                {
                    if (other != 0)
                        pa.SetReward(other);
                }
            }
        }
    }
}