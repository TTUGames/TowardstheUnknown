using UnityEngine;

/// <summary>
/// A source of heat, such as a flame: the snow melts around it, in an uneven patch (SnowCover passes it to the Snow Lit surfaces)
/// </summary>
public class SnowHeat : MonoBehaviour
{
    [SerializeField, Tooltip("How far the snow melts around the source, in meters, before the noise of the patch's edge")] private float radius = 1.6f;

    public Vector4 Source => new Vector4(transform.position.x, transform.position.y, transform.position.z, radius);
}
