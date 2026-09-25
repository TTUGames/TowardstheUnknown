/// <summary>
/// A way the player uses the board: moving or attacking. <c>PlayerTurn</c> enters one at a time and forwards it the tile events
/// </summary>
public interface IPlayerMode
{
    /// <summary>
    /// Shows the tiles the mode can use
    /// </summary>
    void Enter();

    /// <summary>
    /// Clears the tiles shown by the mode
    /// </summary>
    void Exit();

    void OnTileHovered(Tile tile);

    void OnTileClicked(Tile tile);
}
