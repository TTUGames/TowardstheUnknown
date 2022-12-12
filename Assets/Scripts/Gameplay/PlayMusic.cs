using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayMusic : MonoBehaviour
{
    private void Start()
    {
        LaunchAmbianceSound();
    }

    void LaunchAmbianceSound()
    {
        GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().loop = true;
        GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().outputAudioMixerGroup = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer.FindMatchingGroups("Ambiance")[0];
        StartCoroutine(LoopAudio("SFX/Background_Sound"));
    }
    IEnumerator LoopAudio(string sfx)
    {
        AudioSource audio = GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>();
        float length = Resources.Load<AudioClip>(sfx).length;

        while (true)
        {
            audio.PlayOneShot(Resources.Load<AudioClip>(sfx), 1f);
            yield return new WaitForSeconds(length);
        }
    }
}
