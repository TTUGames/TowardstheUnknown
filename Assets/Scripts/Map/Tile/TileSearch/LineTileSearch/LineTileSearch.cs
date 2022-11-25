using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets all the tiles aligned and within a distance from the target
/// </summary>
public class LineTileSearch : TileSearch
{
	protected List<Vector3> directions;

	protected List<TileConstraint> tileConstraints;
	protected List<TileConstraint> pathConstraints;

	public LineTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) {
		SetStartingTile(startingTile);
		SetRange(minRange, maxRange);

		SetConstraints();
		SetDirections();

		tiles = new Dictionary<Tile, TileWrapper>();
	}

	protected virtual void SetConstraints() {
		pathConstraints = new List<TileConstraint>();
		tileConstraints = new List<TileConstraint>();
		tileConstraints.Add(new WalkableTileConstraint());
	}

	protected virtual void SetDirections() {
		directions = new List<Vector3>();
		directions.Add(Vector3.forward);
		directions.Add(Vector3.right);
		directions.Add(Vector3.back);
		directions.Add(Vector3.left);
	}

	public override void Search() {
		Clear();
		if (minRange == 0) tiles.Add(startingTile.tile, startingTile);

		foreach (Vector3 direction in directions) {
			TileWrapper previousTile = startingTile;
			TileWrapper currentTile;
			while (previousTile.distance < maxRange && previousTile.tile.lAdjacent.ContainsKey(direction)) {
				currentTile = new TileWrapper(previousTile.tile.lAdjacent[direction], previousTile.tile, previousTile.distance + 1);
				if (!TileConstraint.CheckTileConstraints(pathConstraints, startingTile.tile, currentTile.tile)) break;
				if (currentTile.distance >= minRange && TileConstraint.CheckTileConstraints(tileConstraints, startingTile.tile, currentTile.tile)) {
					tiles.Add(currentTile.tile, currentTile);
				}
				previousTile = currentTile;
			}
		}
	}
}
