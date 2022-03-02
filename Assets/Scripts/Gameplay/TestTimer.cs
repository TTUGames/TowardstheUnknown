using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestTimer : MonoBehaviour
{
    public TimerCountdown timer;
    // Start is called before the first frame update
    void Start()
    {
        timer.LaunchTimer();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
