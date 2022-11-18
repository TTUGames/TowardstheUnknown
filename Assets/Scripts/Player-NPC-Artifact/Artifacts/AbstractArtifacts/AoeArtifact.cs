using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoeArtifact : Artifact {
    protected TileSearch area;

    public override bool CanTarget(Tile tile) {
		return true;
	}

	public override void Launch(PlayerStats source, Tile tile, Animator animator) {
        ApplyCosts(source);

        foreach (Tile target in GetTargets(tile)) {
            EntityStats entity = target.GetEntity();
            if (entity == null || !targets.Contains(entity.tag)) continue;
            ApplyEffects(source, target.GetEntity());
		}

        PlayAnimation(source.GetComponent<TacticsMove>().CurrentTile, tile, animator);
    }

	public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || !targetedTile.isSelectable) return new List<Tile>();
        area.SetStartingTile(targetedTile);
        area.Search();
        return area.GetTiles();
    }
}
