using UnityEngine;

public class FootstepAudio : MonoBehaviour {

    void PlayFootstep()
    {
        AkUnitySoundEngine.PostEvent("Player_Footstep", gameObject);
    }
}
