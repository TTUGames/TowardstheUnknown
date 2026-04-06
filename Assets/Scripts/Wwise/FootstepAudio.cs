using UnityEngine;

public class FootstepAudio : MonoBehaviour {

    void PlayFootstep()
    {
        AkSoundEngine.PostEvent("Player_Footstep", gameObject);
    }
}
