using UnityEngine;

/// <summary>
/// A local draft, placed in a room (near a crack of the rift, an opening): the plants around it lean harder, the way its
/// forward axis points. <see cref="Wind"/> passes the active room's drafts to the shaders
/// </summary>
public class WindDraft : MonoBehaviour
{
    [SerializeField, Min(0.1f), Tooltip("The plants lean fully within 0.4 of it, less up to the whole radius, in meters")] private float radius = 3;
    [SerializeField, Min(0), Tooltip("How much it adds to the lean, in multiples of the plants' bend")] private float strength = 1.5f;

    public Vector4 Area => new Vector4(transform.position.x, transform.position.y, transform.position.z, radius);

    public Vector4 Force
    {
        get
        {
            Vector3 direction = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * strength;
            return new Vector4(direction.x, 0, direction.z, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.6f, 0.9f, 1f, 0.6f);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.DrawRay(transform.position, Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized * radius);
    }
}
