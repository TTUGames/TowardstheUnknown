using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn timer
/// </summary>
public class Timer : MonoBehaviour
{
    public float timeToEndTurn = 30f;

    public  float timeRemaining; //TODO after Timer is done, put in private
    private float TimeRemainingInPrecedentTurn;

    private bool stopTimer = false;
    
    /// <summary>
    /// Launch the timer
    /// </summary>
    public void LaunchTimer()
    {
        stopTimer = false;
        StartCoroutine(Countdown());
    }

    public void StopTimer()
    {
        stopTimer = true;
    }

    /// <summary>
    /// Decrement the <c>timeRemaining each second</c> <br/>
    /// Uses the current turn time remaining then if it reach 0, uses the time remaining from precedent turn
    /// </summary>
    /// <returns>Nothing</returns>
    private IEnumerator Countdown()
    {
        timeRemaining = timeToEndTurn;
        while (timeRemaining > 0 && TimeRemainingInPrecedentTurn > 0)
        {
            if (!stopTimer)
            {
                timeRemaining--;

                if(timeRemaining == 0)
                {
                    timeRemaining = TimeRemainingInPrecedentTurn;
                    TimeRemainingInPrecedentTurn = 0;
                }

                yield return new WaitForSeconds(1);
            }
            else
            {
                TimeRemainingInPrecedentTurn = timeRemaining;
                yield break;
            }
        }
        TimeRemainingInPrecedentTurn = 0;
        //FINISH TURN
    }

    /// <summary>
    /// Reset the timer to the default value <c>timeToEndTurn</c>
    /// </summary>
    public void ResetTimer()
    {
        timeRemaining = timeToEndTurn;
    }
}
