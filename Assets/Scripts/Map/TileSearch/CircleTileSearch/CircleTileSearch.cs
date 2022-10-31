using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets all the tiles within current distance from target tile
/// </summary>
/// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
public class CircleTileSearch : TileSearch
{
    private Tile startingTile;
	private int minDistance;
	private int maxDistance;

	private Dictionary<Tile, TileWrapper> tiles;

    protected List<TileConstraint> pathConstraints;
    protected List<TileConstraint> tileConstraints;

    public CircleTileSearch(Tile startingTile, int minDistance, int maxDistance) {
        this.startingTile = startingTile;
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;

        SetConstraints();

        Search();
	}

    protected virtual void SetConstraints() {
        this.pathConstraints = new List<TileConstraint>();
        this.tileConstraints = new List<TileConstraint>();
        tileConstraints.Add(new WalkableTileConstraint());
    }

	private void Search() {
        tiles = new Dictionary<Tile, TileWrapper>();
        List<Tile> visitedTiles = new List<Tile>();
        Queue<TileWrapper> process = new Queue<TileWrapper>(); //First In First Out

        process.Enqueue(new TileWrapper(startingTile, null, 0));
        visitedTiles.Add(startingTile);

        while (process.Count > 0) {
            TileWrapper currentTile = process.Dequeue();

            if (currentTile.distance <= maxDistance && TileConstraint.CheckTileConstraints(pathConstraints, startingTile, currentTile.tile)) {
                if (currentTile.distance >= minDistance && TileConstraint.CheckTileConstraints(tileConstraints, startingTile, currentTile.tile)) {
                    tiles.Add(currentTile.tile, currentTile);
                }

                foreach (Tile tile in currentTile.tile.lAdjacent.Values) {
                    if (!visitedTiles.Contains(tile)) {
                        visitedTiles.Add(tile);
                        process.Enqueue(new TileWrapper(tile, currentTile.tile, currentTile.distance + 1));
                    }
                }
            }
        }
    }

	public List<Tile> GetTiles() {
        return new List<Tile>(tiles.Keys);
	}

	public Stack<Tile> GetPath(Tile target) {
        Stack<Tile> path = new Stack<Tile>();
        Tile currentTile = target;
        while (currentTile != startingTile) {
            path.Push(currentTile);
            currentTile = tiles[currentTile].parent;
		}
        return path;
	}

	public int GetDistance(Tile target) {
        return tiles[target].distance;
	}
}
