using UnityEngine;

public class EnemyFootstepAudio : MonoBehaviour {
    public EnemyMove enemyMove;
    float nextEventTime;
    string eventName;
    public float delayWalking = 0.3f;
    public float delayRunning = 0.2f; 

    void Start() {
        nextEventTime = 0;
        eventName = gameObject.name.Replace("(Clone)", "") + "_Footstep";
    }

    void PlayFootstep() 
    {
        AkSoundEngine.PostEvent(eventName, gameObject);
    }

    /*void Update() {
        if (enemyMove.isMoving && Time.time > nextEventTime) {
            AkSoundEngine.PostEvent(eventName, gameObject);
            if (enemyMove.distanceToTarget < enemyMove.tileToRun)
                nextEventTime = Time.time + delayWalking;
            else
                nextEventTime = Time.time + delayRunning;
        }
    }*/
}
