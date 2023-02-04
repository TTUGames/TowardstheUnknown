using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets all the tiles within current distance from target tile
/// </summary>
/// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
public class CircleTileSearch : TileSearch
{
    protected List<TileConstraint> pathConstraints;
    protected List<TileConstraint> tileConstraints;

    public CircleTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) {
        SetStartingTile(startingTile);
		SetRange(minRange, maxRange);

        SetConstraints();

        tiles = new Dictionary<Tile, TileWrapper>();
	}

    protected virtual void SetConstraints() {
        this.pathConstraints = new List<TileConstraint>();
        this.tileConstraints = new List<TileConstraint>();
        tileConstraints.Add(new WalkableTileConstraint());
    }

	public override void Search() {
        Clear();
        List<Tile> visitedTiles = new List<Tile>();
        Queue<TileWrapper> process = new Queue<TileWrapper>(); //First In First Out

        process.Enqueue(startingTile);
        visitedTiles.Add(startingTile.tile);

        while (process.Count > 0) {
            TileWrapper currentTile = process.Dequeue();

            if (currentTile.distance >= minRange && TileConstraint.CheckTileConstraints(tileConstraints, startingTile.tile, currentTile.tile)) {
                tiles.Add(currentTile.tile, currentTile);
            }

            if (currentTile.distance < maxRange && TileConstraint.CheckTileConstraints(pathConstraints, startingTile.tile, currentTile.tile)) {
                foreach (Tile tile in currentTile.tile.lAdjacent.Values) {
                    if (!visitedTiles.Contains(tile)) {
                        visitedTiles.Add(tile);
                        process.Enqueue(new TileWrapper(tile, currentTile.tile, currentTile.distance + 1));
                    }
                }
            }
        }
    }
}
