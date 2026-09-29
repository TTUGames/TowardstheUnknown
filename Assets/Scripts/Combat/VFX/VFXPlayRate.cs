using UnityEngine;
using UnityEngine.VFX;

/// <summary>
/// Plays the VFX graphs under it at another rate, so that a graph timed for an attack lands on the impact of its retimed attack
/// </summary>
public class VFXPlayRate : MonoBehaviour
{
    [Min(0.05f), Tooltip("Speed of the graphs: 1.3 plays an effect set for 0.47 s at 0.36 s")]
    public float playRate = 1;

    private void OnEnable()
    {
        foreach (VisualEffect effect in GetComponentsInChildren<VisualEffect>(true))
            effect.playRate = playRate;
    }
}
