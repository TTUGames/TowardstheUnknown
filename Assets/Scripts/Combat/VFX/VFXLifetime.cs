using UnityEngine;

/// <summary>
/// Keeps an attack's VFX instance at least this long from its play, past its ability's <c>vfxDuration</c>: a mark left on the
/// ground fades out instead of vanishing when the attack's VFX are removed. Read when the attack releases it, it runs nothing
/// </summary>
public class VFXLifetime : MonoBehaviour
{
    [Min(0), Tooltip("Seconds from the play of the VFX before it may be removed")]
    public float seconds = 5;
}
