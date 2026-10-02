/// <summary>
/// What an exit leads to, as its <see cref="ExitPortal"/> shows it, in the colors of the minimap: a room visited and
/// emptied, a room where a relic lies (<see cref="RoomInfo.HasLoot"/>), or a room never entered without a known relic
/// </summary>
public enum ExitDestination
{
    VISITED, COMBAT, TREASURE
}
