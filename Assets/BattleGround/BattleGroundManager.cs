using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleGroundManager : StageManager
{
    public bool needRandom = true;

    public new void FixedUpdate()
    {
        if (timer == 0 && needRandom)
        {
            needRandom = false;
            HideAll();
            string[] twoTeam = RandomTeams(2);
            ShowAsTeam(twoTeam[0], 1);
            ShowAsTeam(twoTeam[1], 2);
            Debug.Log($"当前是{twoTeam[0]}队与{twoTeam[1]}队在比赛");
        }
        if (timer > 3)
        {
            needRandom = true;
        }
        base.FixedUpdate();
    }

    public void HideAll()
    {
        foreach (string k in teamGoals.Keys)
        {
            teamGoals[k].gameObject.SetActive(false);
            foreach (PlayerAgent pa in teams[k])
            {
                pa.gameObject.SetActive(false);
            }
        }
    }

    public void ShowAsTeam(string name,int team)
    {
        Goal g = teamGoals[name];
        g.gameObject.SetActive(true);
        SetGoalTeam(g, team);
        foreach (PlayerAgent pa in teams[name])
        {
            pa.gameObject.SetActive(true);
            SetAgentTeam(pa, team);
        }
    }

    public void SetGoalTeam(Goal g,int team)
    {
        Vector3 lp = g.transform.localPosition;
        BoxCollider bc = g.GetComponents<BoxCollider>()[1];
        Vector3 bcCenter = bc.center;
        lp.z = TeamSign(lp.z, team);
        bcCenter.z = TeamSign(bcCenter.z, team);
        g.transform.localPosition = lp;
        bc.center = bcCenter;
    }

    public void SetAgentTeam(PlayerAgent pa,int team)
    {
        pa.InitPlayer();
        float initZ = pa.transform.localPosition.z;
        Vector3 lp = pa.transform.localPosition;
        lp.z = TeamSign(lp.z, team);
        bool change = initZ != lp.z;
        if (change)
        {
            lp.x *= -1;
            pa.transform.localEulerAngles += Vector3.up * 180f;
        }
        pa.transform.localPosition = lp;
    }

    public float TeamSign(float val,int team)
    {
        if (team == 1)
        {
            return -1 * Mathf.Abs(val);
        }
        else
        {
            return Mathf.Abs(val);
        }
    }

    public string[] RandomTeams(int count)
    {
        List<string> keys = new List<string>();
        foreach (string k in teamGoals.Keys)
        {
            keys.Add(k);
        }
        string[] resultTeams = new string[count];
        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, keys.Count);
            resultTeams[i] = keys[idx];
            keys.RemoveAt(idx);
        }
        return resultTeams;
    } 
}
