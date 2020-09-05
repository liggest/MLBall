using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public List<Ball> balls = new List<Ball>();
    public Dictionary<string, List<PlayerAgent>> teams = new Dictionary<string, List<PlayerAgent>>();

    private void Awake()
    {
        Utils.SetStage(this);
    }

}
