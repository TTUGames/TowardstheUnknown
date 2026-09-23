using UnityEngine;

public class EnemyFootstepAudio : MonoBehaviour {
    string eventName;

    void Start() {
        eventName = gameObject.name.Replace("(Clone)", "") + "_Footstep";
    }

    void PlayFootstep()
    {
        AkUnitySoundEngine.PostEvent(eventName, gameObject);
    }
}
