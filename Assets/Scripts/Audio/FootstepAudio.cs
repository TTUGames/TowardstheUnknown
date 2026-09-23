using UnityEngine;

/// <summary>
/// Plays the <c>&lt;Entity&gt;_Footstep</c> Wwise event from the walk animation events
/// </summary>
public class FootstepAudio : MonoBehaviour
{
    private string eventName;

    private void Start()
    {
        //"Kameiko(Clone)" or "Player (2)" play Kameiko_Footstep and Player_Footstep
        eventName = gameObject.name.Split('(')[0].Trim() + "_Footstep";
    }

    // Called by the animation events
    private void PlayFootstep()
    {
        AkUnitySoundEngine.PostEvent(eventName, gameObject);
    }
}
