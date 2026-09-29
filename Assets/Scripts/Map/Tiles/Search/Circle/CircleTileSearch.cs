using System.Collections.Generic;

/// <summary>
/// Gets all the tiles within current distance from target tile
/// </summary>
/// <seealso cref="wikipedia :&#x20;" href="https://en.wikipedia.org/wiki/Breadth-first_search"/>
public class CircleTileSearch : TileSearch
{
    // Kept between searches: an aim or a threat preview runs many
    private readonly HashSet<Tile> visitedTiles = new HashSet<Tile>();
    private readonly Queue<TileWrapper> process = new Queue<TileWrapper>(); //First In First Out

    public CircleTileSearch(int minRange = 0, int maxRange = 0, Tile startingTile = null) : base(minRange, maxRange, startingTile) { }

	public override void Search() {
        Clear();
        visitedTiles.Clear();
        visitedTiles.Add(startingTile.tile);
        process.Clear();
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
