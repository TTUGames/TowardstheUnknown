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
        //On the manager, which outlives the caster: a caster dying during the delay still gets its VFX on the tiles
        ActionManager.Run(PlayDelayed(action, entity, entity.CurrentTile, targetTile));
	}

    private IEnumerator PlayDelayed(AttackAnimationAction action, TacticsMove source, Tile castTile, Tile targetTile) {
        //The delay is set on the clip: the attack's timing moves it with the pose
        float time = action.Clock.EventTime(delay);
        if (time > 0) yield return new WaitForSeconds(time);

        //A destroyed caster compares equal to null: its body's markers are gone, its tile is where it cast from
        bool alive = source != null;
        if (!alive && target != Target.SOURCETILE && target != Target.TARGETTILE) yield break;
        Tile sourceTile = alive ? source.CurrentTile : castTile;
        GameObject vfx = VFXPool.Get(prefab, alive ? Origin(target, source, targetTile) : target == Target.TARGETTILE ? targetTile.transform : castTile.transform);

        //Faces the target, or the source's facing on its own tile
        Vector3 VFXRotation = sourceTile != targetTile
            ? Vector3.up * ((-Vector3.SignedAngle(sourceTile.transform.position - targetTile.transform.position, Vector3.forward, Vector3.up) + rotationOffset) % 360)
            : (alive ? source.transform.rotation.eulerAngles : Vector3.zero) + Vector3.up * rotationOffset;

        action.AddVFX(vfx);
        if (!vfx.TryGetComponent(out ConstantRotation constantRotation)) constantRotation = vfx.AddComponent<ConstantRotation>();
        constantRotation.SetRotation(VFXRotation);
        vfx.transform.localPosition = offset;
    }

    /// <summary>
    /// The transform a VFX starts from: a marker of the player's body, or a tile
    /// </summary>
    public static Transform Origin(Target target, TacticsMove source, Tile targetTile) => target switch
    {
        Target.SOURCETILE => source.CurrentTile.transform,
        Target.TARGETTILE => targetTile.transform,
        //Only the player's body has markers: an enemy's VFX starts from its tile
        _ => source.TryGetComponent(out PlayerAttack player) ? player.Anchor(target) : source.CurrentTile.transform,
    };
}
