using System.Collections.Generic;

public abstract class SingleTargetArtifact : Artifact
{
	public override bool CanTarget(Tile tile) {
        TacticsMove target = tile.GetEntity();
        return target != null && targets.Contains(target.tag);
    }

	public override void Launch(PlayerAttack source, Tile tile) {
        if (!CanTarget(tile)) return;
        ApplyCosts(source.Stats);
        ApplyEffects(source.Stats, tile.GetEntity().GetComponent<EntityStats>());
        PlayAnimation(source.CurrentTile, tile, source);
    }

    public override List<Tile> GetTargets(Tile targetedTile) {
        List<Tile> targetedTiles = new List<Tile>();
        if (targetedTile != null && targetedTile.Selection == Tile.SelectionType.ATTACK && CanTarget(targetedTile))
            targetedTiles.Add(targetedTile);
        return targetedTiles;
	}
}
