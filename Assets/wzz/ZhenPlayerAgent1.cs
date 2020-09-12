using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class ZhenPlayerAgent1 : PlayerAgent // <- 注意这里是Agent
{
    public float idleDis = 0;
    public int getBallCount = 0;
    public Vector3 idleVec = Vector3.zero;
    public override void OnEpisodeBegin()  // 每个周期开始时 重置场景
    {
        InitPlayer();
        SM.InitBalls();
        SM.InitTimer();
        Vector3 pos = transform.localPosition;
        pos.x = Random.Range(-10, 10);
        pos.z = Random.Range(-10, 10);
        transform.localPosition = pos;
        getBallCount = 0;
    }
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
        idleVec = Vector3.zero;
        CompareReward(0.5f, 0);
        SetReward(1f);
        if (++getBallCount > 3)
        {
            SM.EndEpisodes();
        }
        else
        {
            if (CurrentBall)
                CurrentBall.InitBall();
            InitPlayer();
            Vector3 pos = transform.localPosition;
            pos.x = Random.Range(-10f, 10f);
            pos.z = Random.Range(-10f, 10f);
            transform.localPosition = pos;
        }
    }
    public override void KeepBallReward()
    {
        CompareReward(0.005f, 0);
    }
    public override void LoseBallReward(Ball b)
    {
        idleDis = 0;
        idleVec = Vector3.zero;
        CompareReward(-0.1f, 0);
    }
    public override void ShootReward(float forceValue)
    {
        AddReward(forceValue / 15);
    }
    public override void BumpWallReward()
    {
        AddReward(-0.2f/MaxStep);
    }
    public override void BumpPlayerReward(Transform playerTransform)
    {
        if (IsTeammate(playerTransform.GetComponent<PlayerAgent>()))
        {
            AddReward(-0.1f);
        }
        else
        {
            AddReward(-0.05f);
        }
    }
    public override void FallReward()
    {
        SetReward(-0.5f);
    }
    public override void IdleReward()
    {
        idleDis += Vector3.Distance(transform.localPosition, LastPos);
        idleVec += transform.localPosition - LastPos;
        AddReward(-0.5f/MaxStep);
    }
    public override void ObservationReward(int observeType, Vector3 observePos, float distance)
    {
        if (observeType == -3)
        {
            // AddReward(0.05f/MaxStep);
        }
        else if (observeType == 1) //ball
        {
            AddReward(1f/10*Mathf.Exp(-distance) / MaxStep);
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