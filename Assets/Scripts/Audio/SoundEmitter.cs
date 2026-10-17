using Sirenix.OdinInspector;
using UnityEngine;

/// <summary>
/// A Wwise loop played from this object while it is active, in an edition with <see cref="EditionProfile.placedSounds"/>: the
/// crackle of a flame, the hum of a crystal. Wwise places it where the object stands (it adds the AkGameObj itself), so the
/// attenuation of the event does the rest; the original's scenery was silent
/// </summary>
public class SoundEmitter : MonoBehaviour
{
    [SerializeField, Tooltip("The looping event, stopped when the object goes inactive or the edition has no placed sounds")] private AK.Wwise.Event loop = new AK.Wwise.Event();
    [SerializeField, Min(0), SuffixLabel("ms"), Tooltip("Fade out of the loop when it stops")] private int fadeOut = 300;

    private bool playing;

    private void OnEnable()
    {
        Edition.Changed += OnEditionChanged;
        Apply();
    }

    private void OnDisable()
    {
        Edition.Changed -= OnEditionChanged;
        Stop();
    }

    private void OnEditionChanged(GameEdition edition) => Apply();

    private void Apply()
    {
        if (Edition.Profile.placedSounds) Play();
        else Stop();
    }

    private void Play()
    {
        if (playing || !loop.IsValid()) return;
        loop.Post(gameObject);
        playing = true;
    }

    private void Stop()
    {
        if (!playing) return;
        loop.Stop(gameObject, fadeOut);
        playing = false;
    }
}
