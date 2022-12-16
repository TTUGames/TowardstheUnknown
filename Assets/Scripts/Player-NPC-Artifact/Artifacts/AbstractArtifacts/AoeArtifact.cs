using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AoeArtifact : Artifact {
    protected TileSearch area;

    public override bool CanTarget(Tile tile) {
		return true;
	}

	public override void Launch(PlayerAttack source, Tile tile) {
        ApplyCosts(source.Stats);

        foreach (Tile target in GetTargets(tile)) {
            EntityStats entity = target.GetEntity();
            if (entity == null || !targets.Contains(entity.tag))
                continue;
            ApplyEffects(source.Stats, entity);
		}

        PlayAnimation(source.CurrentTile, tile, source);
    }

	public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || targetedTile.Selection != Tile.SelectionType.ATTACK) return new List<Tile>();
        area.SetStartingTile(targetedTile);
        area.Search();
        return area.GetTiles();
    }
}
