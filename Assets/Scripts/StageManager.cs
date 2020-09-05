using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public Ball ball;

    private void Awake()
    {
        Utils.SetStage(this);
    }

    // Start is called before the first frame update
    void Start()
    {
                
    }

}
