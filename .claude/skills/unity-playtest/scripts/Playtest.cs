using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;

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
    /// <summary>
    /// The combat readability layer: the grid (shown, fade, tiles) and each entity's ring (shown, fade, turn, hover, target)
    /// </summary>
    public static string Overlays()
    {
        CombatGrid grid = Object.FindAnyObjectByType<CombatGrid>();
        string gridState = "none";
        if (grid != null)
        {
            var filter = (MeshFilter)typeof(CombatGrid).GetField("meshFilter", Private).GetValue(grid);
            var renderer = (MeshRenderer)typeof(CombatGrid).GetField("meshRenderer", Private).GetValue(grid);
            int tiles = filter.sharedMesh == null ? 0 : filter.sharedMesh.vertexCount / 4;
            gridState = $"shown={typeof(CombatGrid).GetField("shown", Private).GetValue(grid)} fade={(float)typeof(CombatGrid).GetField("fade", Private).GetValue(grid):0.00} " +
                $"rendered={renderer.enabled} tiles={tiles}";
        }
        FieldInfo current = typeof(EntityRing).GetField("current", Private);
        FieldInfo ringRenderer = typeof(EntityRing).GetField("ringRenderer", Private);
        string rings = string.Join(" | ", Object.FindObjectsByType<EntityRing>(FindObjectsInactive.Exclude).Select(r => {
            float[] v = (float[])current.GetValue(r);
            return $"{r.name}: rendered={((MeshRenderer)ringRenderer.GetValue(r)).enabled} fade={v[0]:0.00} active={v[1]:0.00} hover={v[2]:0.00} targeted={v[3]:0.00}";
        }));
        return $"grid: {gridState} || rings: {rings}";
    }

    public static string Player()
    {
        PlayerTurn player = GameScene.Player;
        var shown = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).GroupBy(t => t.Selection).Where(g => g.Key != Tile.SelectionType.NONE)
            .Select(g => g.Key + "=" + g.Count());
        return $"tile={Pointer.Describe(player.GetComponent<TacticsMove>().CurrentTile)} attacking={player.IsAttacking} energy={player.Stats.CurrentEnergy} " +
            $"statuses=[{string.Join(",", player.Stats.StatusEffects.Select(s => s.Data.name + ":" + s.Duration))}] shown=[{string.Join(",", shown)}]";
    }

    /// <summary>
    /// The hover state: hovered tile and entity, target and threat tiles, the enemy info panel, the damage previews, the
    /// rings' hover and target, whether a UI element is under the pointer, and the outlined entities
    /// </summary>
    public static string Hover()
    {
        Tile[] tiles = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude);
        VisualElement root = Pointer.HudRoot();
        VisualElement info = root.Q("EntityInfo");
        string infoState = info.ClassListContains("shown") ? "shown " + root.Q<Label>("EntityName").text : "hidden";
        var previews = root.Query<Label>(className: "damage-preview").ToList()
            .Where(l => l.style.display == DisplayStyle.Flex).Select(l => l.text.Replace('\n', ' '));
        FieldInfo target = typeof(EntityRing).GetField("target", Private);
        string rings = string.Join(" ", Object.FindObjectsByType<EntityRing>(FindObjectsInactive.Exclude).Select(r => {
            float[] t = (float[])target.GetValue(r);
            return $"{r.name}:hover={t[2]:0} target={t[3]:0}";
        }));
        return $"tile={Pointer.Describe(Room.HoveredTile)} entity={(Room.HoveredEntity != null ? Room.HoveredEntity.name : "none")} " +
            $"attacking={GameScene.Player.IsAttacking} targetTiles={tiles.Count(t => t.IsTarget)} threatTiles={tiles.Count(t => t.IsThreat)} " +
            $"attackTiles={tiles.Count(t => t.Selection == Tile.SelectionType.ATTACK)} info={infoState} previews=[{string.Join(",", previews)}] " +
            $"outlined=[{string.Join(",", EntityOutline.Shown.Select(o => o.name))}] rings: {rings}";
    }
}

