using UnityEngine;

public class FootstepAudio : MonoBehaviour {
    public TacticsMove tacticsMove;
    float nextEventTime;
    public float delayWalking = 1f;
    public float delayRunning = 0.2f; 

    void Start() {
        nextEventTime = 0;
    }

    void Update() {
        if (tacticsMove.isMoving && Time.time > nextEventTime) {
            AkSoundEngine.PostEvent("Player_Footstep", gameObject);
            if (tacticsMove.distanceToTarget < tacticsMove.tileToRun)
                nextEventTime = Time.time + delayWalking;
            else
                nextEventTime = Time.time + delayRunning;
        }
    }
}
