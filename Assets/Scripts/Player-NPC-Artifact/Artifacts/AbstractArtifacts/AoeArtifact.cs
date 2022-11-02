using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoeArtifact : Artifact {
    protected AreaInfo area = new AreaInfo(0, 0, AreaType.CIRCLE);

    public override bool CanTarget(Tile tile) {
		return true;
	}

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        ApplyCosts(source);

        Vector3 VFXposition = tile.transform.position;
        VFXposition.y += 2;
        animator.SetTrigger("attacking");

        //StartCoroutine(LaunchFXAndAnim(animator, position));
        if (Prefab != null)
            GameObject.Instantiate(this.Prefab, VFXposition, Quaternion.identity);
        foreach (Tile target in GetTargets(tile)) {
            EntityStats entity = target.GetEntity();
            if (entity == null || !targets.Contains(entity.tag)) continue;
            ApplyEffects(source, target.GetEntity());
		}
    }

	public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || !targetedTile.isSelectable) return new List<Tile>();
        switch (area.type) {
            case AreaType.CIRCLE:
                return new CircleTileSearch(targetedTile, area.minRange, area.maxRange).GetTiles();
            case AreaType.CROSS:
                return new List<Tile>();
                return new LineTileSearch(targetedTile, area.maxRange, area.minRange).GetTiles();
            default:
                return new List<Tile>();
        }
    }
}
