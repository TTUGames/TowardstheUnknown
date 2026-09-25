using System.Linq;
using System.Reflection;
using UnityEngine;

// Play mode probes, run by playtest.sh through the unity CLI (run_script, compiled in memory against the live game).
// Each public static method returns a line describing what it did or saw.

/// <summary>
/// The state of the run: combat, turns, player, room, run stats, enemies
/// </summary>
public static class Probe
{
    const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    public static string Status()
    {
        TurnSystem turns = TurnSystem.Instance;
        PlayerTurn player = GameScene.Player;
        Room room = Object.FindAnyObjectByType<Room>();
        FieldInfo vfx = typeof(TransitionTile).GetField("vfx", Private);
        string exits = room == null ? "-" : string.Join(",", room.Exits.Select(e => e.direction + ":" + ((vfx.GetValue(e) as GameObject)?.activeSelf.ToString() ?? "novfx")));
        string playerState = player == null ? "dead" : $"{player.Stats.CurrentHealth}/{player.Stats.MaxHealth} armor {player.Stats.Armor} energy {player.Stats.CurrentEnergy}";
        RunStats run = GameScene.Run;
        return $"combat={turns.IsCombat} turns={turns.Turns.Count} playerTurn={(turns.IsCombat ? turns.IsPlayerTurn.ToString() : "-")} busy={ActionManager.IsBusy} " +
            $"player={playerState} room={(room != null ? room.name + " " + room.type : "none")} exitsOpen={exits} " +
            $"collectables={Object.FindObjectsByType<Collectable>(FindObjectsInactive.Exclude).Length} score={run?.Score} rooms={run?.VisitedRoomCount} " +
            $"enemies={string.Join(",", Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude).Select(e => e.ID + ":" + e.CurrentHealth))}";
    }

    /// <summary>
    /// The player's tile, mode and the tiles shown on the board
    /// </summary>
    public static string Player()
    {
        PlayerTurn player = GameScene.Player;
        var shown = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).GroupBy(t => t.Selection).Where(g => g.Key != Tile.SelectionType.NONE)
            .Select(g => g.Key + "=" + g.Count());
        return $"tile={Pointer.Describe(player.GetComponent<TacticsMove>().CurrentTile)} attacking={player.IsAttacking} energy={player.Stats.CurrentEnergy} " +
            $"statuses=[{string.Join(",", player.Stats.StatusEffects.Select(s => s.Data.name + ":" + s.Duration))}] shown=[{string.Join(",", shown)}]";
    }
}

/// <summary>
/// Turns and combat
/// </summary>
public static class Combat
{
    /// <summary>
    /// Ends the deploy phase of a combat room
    /// </summary>
    public static string Deploy()
    {
        var deploy = Object.FindAnyObjectByType<CombatPlayerDeploy>();
        if (deploy == null) return "no deploy phase";
        deploy.EndDeployPhase();
        return "deployed";
    }

    public static string EndTurn()
    {
        TurnSystem.Instance.EndPlayerTurn();
        return "turn ended";
    }

    /// <summary>
    /// Queues damage from the player on every enemy (999 kills them all)
    /// </summary>
    public static string HitAll(int damage)
    {
        foreach (EnemyStats enemy in Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude))
            ActionManager.AddToBottom(new DamageAction(GameScene.Player.Stats, enemy, damage, damage));
        return "queued";
    }

    public static string Artifacts() =>
        string.Join(" ", GameScene.Player.Inventory.GetPlayerArtifacts().Select((a, i) => $"{i}:{a.ID}(cost {a.Cost}, usable {a.CanUse(GameScene.Player.Stats)})"));

    /// <summary>
    /// Selects the artifact and casts it on the first valid tile of its range
    /// </summary>
    public static string Cast(int index)
    {
        PlayerTurn player = GameScene.Player;
        player.SetState(PlayerTurn.PlayerState.ATTACK, index);
        Artifact artifact = player.playerAttack.currentArtifact;
        if (!player.IsAttacking) return "cannot use " + artifact?.ID;
        Tile tile = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).FirstOrDefault(t => t.Selection == Tile.SelectionType.ATTACK && artifact.CanTarget(t));
        if (tile == null) return "no valid tile in range for " + artifact.ID;
        player.playerAttack.Attack(tile);
        return $"cast {artifact.ID} on {tile.GetEntity()?.name ?? "empty tile"}";
    }

    /// <summary>
    /// Queues a status effect on the player, by asset name (AttackUp, DefenseDown...)
    /// </summary>
    public static string ApplyStatus(string status, int duration)
    {
#if UNITY_EDITOR
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<StatusEffectData>($"Assets/Data/StatusEffects/{status}.asset");
        ActionManager.AddToBottom(new ApplyStatusAction(GameScene.Player.Stats, data, duration));
        return "queued " + status;
#else
        return "editor only";
#endif
    }
}

