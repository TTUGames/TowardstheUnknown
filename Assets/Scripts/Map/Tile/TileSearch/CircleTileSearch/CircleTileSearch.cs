using System.Collections.Generic;

/// <summary>
/// Gets all the tiles within current distance from target tile
/// </summary>
/// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
public class CircleTileSearch : TileSearch
{
    public CircleTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) { }

	public override void Search() {
        Clear();
        HashSet<Tile> visitedTiles = new HashSet<Tile>() { startingTile.tile };
        Queue<TileWrapper> process = new Queue<TileWrapper>(); //First In First Out
        process.Enqueue(startingTile);

        while (process.Count > 0) {
            TileWrapper currentTile = process.Dequeue();

            if (currentTile.distance >= minRange && IsValidTile(currentTile.tile)) {
                tiles.Add(currentTile.tile, currentTile);
            }

            if (currentTile.distance < maxRange && IsValidPath(currentTile.tile)) {
                foreach (Tile tile in currentTile.tile.lAdjacent.Values) {
                    if (visitedTiles.Add(tile))
                        process.Enqueue(new TileWrapper(tile, currentTile.tile, currentTile.distance + 1));
                }
            }
        }
    }
}
