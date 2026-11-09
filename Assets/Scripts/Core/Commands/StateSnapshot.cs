using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// The logical state of the game at a point of the run, nothing visual nor timed: the room, each entity of the turn order
/// (its ID, tile, health, armor, statuses and their turns, an enemy's own state its next turns depend on), the player's energy, grid and its artifacts' cooldowns and uses, and
/// where the random streams are (<see cref="GameRandom"/>). <see cref="Dump"/> writes it as JSON, <see cref="Hash"/> as a
/// short fingerprint. The journal records one at each end of the player's turn and of a combat (<see cref="CommandLog"/>):
/// two plays of the same journal must give the same ones, the first that differs saying what diverged
/// </summary>
[Serializable]
public class StateSnapshot
{
    [Serializable]
    public class Entity
    {
        public string id;
        public Vector2Int cell;
        public int health;
        public int armor;
        public List<string> statuses = new();
        // What an enemy's AI keeps that its next turns depend on (EnemyAI.LogicalState): Drareg's phase and ultimate countdown
        public string ai;
    }

    [Serializable]
    public class Item
    {
        public string artifact;
        public Vector2Int slot;
        public int rotation;
        public int cooldown;
        public int usesLeft;
    }

    public Vector2Int room;
    public bool combat;
    public int energy;
    public List<Entity> entities = new();
    public List<Item> grid = new();
    public List<string> randomStreams = new();

    /// <summary>
    /// The state now
    /// </summary>
    public static StateSnapshot Capture()
    {
        var state = new StateSnapshot();
        Map map = GameScene.Map;
        Room room = map != null ? map.CurrentRoom : null;
        TurnSystem turns = TurnSystem.Instance;
        state.room = map != null ? map.CurrentRoomPosition : Vector2Int.zero;
        state.combat = turns != null && turns.IsCombat;
        PlayerTurn player = GameScene.Player;
        if (player != null)
        {
            state.energy = player.Stats.CurrentEnergy;
            foreach (TetrisInventoryItem item in player.Inventory.Data.Items)
                state.grid.Add(new Item
                {
                    artifact = item.itemData.ID, slot = item.slot, rotation = item.rotation,
                    cooldown = item.itemData.RemainingCooldown, usesLeft = item.itemData.RemainingUsesThisTurn,
                });
        }
        if (turns != null)
            foreach (EntityTurn turn in turns.Turns)
            {
                if (turn == null || !turn.TryGetComponent(out EntityStats stats)) continue;
                state.entities.Add(new Entity
                {
                    id = stats.ID,
                    cell = room != null && stats.Tile != null ? room.CellOf(stats.Tile) : new Vector2Int(int.MinValue, int.MinValue),
                    health = stats.CurrentHealth,
                    armor = stats.Armor,
                    statuses = stats.StatusEffects.Select(status => status.Data.name + ":" + status.Duration).OrderBy(name => name, StringComparer.Ordinal).ToList(),
                    ai = turn is EnemyAI enemy ? enemy.LogicalState : "",
                });
            }
        state.randomStreams = GameRandom.Save().Select(stream => ((ulong)stream).ToString("x16")).ToList();
        return state;
    }

    /// <summary>
    /// The state as indented JSON, one value per line: two dumps compare line by line
    /// </summary>
    public string Dump() => JsonUtility.ToJson(this, true);

    /// <summary>
    /// A 64 bits fingerprint of the dump (FNV-1a), in hexadecimal
    /// </summary>
    public string Hash() => HashOf(Dump());

    public static string HashOf(string text)
    {
        ulong hash = 14695981039346656037UL;
        foreach (char character in text)
        {
            hash ^= character;
            hash *= 1099511628211UL;
        }
        return hash.ToString("x16");
    }

    /// <summary>
    /// The first line where two dumps differ, with its neighbours' context; null if they are the same
    /// </summary>
    public static string FirstDifference(string expected, string actual)
    {
        string[] a = expected.Split('\n'), b = actual.Split('\n');
        for (int i = 0; i < Mathf.Max(a.Length, b.Length); i++)
        {
            string left = i < a.Length ? a[i].Trim() : "(end)", right = i < b.Length ? b[i].Trim() : "(end)";
            if (left == right) continue;
            // The object the line belongs to: the last key opening one above it
            string context = "";
            for (int j = i - 1; j >= 0 && j < a.Length; j--)
                if (a[j].Trim().StartsWith("\"id\"") || a[j].Trim().StartsWith("\"artifact\"")) { context = a[j].Trim() + " "; break; }
            return $"line {i + 1}: {context}expected {left}, got {right}";
        }
        return null;
    }
}
