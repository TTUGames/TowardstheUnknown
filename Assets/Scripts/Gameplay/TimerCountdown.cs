using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Turn timer
/// </summary>
public class TimerCountdown : MonoBehaviour
{
    public float timeToEndTurn = 30f;

    public float timeRemaining;
    
    /// <summary>
    /// Launch the timer
    /// </summary>
    public void LaunchTimer()
    {
        StartCoroutine(Countdown());
    }

    /// <summary>
    /// Decrement the <c>timeRemaining each second</c>
    /// </summary>
    /// <returns>Nothing</returns>
    private IEnumerator Countdown()
    {
        Debug.Log(timeRemaining);
        timeRemaining = timeToEndTurn;
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

    /// <summary>
    /// Reset the timer to the default value <c>timeToEndTurn</c>
    /// </summary>
    public void resetTimer()
    {
        timeRemaining = timeToEndTurn;
    }
}
