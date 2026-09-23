using UnityEngine;

public class EnemyFootstepAudio : MonoBehaviour {
    string eventName;
    public float delayWalking = 0.3f;
    public float delayRunning = 0.2f; 

    void Start() {
        eventName = gameObject.name.Replace("(Clone)", "") + "_Footstep";
    }

    void PlayFootstep()
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }
}
