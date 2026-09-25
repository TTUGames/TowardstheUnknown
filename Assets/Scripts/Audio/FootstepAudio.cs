using UnityEngine;

/// <summary>
/// Plays the footstep sound of the entity from the walk animation events
/// </summary>
public class FootstepAudio : MonoBehaviour
{
    private AK.Wwise.Event footstep;

    private void Start()
    {
        footstep = GetComponent<EntityStats>().Data.footstep;
    }

    // Called by the animation events
    private void PlayFootstep()
    {
        footstep.Post(gameObject);
    }
}
