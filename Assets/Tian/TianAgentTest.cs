using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TianAgentTest : PlayerAgent
{
    /*

        最初：
            随机Agent位置和角度、随机球位置、Agent碰球则周期结束、玩家看球有奖励，看非球的话奖励会减少甚至变成惩罚
        几个模型后：（约catchBall4）
            取消了看球奖励
        约catchBall6：
            Agent碰球不再游戏结束，而是同一Agent累计碰球三次后才周期结束，每次碰球后球位置随机变化
        catchBall7：
            同一Agent累计碰球若干次后周期结束，碰球次数要求随着周期数缓慢增加，每次碰球后球位置随机变化，拥有随机初速度
    */
    float ballCount = 0;
    float catchCount = 0;
    static float catchLimit = 3;

    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        /*
        transform.localPosition = RandomPosition();
        foreach(Ball b in SM.balls)
        {
            b.transform.localPosition = RandomPosition();
        }
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
        */

        //ballCount = 0;
        //catchCount = 0;
    }

    Vector3 RandomPosition()
    {
        return new Vector3(Random.Range(-9, 9), 1, Random.Range(-13, 13));
    }
    public override void ObservationReward(int observeType, Vector3 observePos, float distance)
    {
        /*
        if (!IsKeepingBall)
        {
            if (observeType == 1)
            {
                ballCount++;
                float award = ballCount * 0.0001f;
                AddReward(award);
                //Debug.Log(award);
            }
            else
            {
                ballCount -= 0.002f;
            }
        }
        */
    }
    public override void GetBallReward()
    {
        /*
        AddReward(0.1f);
        foreach (string key in SM.teams.Keys)
        {
            if (key == TeamName)
            {
                SM.AddTeamReward(key, 0.1f);
            }
            else
            {
                SM.AddTeamReward(key, -0.02f);
            }
        }
        catchCount++;
        if (catchCount < catchLimit)
        {
            Ball b = CurrentBall;
            b.InitBall();
            b.transform.localPosition = RandomPosition();
            b.Rig.velocity = RandomPosition();
            //StartCoroutine(NewBall(b));
        }
        else
        {
            catchLimit += 0.1f;
            AddReward((catchLimit - 3) * 0.04f);
            Debug.Log($"{TeamName}-{name}拿到了{(int)catchLimit}个！");
            StartCoroutine(EndEpisodeCoroutine());
        }
        //StartCoroutine(EndEpisodeCortine());
        */

        if (!IsTeammate(CurrentBall.lastPlayer))
        {
            AddReward(0.1f);
            SM.AddTeamReward(TeamName, 0.01f);
            if (CurrentBall.lastPlayer)
            {
                CurrentBall.lastPlayer.AddReward(-0.05f);
                SM.AddTeamReward(CurrentBall.lastPlayer.TeamName, -0.01f);
            }
        }
    }
    public override void KeepBallReward()
    {
        float award = -Mathf.Log10(KeepBallTime + 5) + 1; //5秒内持球是正奖励
        award *= 0.005f;
        AddReward(award);
        //Debug.Log(award);   
    }
    public override void GoalReward(Goal g, Ball b)
    {
        if (g.IsRivalGoal(b))
        {
            AddReward(1.0f);
            SM.AddTeamReward(TeamName, 0.2f);
        }
        else
        {
            AddReward(-1.0f);
            SM.AddTeamReward(TeamName, -0.2f);
        }
    }

    public override void IdleReward()
    {
        //AddReward(-0.0004f);
    }

    public override void FallReward()
    {
        SetReward(-1f);
    }

    IEnumerator EndEpisodeCoroutine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        SM.EndEpisodes();
    }

    IEnumerator NewBall(Ball b)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        if (b)
        {
            b.transform.localPosition = RandomPosition();
        }

    }

}
