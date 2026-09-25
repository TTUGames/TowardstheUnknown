using UnityEngine;

/// <summary>
/// Access to the unique objects of the game scene, for the objects that cannot reference them from a serialized field:
/// those spawned at runtime (rooms, enemies, HUD items) and the actions.
/// Each object is looked up once per scene, then cached.
/// </summary>
public static class GameScene
{
    private static PlayerTurn player;
    private static ChangeUI ui;
    private static Map map;

    /// <summary>
    /// The player, null once dead
    /// </summary>
    public static PlayerTurn Player => player != null ? player : player = Object.FindAnyObjectByType<PlayerTurn>();

    /// <summary>
    /// The game UI, giving access to its menus, HUD and fade
    /// </summary>
    public static ChangeUI UI => ui != null ? ui : ui = Object.FindAnyObjectByType<ChangeUI>();

    /// <summary>
    /// The map, null in the debug scenes loading a single room
    /// </summary>
    public static Map Map => map != null ? map : map = Object.FindAnyObjectByType<Map>();
}
