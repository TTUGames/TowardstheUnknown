using UnityEngine;

/// <summary>
/// Where an attack's VFX starts from, in place of its ability's <see cref="VFXInfo"/> target and offset: set on the Anniversary prefab
/// of an <see cref="EditionSkin"/> pair, it moves that edition's VFX while the Classic's original keeps the ability's target.
/// Read when the VFX plays, it runs nothing
/// </summary>
public class VFXAnchor : MonoBehaviour
{
    [Tooltip("The marker of the caster's body or the tile it starts from")]
    public VFXInfo.Target target = VFXInfo.Target.SOURCETILE;
    [Tooltip("From the target, in its space: a tile's origin is at its middle, half a meter under its top")]
    public Vector3 offset = new Vector3(0, 0.5f, 0);
}
