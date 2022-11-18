using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleTargetArtifact : Artifact
{	
	public override bool CanTarget(Tile tile) {
        EntityStats target = tile.GetEntity();
        return target != null && targets.Contains(target.tag);
    }

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        if (!CanTarget(tile)) return;
        ApplyCosts(source);
        EntityStats target = tile.GetEntity();

        PlayAnimation(source.GetComponent<TacticsMove>().CurrentTile, tile, animator);
        ApplyEffects(source, target.GetComponentInParent<EntityStats>());
    }

    public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || !targetedTile.isSelectable || targetedTile.GetEntity() == null || !targets.Contains(targetedTile.GetEntity().tag)) return new List<Tile>();
        List<Tile> targetedTiles = new List<Tile>();
        targetedTiles.Add(targetedTile);
        return targetedTiles;
	}
}