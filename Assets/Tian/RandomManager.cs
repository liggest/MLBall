using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomManager : MonoBehaviour
{
    public int maxAgnetCount = 4;

    int agentCount = 0;

    public int activeCount = 4;

    public List<PlayerAgent> agents = new List<PlayerAgent>();

    public void DisactiveAgents()
    {
        foreach (PlayerAgent pa in agents)
        {
            pa.gameObject.SetActive(false);
        }
    }

    public void ActivateAll(PlayerAgent target)
    {
        foreach (PlayerAgent pa in agents)
        {
            if (!pa.Equals(target))
            {
                pa.gameObject.SetActive(true);
                activeCount++;
            }
        }
    }

    public void RandomActiveOne()
    {
        int idx = Random.Range(0, agents.Count);

        if (!agents[idx].gameObject.activeInHierarchy)
        {
            agents[idx].gameObject.SetActive(true);
        }
        for (int i = 0; i < agents.Count; i++)
        {
            if (i != idx)
            {
                agents[i].gameObject.SetActive(false);
                activeCount--;
            }
        }
    }

    public void AddAgentCount()
    {
        agentCount++;
    }

    public void FixedUpdate()
    {
        //Debug.Log(agentCount);
        if (agentCount >= maxAgnetCount)
        {
            RandomActiveOne();
            Debug.Log("多了一个");
        }
        agentCount = 0;
    }


}
