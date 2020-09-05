using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    //public Ball ball;

    StageManager sm;

    private void Start()
    {
        sm = Utils.GetStage(transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ball"))
        {
            Ball ball = other.GetComponent<Ball>();
            ball.InitBall();
            //sm.ball.ResetBall();
        }
    }
}