/// <summary>
/// The HUD tooltips (HudTooltip): shown or not, classes, text, position in the HUD and whether they fit on screen
/// </summary>
public static class Tooltips
{
    public static string Show()
    {
        VisualElement root = Pointer.HudRoot();
        Rect screen = root.worldBound;
        return string.Join(" || ", root.Query<HudTooltip>().ToList().Select(t => {
            Rect r = t.worldBound;
            bool inside = r.xMin >= screen.xMin - 0.5f && r.yMin >= screen.yMin - 0.5f && r.xMax <= screen.xMax + 0.5f && r.yMax <= screen.yMax + 0.5f;
            return $"{t.name}: shown={t.ClassListContains("shown")} opacity={t.resolvedStyle.opacity:0.00} classes=[{string.Join(" ", t.GetClasses())}] " +
                $"rect=({r.xMin:0},{r.yMin:0},{r.width:0}x{r.height:0}) of ({screen.width:0}x{screen.height:0}) onScreen={inside} text=\"{t.text.Replace("\n", " / ")}\"";
        }));
    }
}

/// <summary>
/// The menus covering the game
/// </summary>
public static class Menus
{
    /// <summary>
    /// Opens (true) or closes (false) the pause menu, as the Back key does
    /// </summary>
    public static string Pause(bool open)
    {
        GameScene.UI.uIPause.ToggleOptions(open);
        return $"paused={GameScene.UI.uIPause.isPaused} blocked={GameScene.IsGameplayBlocked}";
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
    /// Casts artifact n like <c>Cast</c> and logs, as "[timeline] +seconds event", the damage, deaths, removed entities and the end of the queue.
    /// Read them with <c>unity command console --level log</c> after the actions played
    /// </summary>
    public static string CastTimed(int index)
    {
        float start = Time.time;
        void Log(string message) => Debug.Log($"[timeline] +{Time.time - start:0.00}s {message}");
        System.Action<EntityStats, int, int> onDamage = (entity, amount, healthLost) => Log($"damage {amount} (health lost {healthLost}) on {entity.name}");
        System.Action<EntityStats> onDied = entity => Log($"died {entity.name}");
        EntityStats.AnyDamageTaken += onDamage;
        GameEvents.EntityDied += onDied;
        List<GameObject> enemies = Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude).Select(e => e.gameObject).ToList();
        string result = Cast(index);
        ActionManager.Run(Watch());
        return result;

        System.Collections.IEnumerator Watch()
        {
            bool queueEnded = false;
            while (Time.time - start < 10)
            {
                for (int i = enemies.Count - 1; i >= 0; i--)
                    if (enemies[i] == null) { Log("removed an enemy"); enemies.RemoveAt(i); }
                if (!queueEnded && !ActionManager.IsBusy) { Log("queue free"); queueEnded = true; }
                yield return null;
            }
            EntityStats.AnyDamageTaken -= onDamage;
            GameEvents.EntityDied -= onDied;
        }
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

    internal static VisualElement HudRoot() =>
        ((UIDocument)typeof(Hud).GetField("document", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(GameScene.UI.Hud)).rootVisualElement;

    // Queued as a mouse event for the game's next input update, as if the real mouse moved there: the game and the UI
    // Toolkit panels (pointer enter and leave) see it on the next frame. The probes run in the editor's input update,
    // where GameInput.PointerPosition doesn't read the game's mouse
    static void MoveMouse(Vector2 screen)
    {
        using (DeltaStateEvent.From(Mouse.current.position, out InputEventPtr eventPtr))
        {
            Mouse.current.position.WriteValueIntoEvent(screen, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
    }

    /// <summary>
    /// Moves the mouse over an entity's model, <paramref name="height"/> meters above its feet: "Player", or the nth enemy
    /// (by x position) whose ID starts with <paramref name="id"/> ("" for any). Read the result with <c>Probe.Hover</c> on the next call
    /// </summary>
    public static string HoverEntity(string id, int n, float height)
    {
        Transform entity = id == "Player" ? GameScene.Player.transform
            : Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude).Where(e => e.ID.StartsWith(id))
                .OrderBy(e => e.transform.position.x).Select(e => e.transform).ElementAtOrDefault(n);
        if (entity == null) return "no entity " + id + " " + n;
        Vector3 screen = Camera.main.WorldToScreenPoint(entity.position + Vector3.up * height);
        MoveMouse(screen);
        return $"mouse on {entity.name} at {height} m, screen ({screen.x:0},{screen.y:0}), its tile {Describe(entity.GetComponent<TacticsMove>().CurrentTile)}";
    }

    /// <summary>
    /// Moves the mouse over the center of a tile's top, by its position as <c>Describe</c> prints it
    /// </summary>
    public static string HoverTile(int x, int z)
    {
        Tile tile = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude)
            .FirstOrDefault(t => Describe(t) == $"({x},{z})");
        if (tile == null) return "no tile " + x + "," + z;
        Vector3 top = tile.transform.position; top.y = tile.GetComponent<Collider>().bounds.max.y;
        MoveMouse(Camera.main.WorldToScreenPoint(top));
        return "mouse on tile " + Describe(tile);
    }

    /// <summary>
    /// Moves the mouse over the nth item of the HUD's timeline (the turn order, player included)
    /// </summary>
    public static string HoverTimeline(int n)
    {
        VisualElement item = HudRoot().Q("Timeline").Query(className: "timeline-item").AtIndex(n);
        if (item == null) return "no timeline item " + n;
        Vector2 screen = MoveMouseTo(item, 0.5f);
        return $"mouse on timeline item {n} ({TurnSystem.Instance.Turns[n].name}) at screen ({screen.x:0},{screen.y:0})";
    }

    /// <summary>
    /// Moves the mouse over the nth HUD element matching <paramref name="query"/> (an element name, ".class" or a type
    /// name such as "HealthBar"), at <paramref name="x"/> (0 to 1) of its width, mid height; "none" moves it to the
    /// middle of the screen. Read the tooltips with <c>Probe.Tooltips</c> after the tooltip delay
    /// </summary>
    public static string HoverHud(string query, int n, float x)
    {
        if (query == "none")
        {
            MoveMouse(new Vector2(Screen.width / 2f, Screen.height / 2f));
            return "mouse in the middle of the screen";
        }
        VisualElement root = HudRoot();
        List<VisualElement> matches = query.StartsWith(".") ? root.Query(className: query.Substring(1)).ToList()
            : root.Query().Where(e => e.name == query || e.GetType().Name == query).ToList();
        VisualElement element = matches.ElementAtOrDefault(n);
        if (element == null) return "no HUD element " + query + " " + n;
        Vector2 screen = MoveMouseTo(element, x);
        return $"mouse on {query} {n} at {x:0.00} of its width, screen ({screen.x:0},{screen.y:0}), picked {HudRoot().panel.Pick(new Vector2(element.worldBound.xMin + element.worldBound.width * x, element.worldBound.center.y))?.GetType().Name}";
    }

    // Moves the mouse over a UI Toolkit element, at x (0 to 1) of its width and mid height
    static Vector2 MoveMouseTo(VisualElement element, float x)
    {
        IPanel panel = element.panel;
        // ScreenToPanel is affine (y from the top): invert it from two points
        Vector2 origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
        Vector2 scale = RuntimePanelUtils.ScreenToPanel(panel, Vector2.one) - origin;
        Rect bound = element.worldBound;
        var point = new Vector2(bound.xMin + bound.width * x, bound.center.y);
        var screen = new Vector2((point.x - origin.x) / scale.x, Screen.height - (point.y - origin.y) / scale.y);
        MoveMouse(screen);
        return screen;
    }

    /// <summary>
    /// Presses (true) or releases (false) the left mouse button where the mouse is: a press is a click for the game (Select)
    /// </summary>
    public static string Press(bool down)
    {
        // Queued for the game's next input update (the probes run in the editor's), only the button's bytes
        using (DeltaStateEvent.From(Mouse.current.leftButton, out InputEventPtr eventPtr))
        {
            Mouse.current.leftButton.WriteValueIntoEvent(down ? 1f : 0f, eventPtr);
            InputSystem.QueueEvent(eventPtr);
        }
        return (down ? "pressed" : "released") + " on " + Describe(Room.HoveredTile);
    }

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

/// <summary>
/// The game feel of the hits: time scale and hit stops, camera shake, hit flashes
/// </summary>
public static class Feel
{
    const BindingFlags Private = BindingFlags.NonPublic | BindingFlags.Instance;

    static string Camera(out float offset)
    {
        var impact = Object.FindAnyObjectByType<ImpactFeedback>();
        offset = 0;
        if (impact == null) return "no ImpactFeedback";
        var camera = (Transform)typeof(ImpactFeedback).GetField("shakenCamera", Private).GetValue(impact);
        var start = (Vector3)typeof(ImpactFeedback).GetField("startPosition", Private).GetValue(impact);
        var startRotation = (Quaternion)typeof(ImpactFeedback).GetField("startRotation", Private).GetValue(impact);
        float trauma = (float)typeof(ImpactFeedback).GetField("trauma", Private).GetValue(impact);
        offset = Vector3.Distance(camera.localPosition, start);
        return $"camera offset {offset * 100:0.00}cm roll {Quaternion.Angle(camera.localRotation, startRotation):0.00}deg trauma {trauma:0.00} shakeSetting {GameSettings.ScreenShake:0.00}";
    }

    static string Flashes() => $"flashes shown {HitFlash.Shown.Count} [{string.Join(",", HitFlash.Shown.Select(f => f.Amount.ToString("0.00")))}]";

    /// <summary>
    /// The time scale, the camera against its rest and the hit flashes shown
    /// </summary>
    public static string Show() =>
        $"timeScale {Time.timeScale:0.00} paused {GameTime.Paused} speed {GameTime.Speed:0.00} {Camera(out _)} {Flashes()}";

    /// <summary>
    /// Watches for real seconds and logs, as "[feel] ...", each hit (health lost, weight) and death, each hit stop (frozen real time),
    /// the strongest flash and camera offset until all is back to rest, then the state at the end. Read them with
    /// <c>unity --json command console --level log</c>
    /// </summary>
    public static string Watch(float seconds)
    {
        var impact = Object.FindAnyObjectByType<ImpactFeedback>();
        float start = Time.unscaledTime;
        void Log(string message) => Debug.Log($"[feel] +{Time.unscaledTime - start:0.000}s {message}");
        float maxFlash = 0, maxOffset = 0;
        bool reported = true;
        System.Action<EntityStats, int, int> onDamage = (entity, amount, healthLost) =>
        {
            if (!reported) Log($"  strongest flash {maxFlash:0.00} camera offset {maxOffset * 100:0.00}cm");
            Log($"hit {entity.name} damage {amount} health lost {healthLost} weight {(impact != null ? impact.HitWeight(entity, healthLost) : -1):0.00}");
            maxFlash = maxOffset = 0;
            reported = false;
        };
        System.Action<EntityStats> onDied = entity =>
        {
            Log($"died {entity.name}");
            reported = false;
        };
        EntityStats.AnyDamageTaken += onDamage;
        GameEvents.EntityDied += onDied;
        ActionManager.Run(Run());
        return $"watching {seconds}s";

        System.Collections.IEnumerator Run()
        {
            float frozenSince = -1;
            while (Time.unscaledTime - start < seconds)
            {
                bool frozen = Time.timeScale == 0 && !GameTime.Paused;
                if (frozen && frozenSince < 0) frozenSince = Time.unscaledTime;
                else if (!frozen && frozenSince >= 0)
                {
                    Log($"  hit stop {Time.unscaledTime - frozenSince:0.000}s, then timeScale {Time.timeScale:0.00}");
                    frozenSince = -1;
                }
                foreach (HitFlash flash in HitFlash.Shown) maxFlash = Mathf.Max(maxFlash, flash.Amount);
                Camera(out float offset);
                maxOffset = Mathf.Max(maxOffset, offset);
                if (!reported && HitFlash.Shown.Count == 0 && offset == 0 && Time.timeScale == GameTime.Speed)
                {
                    Log($"  strongest flash {maxFlash:0.00} camera offset {maxOffset * 100:0.00}cm, back to rest");
                    reported = true;
                }
                yield return null;
            }
            EntityStats.AnyDamageTaken -= onDamage;
            GameEvents.EntityDied -= onDied;
            if (!reported) Log($"  strongest flash {maxFlash:0.00} camera offset {maxOffset * 100:0.00}cm");
            Log("end: " + Show());
        }
    }
}