/// <summary>
/// The pointer on the board, raised through the room's tile events
/// </summary>
public static class Pointer
{
    static void Raise(string eventName, Tile tile)
    {
        var field = typeof(Room).GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static);
        (field.GetValue(null) as System.Action<Tile>)?.Invoke(tile);
    }

    public static string Describe(Tile tile) => tile == null ? "none" : $"({tile.transform.position.x:0},{tile.transform.position.z:0})";

    /// <summary>
    /// Hovers and clicks the nth tile of a selection (MOVEMENT, ATTACK, DEPLOY) by distance to the player, counted from the farthest if negative
    /// </summary>
    public static string Click(string selection, int n)
    {
        var type = (Tile.SelectionType)System.Enum.Parse(typeof(Tile.SelectionType), selection);
        Vector3 player = GameScene.Player.transform.position;
        var tiles = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).Where(t => t.Selection == type)
            .OrderBy(t => Vector3.Distance(t.transform.position, player)).ToList();
        if (tiles.Count == 0) return "no " + selection + " tile";
        Tile tile = n >= 0 ? tiles[Mathf.Min(n, tiles.Count - 1)] : tiles[Mathf.Max(0, tiles.Count + n)];
        Raise("TileHovered", tile);
        Raise("TileClicked", tile);
        return $"clicked {Describe(tile)} among {tiles.Count} {selection}";
    }

    /// <summary>
    /// Clicks an exit of the room (NORTH, EAST, SOUTH, WEST, or ANY): the player walks to it and changes room out of combat
    /// </summary>
    public static string ClickExit(string direction)
    {
        var exit = Object.FindObjectsByType<TransitionTile>(FindObjectsInactive.Exclude).FirstOrDefault(e => direction == "ANY" || e.direction.ToString() == direction);
        if (exit == null) return "no exit " + direction;
        Tile tile = exit.GetComponent<Tile>();
        Raise("TileHovered", tile);
        Raise("TileClicked", tile);
        return $"clicked exit {exit.direction} ({tile.Selection})";
    }
}

/// <summary>
/// The inventory screen and the chests
/// </summary>
public static class Bag
{
    static TetrisInventoryData Shown(TetrisInventory view) =>
        (TetrisInventoryData)typeof(TetrisInventory).GetField("data", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);

    static string Grid(TetrisInventoryData data) => $"{data.Items.Count} items [{string.Join(",", data.Items.Select(i => i.itemData.ID + "@" + i.slot))}]";

    public static string Show()
    {
        InventoryScreen screen = GameScene.UI.Inventory;
        return $"open={screen.IsOpen} player: {Grid(Shown(screen.PlayerInventory))} chest: {Grid(Shown(screen.Chest))}";
    }

    public static string Toggle()
    {
        GameScene.UI.Inventory.Toggle();
        return Show();
    }

    /// <summary>
    /// Picks up the room's collectable, opening its chest grid
    /// </summary>
    public static string PickUp()
    {
        var collectable = Object.FindAnyObjectByType<Collectable>();
        if (collectable == null) return "no collectable";
        typeof(Collectable).GetMethod("TryPickUp", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(collectable, null);
        return Show();
    }

    /// <summary>
    /// Moves the first item of the chest to the first free slot of the player's grid, as a drag and drop would
    /// </summary>
    public static string TakeFromChest()
    {
        InventoryScreen screen = GameScene.UI.Inventory;
        TetrisInventoryData chest = Shown(screen.Chest);
        if (chest.Items.Count == 0) return "empty chest";
        TetrisInventoryItem item = chest.Items[0];
        if (!GameScene.Player.Inventory.Data.FindSlotForItem(item, out Vector2Int slot)) return "no room for " + item.itemData.ID;
        screen.Chest.RemoveItem(item);
        screen.PlayerInventory.AddItem(slot, item);
        return Show();
    }
}

/// <summary>
/// Rooms
/// </summary>
public static class World
{
    /// <summary>
    /// Moves to the adjacent room at once (NORTH, EAST, SOUTH, WEST)
    /// </summary>
    public static string Move(string direction)
    {
        GameScene.Map.MoveToAdjacentRoom((Direction)System.Enum.Parse(typeof(Direction), direction));
        return "moving " + direction;
    }
}
