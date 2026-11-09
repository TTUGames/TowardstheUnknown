// Plays a run by itself through the game's own entry points, so that every decision lands in the command journal and the
// run replays (CommandReplay): deploys, casts the first artifact that reaches an enemy, shields itself (Shield, Bastion,
// Barrier) when an enemy is close, keeps about 4 m from the nearest enemy, ends its turn, empties the chests into the grid,
// takes exits, then F8's jump to Drareg (Map.JumpTo). Ticks in EditorApplication.update; its state lives in the AppDomain,
// each script run being a new assembly. Start(rooms, goBoss, give) ; Status ; Stop. "give" replaces the grid at the start
// (asset names, recorded as an Inventory command): a weak kit dies before Drareg.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Bot
{
    const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;
    static AppDomain D => AppDomain.CurrentDomain;

    class State
    {
        public int roomsWanted; public bool goBoss; public string give;
        public int roomsDone; public bool given; public bool jumped;
        public double waitUntil; public int movedTurn = -1; public int castsThisTurn; public int turnSeen = -1;
        public Direction cameFrom = Direction.NULL; public string lastRoom = "";
        public HashSet<string> looted = new HashSet<string>();
        public List<string> log = new List<string>(); public string status = "starting"; public bool done;
        public System.Random rng = new System.Random(7);
        public double started;
    }

    public static string Start(int rooms, bool goBoss, string give)
    {
        Stop();
        var s = new State { roomsWanted = rooms, goBoss = goBoss, give = give, started = EditorApplication.timeSinceStartup };
        EditorApplication.CallbackFunction tick = null;
        tick = () =>
        {
            if (!EditorApplication.isPlaying || s.done) { EditorApplication.update -= tick; if (!s.done) { s.status = "stopped: play mode left"; s.done = true; } return; }
            try { Tick(s); } catch (Exception e) { s.log.Add("exception " + e.GetType().Name + ": " + e.Message); s.waitUntil = Now + 1; }
        };
        D.SetData("bot.state", s); D.SetData("bot.tick", tick);
        EditorApplication.update += tick;
        return "bot started";
    }

    public static string Status()
    {
        if (!(D.GetData("bot.state") is object o)) return "no bot";
        var t = o.GetType();
        var log = (List<string>)t.GetField("log").GetValue(o);
        return $"{t.GetField("status").GetValue(o)} | rooms {t.GetField("roomsDone").GetValue(o)} | {(EditorApplication.timeSinceStartup - (double)t.GetField("started").GetValue(o)):F0}s | last: {string.Join(" / ", log.Skip(Math.Max(0, log.Count - 6)))}";
    }

    public static string Stop()
    {
        if (D.GetData("bot.tick") is EditorApplication.CallbackFunction tick) EditorApplication.update -= tick;
        if (D.GetData("bot.state") is object o) o.GetType().GetField("done").SetValue(o, true);
        D.SetData("bot.tick", null);
        return "stopped";
    }

    static double Now => EditorApplication.timeSinceStartup;

    static void Log(State s, string line) { s.log.Add(line); if (s.log.Count > 200) s.log.RemoveAt(0); }

    static void Tick(State s)
    {
        if (Now < s.waitUntil) return;
        s.waitUntil = Now + 0.3;
        Map map = GameScene.Map; PlayerTurn player = GameScene.Player; TurnSystem turns = TurnSystem.Instance;
        if (map == null || turns == null || map.CurrentRoom == null) { s.status = "waiting for the game"; return; }
        if (player == null || player.Stats.CurrentHealth <= 0) { Finish(s, "player dead"); return; }
        if (ActionManager.IsBusy) return;
        Room room = map.CurrentRoom;
        if (room.name != s.lastRoom) { s.lastRoom = room.name; Log(s, "room " + room.name + " " + room.type); s.waitUntil = Now + 3; return; }
        var enemies = Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude).Where(e => e.CurrentHealth > 0).ToList();

        if (!s.given && !string.IsNullOrEmpty(s.give) && !turns.IsCombat) { Give(s, s.give); s.given = true; return; }

        if (!turns.IsCombat)
        {
            if (enemies.Count > 0)
            {
                var deploy = Object.FindAnyObjectByType<CombatPlayerDeploy>();
                if (deploy != null) { deploy.EndDeployPhase(); Log(s, "deployed"); s.waitUntil = Now + 2; }
                s.status = "deploy";
                return;
            }
            if (room.type == RoomType.BOSS && s.jumped) { Finish(s, "victory: Drareg dead"); return; }
            if (GameScene.UI.Inventory.IsOpen) { GameScene.UI.Inventory.Toggle(); s.waitUntil = Now + 1; return; }
            var collectable = Object.FindAnyObjectByType<Collectable>();
            if (collectable != null && s.looted.Add(room.name + collectable.GetInstanceHash()))
            {
                typeof(Collectable).GetMethod("TryPickUp", Private).Invoke(collectable, null);
                s.waitUntil = Now + 1.5; Log(s, "chest opened");
                return;
            }
            if (TakeOne(s)) { s.waitUntil = Now + 0.5; return; }
            if (s.roomsDone < s.roomsWanted)
            {
                var exits = room.Exits.Select(e => e.direction).ToList();
                if (exits.Count == 0) { Finish(s, "no exit"); return; }
                var forward = exits.Where(d => d != s.cameFrom).ToList();
                Direction to = (forward.Count > 0 ? forward : exits)[s.rng.Next((forward.Count > 0 ? forward : exits).Count)];
                s.cameFrom = Opposite(to); s.roomsDone++;
                map.MoveToAdjacentRoom(to); Log(s, "exit " + to); s.waitUntil = Now + 8; s.status = "moving";
                return;
            }
            if (s.goBoss && !s.jumped)
            {
                for (int x = 0; x < map.Rooms.Count; x++)
                    for (int y = 0; y < map.Rooms[x].Count; y++)
                        if (map.Rooms[x][y] != null && map.Rooms[x][y].GetRoomType() == RoomType.BOSS)
                        {
                            s.jumped = true; map.JumpTo(new Vector2Int(x, y)); Log(s, $"JumpTo boss ({x},{y})"); s.waitUntil = Now + 10; s.status = "to the boss";
                            return;
                        }
                Finish(s, "no boss room"); return;
            }
            Finish(s, "rooms done");
            return;
        }

        if (!turns.IsPlayerTurn) { s.status = "enemy turn"; return; }
        int turn = CommandLog.PlayerTurn;
        if (turn != s.turnSeen) { s.turnSeen = turn; s.castsThisTurn = 0; s.waitUntil = Now + 0.8; return; }
        s.status = $"player turn {turn}";
        if (player.playerMove != null && IsMoving(player)) return;

        // A cast on an enemy, the first artifact that can
        var artifacts = player.Inventory.GetPlayerArtifacts();
        if (s.castsThisTurn < 6)
            for (int i = 0; i < artifacts.Count; i++)
            {
                if (!artifacts[i].CanUse(player.Stats)) continue;
                player.SetState(PlayerTurn.PlayerState.ATTACK, i);
                if (!player.IsAttacking) continue;
                Artifact artifact = player.playerAttack.currentArtifact;
                Tile tile = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude)
                    .Where(t => t.Selection == Tile.SelectionType.ATTACK && artifact.CanTarget(t) && t.GetEntity() != null && t.GetEntity().GetComponent<EnemyStats>() != null)
                    .FirstOrDefault();
                if (tile == null) continue;
                player.playerAttack.Attack(tile);
                s.castsThisTurn++; Log(s, $"t{turn} cast {artifact.ID} on {tile.GetEntity()?.name}"); s.waitUntil = Now + 1;
                return;
            }

        // A shield on itself when an enemy is close
        for (int i = 0; i < artifacts.Count; i++)
        {
            if (!(artifacts[i].ID.Contains("Shield") || artifacts[i].ID.Contains("Bastion") || artifacts[i].ID.Contains("Barrier")) || !artifacts[i].CanUse(player.Stats)) continue;
            if (!enemies.Any(e => Vector3.Distance(e.transform.position, player.transform.position) < 3.5f)) break;
            player.SetState(PlayerTurn.PlayerState.ATTACK, i);
            if (!player.IsAttacking) break;
            Artifact shield = player.playerAttack.currentArtifact;
            Tile self = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude)
                .FirstOrDefault(t => t.Selection == Tile.SelectionType.ATTACK && shield.CanTarget(t) && t.GetEntity() == (TacticsMove)player.playerMove);
            if (self == null) break;
            player.playerAttack.Attack(self); s.castsThisTurn++; Log(s, $"t{turn} shield"); s.waitUntil = Now + 1;
            return;
        }

        // Once a turn, to the tile about 4 m from the nearest enemy: in range of the shots, out of the melee
        if (s.movedTurn != turn && enemies.Count > 0)
        {
            s.movedTurn = turn;
            player.SetState(PlayerTurn.PlayerState.MOVE);
            Vector3 p = player.transform.position;
            float Score(Vector3 at) => Mathf.Abs(enemies.Min(e => Vector3.Distance(e.transform.position, at)) - 4f);
            Tile best = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).Where(t => t.Selection == Tile.SelectionType.MOVEMENT)
                .OrderBy(t => Score(t.transform.position)).FirstOrDefault();
            if (best != null && Score(best.transform.position) < Score(p) - 0.5f)
            {
                Raise("TileHovered", best); Raise("TileClicked", best);
                Log(s, $"t{turn} move to keep 4 m"); s.waitUntil = Now + 2;
                return;
            }
        }

        turns.EndPlayerTurn(); Log(s, $"t{turn} end turn"); s.waitUntil = Now + 2;
    }

    static bool IsMoving(PlayerTurn player)
    {
        var f = typeof(TacticsMove).GetField("isMoving", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        var prop = typeof(TacticsMove).GetProperty("isMoving") ?? typeof(TacticsMove).GetProperty("IsMoving");
        if (f != null) return (bool)f.GetValue(player.playerMove);
        if (prop != null) return (bool)prop.GetValue(player.playerMove);
        return false;
    }

    static int GetInstanceHash(this Collectable c) => c.transform.position.GetHashCode();

    static bool TakeOne(State s)
    {
        InventoryScreen screen = GameScene.UI.Inventory;
        if (!screen.IsOpen) return false;
        var data = (TetrisInventoryData)typeof(TetrisInventory).GetField("data", Private).GetValue(screen.Chest);
        if (data == null || data.Items.Count == 0) return false;
        foreach (TetrisInventoryItem item in data.Items.ToList())
            if (GameScene.Player.Inventory.Data.FindSlotForItem(item, out Vector2Int slot))
            {
                screen.Chest.RemoveItem(item); screen.PlayerInventory.AddItem(slot, item);
                Log(s, "took " + item.itemData.ID); return true;
            }
        return false;
    }

    static void Give(State s, string names)
    {
        var datas = names.Split(',').Select(n => n.Trim()).Select(n => AssetDatabase.FindAssets($"t:ArtifactData {n}")
            .Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<ArtifactData>)
            .FirstOrDefault(a => a != null && a.name == n)).ToList();
        if (datas.Any(d => d == null)) { Log(s, "unknown artifact in " + names); return; }
        GameScene.Player.Inventory.Data.Replace(datas.Select(d => d.CreateArtifact()));
        Log(s, "given " + names);
    }

    static void Raise(string eventName, Tile tile)
    {
        var field = typeof(BoardPointer).GetField(eventName, BindingFlags.NonPublic | BindingFlags.Static);
        (field.GetValue(null) as Action<Tile>)?.Invoke(tile);
    }

    static Direction Opposite(Direction d) => d switch { Direction.NORTH => Direction.SOUTH, Direction.SOUTH => Direction.NORTH, Direction.EAST => Direction.WEST, Direction.WEST => Direction.EAST, _ => Direction.NULL };

    static void Finish(State s, string why) { s.status = "done: " + why; s.done = true; Log(s, why); }
}
