using UnityEngine;

/// <summary>
/// A utility class preventing an object from rotating because of its parent
/// </summary>
public class ConstantRotation : MonoBehaviour
{
    private Vector3 rotation;

    /// <summary>
    /// Sets (or overrides) the object's constant rotation
    /// </summary>
    /// <param name="rotation"></param>
    public void SetRotation(Vector3 rotation) {
        this.rotation = rotation;
	}

    void LateUpdate()
    {
        transform.rotation = Quaternion.Euler(rotation);
    }
}
