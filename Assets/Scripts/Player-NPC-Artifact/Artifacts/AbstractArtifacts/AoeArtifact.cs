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
        foreach (Tile t in tile.GetTilesWithinDistance(range.maxRange, range.minRange)) {
            EntityStats entity = t.GetEntity();
            if (entity == null || !targets.Contains(entity.tag)) continue;
            ApplyEffects(source, t.GetEntity());
		}
    }

	public override List<Tile> GetTargets() {
        Tile targetedTile = Tile.GetHoveredTile();
        if (targetedTile == null || !targetedTile.isSelectable) return new List<Tile>();
        switch (area.type) {
            case AreaType.CIRCLE:
                return targetedTile.GetTilesWithinDistance(area.maxRange, area.minRange);
            case AreaType.CROSS:
                return targetedTile.GetAlignedTilesWithinDistance(area.maxRange, area.minRange);
            default:
                return new List<Tile>();
        }
    }
}
