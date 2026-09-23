using System.Collections.Generic;

public abstract class AoeArtifact : Artifact {
    protected TileSearch area;

    public override bool CanTarget(Tile tile) {
		return true;
	}

	public override void Launch(PlayerAttack source, Tile tile) {
        ApplyCosts(source.Stats);

        ApplyEffectOnCast(source.Stats);
        foreach (Tile target in GetTargets(tile)) {
            TacticsMove entity = target.GetEntity();
            if (entity == null || !targets.Contains(entity.tag))
                continue;
            ApplyEffects(source.Stats, entity.GetComponent<EntityStats>());
		}

        PlayAnimation(source.CurrentTile, tile, source);
    }

    /// <summary>
    /// Sets the tile search used to find the tiles hit around the targeted tile, and its range
    /// </summary>
    protected void SetArea(TileSearch tileSearch, int min, int max) {
        area = tileSearch;
        minArea = min;
        maxArea = max;
        area.SetRange(min, max);
    }

    /// <summary>
    /// Effects that must be applied exactly once, not taking the number of targets into account
    /// </summary>
    /// <param name="source"></param>
    protected virtual void ApplyEffectOnCast(EntityStats source) { }

	public override List<Tile> GetTargets(Tile targetedTile) {
        if (targetedTile == null || targetedTile.Selection != Tile.SelectionType.ATTACK) return new List<Tile>();
        area.SetStartingTile(targetedTile);
        area.Search();
        return area.GetTiles();
    }
}
