using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TianAgentTest : PlayerAgent
{


    public override void OnEpisodeBegin()
    {
        base.OnEpisodeBegin();

        transform.localPosition = RandomPosition();
        foreach(Ball b in SM.balls)
        {
            b.transform.localPosition = RandomPosition();
        }
        transform.localRotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    Vector3 RandomPosition()
    {
        return new Vector3(Random.Range(-9, 9), 1, Random.Range(-13, 13));
    }

    public override void ObservationReward(int observeType, Vector3 observePos, float distance)
    {
        if (observeType == 1)
        {
            float distanceFactor = (1.0f - distance / maxViewDistance) * 0.1f;
            AddReward(distanceFactor);
        }
    }

    public override void GetBallReward()
    {
        AddReward(0.5f);
        SM.AddTeamReward(TeamName, 0.2f);
        CurrentBall.ResetOwner();
        StartCoroutine(EndEpisodeCortine());
    }

    IEnumerator EndEpisodeCortine()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForFixedUpdate();
        yield return new WaitForEndOfFrame();
        SM.EndEpisodes();
    }

}
