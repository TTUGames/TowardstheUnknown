using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Gives a recorded command (<see cref="GameCommand"/>) to the gameplay, through the same entry points as the player's
/// input, and tells when the game can take it: the replay (<see cref="CommandReplay"/>) pushes a journal through it
/// </summary>
public static class PlayerCommands
{
    /// <summary>
    /// Whether the game is where the command was given: its room, the action queue free, and for a combat command the
    /// player's turn of the same number; a deploy, the deploy phase; an exit, that exit open
    /// </summary>
    public static bool Ready(GameCommand command)
    {
        Map map = GameScene.Map;
        if (map == null || map.CurrentRoom == null || GameFlow.IsLoading || ActionManager.IsBusy) return false;
        if (map.CurrentRoomPosition != command.room) return false;
        TurnSystem turns = TurnSystem.Instance;
        switch (command.type)
        {
            case CommandType.Deploy:
                return PlayerDeploy.InDeployPhase && !GameScene.UI.Fade.IsCovered;
            case CommandType.Move:
            case CommandType.Cast:
            case CommandType.EndTurn:
                return turns.IsCombat && turns.IsPlayerTurn && CommandLog.PlayerTurn == command.turn;
            case CommandType.Exit:
                return !turns.IsCombat && !GameScene.Player.playerMove.IsMapTransitioning
                    && map.CurrentRoom.Exits.Any(exit => exit.direction == command.direction && exit.IsOpen);
            case CommandType.JumpTo:
                return !turns.IsCombat && !GameScene.Player.playerMove.IsMapTransitioning;
            default:
                return true;
        }
    }

    /// <summary>
    /// Plays the command now: false if its tile or artifact isn't there (the run went another way)
    /// </summary>
    public static bool Execute(GameCommand command)
    {
        PlayerTurn player = GameScene.Player;
        Room room = GameScene.Map.CurrentRoom;
        Tile tile = room.TileAt(command.cell);
        switch (command.type)
        {
            case CommandType.Deploy:
                if (tile == null || !room.TryGetComponent(out PlayerDeploy deploy)) return false;
                deploy.Choose(tile);
                deploy.EndDeployPhase();
                return true;
            case CommandType.Move:
                if (tile == null || tile.Selection != Tile.SelectionType.MOVEMENT) return false;
                player.Move();
                player.playerMove.OnTileClicked(tile);
                return true;
            case CommandType.Cast:
                if (tile == null || command.artifact >= player.Inventory.GetPlayerArtifacts().Count) return false;
                player.Aim(command.artifact);
                if (!player.playerAttack.currentArtifact.CanTarget(tile)) return false;
                player.playerAttack.Attack(tile);
                return true;
            case CommandType.EndTurn:
                TurnSystem.Instance.EndPlayerTurn();
                return true;
            case CommandType.Exit:
                GameScene.Map.MoveToAdjacentRoom(command.direction);
                return true;
            case CommandType.JumpTo:
                GameScene.Map.JumpTo(command.target);
                return true;
            case CommandType.Inventory:
                ApplyGrid(player.Inventory, command.items);
                return true;
        }
        return false;
    }

    /// <summary>
    /// Lays the player's grid out as recorded, keeping the artifacts it holds (their cooldowns and uses) and making the
    /// others from the catalog
    /// </summary>
    private static void ApplyGrid(InventoryManager inventory, List<RunSave.Item> items)
    {
        TetrisInventoryData data = inventory.Data;
        var kept = data.Items.Select(item => item.itemData).ToList();
        foreach (TetrisInventoryItem item in data.Items.ToList()) data.RemoveItem(item);
        data.Place(items, id =>
        {
            Artifact held = kept.FirstOrDefault(artifact => artifact.ID == id);
            if (held != null) kept.Remove(held);
            return held ?? inventory.Create(id);
        });
    }
}
