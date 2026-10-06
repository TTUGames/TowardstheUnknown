using UnityEngine;

/// <summary>
/// The intensity a room's main light takes under the rift (<see cref="RiftLighting"/>, before its boost), where the
/// original's value doesn't suit the Anniversary's light: the light keeps the original's, which the Classic shows
/// </summary>
[RequireComponent(typeof(Light))]
public class RiftLightIntensity : MonoBehaviour
{
    [Min(0)] public float intensity = 180;
}
