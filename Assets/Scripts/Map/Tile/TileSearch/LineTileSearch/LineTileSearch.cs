using UnityEngine;

/// <summary>
/// Gets all the tiles aligned and within a distance from the target
/// </summary>
public class LineTileSearch : TileSearch
{
	private static readonly Vector3[] directions = { Vector3.forward, Vector3.right, Vector3.back, Vector3.left };

	public LineTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) { }

	public override void Search() {
		Clear();
		if (minRange == 0) tiles.Add(startingTile.tile, startingTile);

		foreach (Vector3 direction in directions) {
			TileWrapper previousTile = startingTile;
			while (previousTile.distance < maxRange && previousTile.tile.lAdjacent.TryGetValue(direction, out Tile nextTile)) {
				TileWrapper currentTile = new TileWrapper(nextTile, previousTile.tile, previousTile.distance + 1);
				if (currentTile.distance >= minRange && IsValidTile(currentTile.tile)) {
					tiles.Add(currentTile.tile, currentTile);
				}
				if (!IsValidPath(currentTile.tile)) break;
				previousTile = currentTile;
			}
		}
	}
}
