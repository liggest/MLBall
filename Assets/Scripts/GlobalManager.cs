using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager instance;

    public List<string> teamNames = new List<string>();
    public List<Color> teamColors = new List<Color>();
    public List<Color> teamHatColors = new List<Color>();

    public string defaultTeamName = "煤";
    public Color defaultTeamColor = Color.black;
    public Color defaultTeamHatColor = Color.grey;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.Log("出现了复数 GlobalManager，有问题");
        }
    }

    public int GetTeamID(string teamName)
    {
        int idx = teamNames.IndexOf(teamName);
        if (idx >= 0)
        {
            return idx + 1;
        }
        return idx;
    }

    public Color GetTeamColor(int teamID, bool isHat = false)
    {
        if (teamID < 0)
        {
            if (isHat)
            {
                return defaultTeamHatColor;
            }
            else
            {
                return defaultTeamColor;
            }
        }
        else
        {
            if (isHat)
            {
                return teamHatColors[teamID - 1];
            }
            else
            {
                return teamColors[teamID - 1];
            }
        }
    }

    public string GetTeamName(int teamID)
    {
        if (teamID < 0)
        {
            return defaultTeamName;
        }
        return teamNames[teamID - 1];
    }

}
