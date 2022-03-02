using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerCountdown : MonoBehaviour
{
    public float timeToEndTurn = 30f;

    public float timeRemaining;

    // Start is called before the first frame update
    void Start()
    {
        timeRemaining = timeToEndTurn;
    }

    
    public void LaunchTimer()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (timeRemaining > 0)
        {
            timeRemaining--;
            yield return new WaitForSeconds(1);
        }
        if(timeRemaining == 0)
        {
            //FINISH TURN
        }
    }

    public void resetTimer()
    {
        timeRemaining = timeToEndTurn;
    }
}
