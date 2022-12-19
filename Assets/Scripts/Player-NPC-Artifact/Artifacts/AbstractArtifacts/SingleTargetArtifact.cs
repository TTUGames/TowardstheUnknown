using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SingleTargetArtifact : Artifact
{	
	public override bool CanTarget(Tile tile) {
        EntityStats target = tile.GetEntity();
        return target != null && targets.Contains(target.tag);
    }

	public override void Launch(PlayerAttack source, Tile tile) {
        if (!CanTarget(tile)) return;
        ApplyCosts(source.Stats);
        EntityStats target = tile.GetEntity();

        ApplyEffects(source.Stats, target.GetComponentInParent<EntityStats>());
        PlayAnimation(source.CurrentTile, tile, source);
    }

    public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || targetedTile.Selection != Tile.SelectionType.ATTACK || targetedTile.GetEntity() == null || !targets.Contains(targetedTile.GetEntity().tag))
            return new List<Tile>();
        List<Tile> targetedTiles = new List<Tile>();
        targetedTiles.Add(targetedTile);
        return targetedTiles;
	}
}