using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleTileSearch : TileSearch
{
    private Tile startingTile;
	private int minDistance;
	private int maxDistance;

	private Dictionary<Tile, TileWrapper> tiles;

    protected List<TileConstraint> pathConstraints;
    protected List<TileConstraint> tileConstraints;

    /// <summary>
    /// Gets all the tiles within current distance from target tile
    /// </summary>
    /// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
    /// <param name="startingTile"></param>
    /// <param name="maxDistance"></param>
    /// <param name="minDistance"></param>
    public CircleTileSearch(Tile startingTile, int minDistance, int maxDistance) {
        this.startingTile = startingTile;
		this.minDistance = minDistance;
		this.maxDistance = maxDistance;

        Search();
	}

	private void Search() {
        tiles = new Dictionary<Tile, TileWrapper>();
        List<Tile> visitedTiles = new List<Tile>();
        Queue<TileWrapper> process = new Queue<TileWrapper>(); //First In First Out

        process.Enqueue(new TileWrapper(startingTile, null, 0));
        visitedTiles.Add(startingTile);

        while (process.Count > 0) {
            TileWrapper currentTile = process.Dequeue();

            Debug.Log("aaaaa");
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
        while (target != startingTile) {
            path.Push(target);
            target = tiles[target].parent;
		}
        return path;
	}

	public int GetDistance(Tile target) {
        return tiles[target].distance;
	}
}
