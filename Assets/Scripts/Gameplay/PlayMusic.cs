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
        GameObject.FindGameObjectWithTag("AudioSource").GetComponent<AudioSource>().PlayOneShot(Resources.Load<AudioClip>("SFX/Background_Sound"), 1f);
    }
}
