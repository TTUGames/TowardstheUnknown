using UnityEngine;

/// <summary>
/// Turns its object upright in the world when it wakes, keeping its heading: a flame stays upright whatever the rotation
/// of the object holding it (a torch placed upside down or tilted in a room). At runtime only, so that the rooms' prefabs
/// don't get overrides
/// </summary>
public class KeepUpright : MonoBehaviour
{
    private void Awake()
    {
        transform.rotation = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;
    }
}
