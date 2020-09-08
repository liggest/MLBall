using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;

public class ZhenPlayerAgent : PlayerAgent // <- 注意这里是Agent
{
    public void GoalReward(Goal g, Ball b)
    {
        //g.IsRivalGoal
        //b.lastPlayer
        //SetReward
        float factor = g.IsRivalGoal(b) ? -1f : 1f;
        foreach (string key in sm.teams.Keys)
        {
            if(key==teamName)
            {
                SetReward(factor);
            }else{
                SetReward(-factor);
            }
        }
    }
}