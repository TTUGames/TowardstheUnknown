using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets all the tiles aligned and within a distance from the target
/// </summary>
public class LineTileSearch : TileSearch
{
	protected List<Vector3> directions;

	private TileWrapper startingTile;
	private int minDistance;
	private int maxDistance;

	protected List<TileConstraint> tileConstraints;
	protected List<TileConstraint> pathConstraints;

	private Dictionary<Tile, TileWrapper> tiles;

	public LineTileSearch(Tile startingTile, int minDistance, int maxDistance) {
		this.startingTile = new TileWrapper(startingTile, null, 0);
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;

		SetConstraints();

		SetDirections();
		Search();
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

	private void Search() {
		tiles = new Dictionary<Tile, TileWrapper>();
		if (minDistance == 0) tiles.Add(startingTile.tile, startingTile);

		foreach (Vector3 direction in directions) {
			TileWrapper previousTile = startingTile;
			TileWrapper currentTile;
			while (previousTile.distance < maxDistance && previousTile.tile.lAdjacent.ContainsKey(direction)) {
				currentTile = new TileWrapper(previousTile.tile.lAdjacent[direction], previousTile.tile, previousTile.distance + 1);
				if (!TileConstraint.CheckTileConstraints(pathConstraints, startingTile.tile, currentTile.tile)) break;
				if (currentTile.distance >= minDistance && TileConstraint.CheckTileConstraints(tileConstraints, startingTile.tile, currentTile.tile)) {
					tiles.Add(currentTile.tile, currentTile);
				}
				previousTile = currentTile;
			}
		}
	}

	public int GetDistance(Tile target) {
		return tiles[target].distance;
	}

	public Stack<Tile> GetPath(Tile target) {
		Stack<Tile> path = new Stack<Tile>();
		Tile currentTile = target;
		while (currentTile != startingTile.tile) {
			path.Push(currentTile);
			currentTile = tiles[currentTile].parent;
		}
		return path;
	}

	public List<Tile> GetTiles() {
		return new List<Tile>(tiles.Keys);
	}
}
