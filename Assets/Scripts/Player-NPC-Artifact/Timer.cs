using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A timer that can be linked to any UnityEvent
/// </summary>
public class Timer : MonoBehaviour
{
    public float timeToEndTurn = 30f;

    private float timeRemaining; //TODO after Timer is done, put in private

    public UnityEvent onTimerEnd;

    private Coroutine timerCoroutine = null;
    
    /// <summary>
    /// Launch the <c>Timer</c> from the start
    /// </summary>
    public void LaunchTimer()
    {
        ResetTimer();
        timerCoroutine = StartCoroutine(Countdown());
    }

    /// <summary>
    /// Stop the <c>Timer</c>
    /// </summary>
    public void StopTimer()
    {
        StopCoroutine(timerCoroutine);
    }

    /// <summary>
    /// Reset the <c>Timer</c> to the default value <c>timeToEndTurn</c>
    /// </summary>
    public void ResetTimer()
    {
        timeRemaining = timeToEndTurn;
    }

    /// <summary>
    /// Decrement the <c>timer</c> each second<br/>
    /// Uses the current turn time remaining. Todo : Add the possibility to keep time for the next turn.
    /// </summary>
    /// <returns>an <c>IEnumerator</c></returns>
    private IEnumerator Countdown()
    {
        while (timeRemaining > 0)
        {
            timeRemaining--;

            yield return new WaitForSeconds(1);
        }
        onTimerEnd.Invoke();
    }
}
