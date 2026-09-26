using System.Collections;
using UnityEngine;

[System.Serializable]
public class VFXInfo
{
    public enum Target { GUN, SWORD, LEFTHAND, RIGHTHAND, SOURCETILE, TARGETTILE, BACK }

    [SerializeField] private GameObject prefab;
    [SerializeField] private Target target;
    [SerializeField] private float delay;
    [SerializeField] private Vector3 offset;
    [SerializeField] private float rotationOffset;

    public GameObject Prefab => prefab;

    public void Play(AttackAnimationAction action, GameObject source, Tile targetTile) {
        if (prefab == null) return;
        TacticsMove entity = source.GetComponent<TacticsMove>();
        entity.StartCoroutine(PlayDelayed(action, entity, targetTile));
	}

    private IEnumerator PlayDelayed(AttackAnimationAction action, TacticsMove source, Tile targetTile) {
        yield return new WaitForSeconds(delay);

        Tile sourceTile = source.CurrentTile;
        GameObject vfx = VFXPool.Get(prefab, GetOrigin(source, targetTile));

        //Faces the target, or the source's facing on its own tile
        Vector3 VFXRotation = sourceTile != targetTile
            ? Vector3.up * ((-Vector3.SignedAngle(sourceTile.transform.position - targetTile.transform.position, Vector3.forward, Vector3.up) + rotationOffset) % 360)
            : source.transform.rotation.eulerAngles + Vector3.up * rotationOffset;

        action.AddVFX(vfx);
        if (!vfx.TryGetComponent(out ConstantRotation constantRotation)) constantRotation = vfx.AddComponent<ConstantRotation>();
        constantRotation.SetRotation(VFXRotation);
        vfx.transform.localPosition = offset;
    }

    private Transform GetOrigin(TacticsMove source, Tile targetTile) => target switch
    {
        Target.SOURCETILE => source.CurrentTile.transform,
        Target.TARGETTILE => targetTile.transform,
        //Only the player's body has markers: an enemy's VFX starts from its tile
        _ => source.TryGetComponent(out PlayerAttack player) ? player.Anchor(target) : source.CurrentTile.transform,
    };
}
