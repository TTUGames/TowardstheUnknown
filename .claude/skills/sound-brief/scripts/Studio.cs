using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

/// <summary>
/// Probes to film the game for the sound brief: a visible cursor drawn over everything, a mouse that glides and clicks
/// like a hand would, inventory drags, a relic of a given rarity. Run through the unity CLI (run_script)
/// </summary>
public static class Studio
{
    // ---------- Mouse
    // Called from the game's frames (the coroutines of Run): the state changes in the player's input buffer, which the
    // game's actions and the UI read; events queued from the CLI would land in the editor's while the Game view is unfocused

    static void MoveMouse(Vector2 screen) => Write(screen, ButtonDown);

    static void Button(bool down) => Write(CursorPosition, down);

    static bool ButtonDown
    {
        get => System.AppDomain.CurrentDomain.GetData("ttu.button") is bool b && b;
        set => System.AppDomain.CurrentDomain.SetData("ttu.button", value);
    }

    // The whole mouse state (the buttons are bits, which a single control can't change)
    static void Write(Vector2 screen, bool down)
    {
        ButtonDown = down;
        var state = new MouseState { position = screen };
        if (down) state = state.WithButton(UnityEngine.InputSystem.LowLevel.MouseButton.Left);
        InputState.Change(Mouse.current, state, InputUpdateType.Dynamic);
    }

    static MonoBehaviour Runner => GameScene.Map != null ? GameScene.Map : (MonoBehaviour)Object.FindAnyObjectByType<Camera>()?.GetComponent<MonoBehaviour>();

    static Vector2 Mouse2 => Mouse.current.position.ReadValue();

    static IEnumerator Glide(Vector2 to, float seconds)
    {
        Vector2 from = CursorPosition;
        for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
        {
            float k = Mathf.SmoothStep(0, 1, t / seconds);
            CursorPosition = Vector2.Lerp(from, to, k);
            MoveMouse(CursorPosition);
            yield return null;
        }
        CursorPosition = to;
        MoveMouse(to);
        yield return null;
    }

    static IEnumerator ClickHere()
    {
        Button(true);
        yield return null;
        yield return null;
        Button(false);
        yield return null;
    }

    // The last position the probes gave the mouse (the editor's mouse reading is not the game's)
    static Vector2 CursorPosition
    {
        get => System.AppDomain.CurrentDomain.GetData("ttu.cursor") is Vector2 v ? v : new Vector2(Screen.width / 2f, Screen.height / 2f);
        set => System.AppDomain.CurrentDomain.SetData("ttu.cursor", value);
    }

    static Vector2 TileScreen(Tile tile)
    {
        Vector3 top = tile.transform.position;
        top.y = tile.GetComponent<Collider>().bounds.max.y;
        return Camera.main.WorldToScreenPoint(top);
    }

    static string Describe(Tile tile) => tile == null ? "none" : $"({tile.transform.position.x:0},{tile.transform.position.z:0})";

    static Tile TileAt(int x, int z) => Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).FirstOrDefault(t => Describe(t) == $"({x},{z})");

    static Vector2 ElementScreen(VisualElement element, float x, float y)
    {
        IPanel panel = element.panel;
        Vector2 origin = RuntimePanelUtils.ScreenToPanel(panel, Vector2.zero);
        Vector2 scale = RuntimePanelUtils.ScreenToPanel(panel, Vector2.one) - origin;
        Rect bound = element.worldBound;
        var point = new Vector2(bound.xMin + bound.width * x, bound.yMin + bound.height * y);
        return new Vector2((point.x - origin.x) / scale.x, Screen.height - (point.y - origin.y) / scale.y);
    }

    static VisualElement Root(MonoBehaviour owner) =>
        ((UIDocument)owner.GetType().GetField("document", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(owner)).rootVisualElement;

    /// <summary>
    /// A sequence of mouse steps run on the game's frames: "tile x z seconds", "hud query n x seconds", "click", "down", "up",
    /// "wait seconds", "screen x y seconds" (fractions of the screen), separated by ';'
    /// </summary>
    public static string Mouse_(string steps)
    {
        Runner.StartCoroutine(Run(steps));
        return "running " + steps;
    }

    static IEnumerator Run(string steps)
    {
        foreach (string raw in steps.Split(';'))
        {
            string[] p = raw.Trim().Split(' ');
            switch (p[0])
            {
                case "tile":
                    Tile tile = TileAt(int.Parse(p[1]), int.Parse(p[2]));
                    if (tile != null) yield return Glide(TileScreen(tile), float.Parse(p[3], System.Globalization.CultureInfo.InvariantCulture));
                    break;
                case "hud":
                    VisualElement root = Root(GameScene.UI.Hud);
                    VisualElement element = p[1].StartsWith(".") ? root.Query(className: p[1].Substring(1)).AtIndex(int.Parse(p[2]))
                        : root.Query().Where(e => e.name == p[1] || e.GetType().Name == p[1]).AtIndex(int.Parse(p[2]));
                    if (element != null) yield return Glide(ElementScreen(element, F(p[3]), 0.5f), F(p[4]));
                    break;
                case "exit":
                    // exit <NORTH|EAST|SOUTH|WEST|ANY> <seconds>: an exit of the room
                    var exit = Object.FindObjectsByType<TransitionTile>(FindObjectsInactive.Exclude).FirstOrDefault(e => p[1] == "ANY" || e.direction.ToString() == p[1]);
                    if (exit != null) yield return Glide(TileScreen(exit.GetComponent<Tile>()), F(p[2]));
                    break;
                case "screen":
                    yield return Glide(new Vector2(Screen.width * F(p[1]), Screen.height * F(p[2])), F(p[3]));
                    break;
                case "piece":
                    // piece <n> <seconds>: the nth piece of the player's grid
                    VisualElement piece = PlayerPieces().ElementAtOrDefault(int.Parse(p[1]));
                    if (piece != null) yield return Glide(ElementScreen(piece, 0.3f, 0.5f), F(p[2]));
                    break;
                case "slot":
                    // slot <x> <y> <seconds>: a slot of the player's grid, by its column and row
                    yield return Glide(SlotScreen(int.Parse(p[1]), int.Parse(p[2])), F(p[3]));
                    break;
                case "click": yield return ClickHere(); break;
                case "down": Button(true); yield return null; break;
                case "up": Button(false); yield return null; break;
                case "wait": yield return new WaitForSecondsRealtime(F(p[1])); break;
            }
        }
    }

    static float F(string s) => float.Parse(s, System.Globalization.CultureInfo.InvariantCulture);

    static List<VisualElement> PlayerPieces()
    {
        TetrisInventory view = GameScene.UI.Inventory.PlayerInventory;
        var images = (Dictionary<TetrisInventoryItem, VisualElement>)typeof(TetrisInventory).GetField("itemImages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);
        return images.OrderBy(pair => pair.Key.slot.y).ThenBy(pair => pair.Key.slot.x).Select(pair => pair.Value).ToList();
    }

    static Vector2 SlotScreen(int x, int y)
    {
        TetrisInventory view = GameScene.UI.Inventory.PlayerInventory;
        var slots = (VisualElement[,])typeof(TetrisInventory).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);
        return ElementScreen(slots[x, y], 0.5f, 0.5f);
    }

    public static string Pieces()
    {
        TetrisInventory view = GameScene.UI.Inventory.PlayerInventory;
        var images = (Dictionary<TetrisInventoryItem, VisualElement>)typeof(TetrisInventory).GetField("itemImages", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);
        var slots = (VisualElement[,])typeof(TetrisInventory).GetField("slots", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(view);
        return $"grid {slots.GetLength(0)}x{slots.GetLength(1)}: " + string.Join(" ", images.OrderBy(pair => pair.Key.slot.y).ThenBy(pair => pair.Key.slot.x).Select(pair => $"{pair.Key.itemData.ID}@{pair.Key.slot}"));
    }

    // ---------- Cursor drawn over every panel, following the probes' mouse
    public static string Cursor(bool on)
    {
        GameObject existing = GameObject.Find("StudioCursor");
        if (existing != null) Object.Destroy(existing);
        if (!on) return "cursor off";
        var go = new GameObject("StudioCursor");
        go.SetActive(false);
        var document = go.AddComponent<UIDocument>();
        document.panelSettings = GameAssets.Instance.panelSettings;
        document.sortingOrder = 2000;
        go.SetActive(true);
        var arrow = new VisualElement { pickingMode = PickingMode.Ignore };
        arrow.style.position = Position.Absolute;
        arrow.style.width = 40;
        arrow.style.height = 40;
        arrow.generateVisualContent += context =>
        {
            Painter2D p = context.painter2D;
            p.lineJoin = LineJoin.Round;
            p.BeginPath();
            p.MoveTo(new Vector2(0, 0)); p.LineTo(new Vector2(0, 26)); p.LineTo(new Vector2(7, 20)); p.LineTo(new Vector2(12, 31));
            p.LineTo(new Vector2(17, 29)); p.LineTo(new Vector2(12, 18)); p.LineTo(new Vector2(21, 18)); p.ClosePath();
            p.fillColor = Color.white; p.Fill();
            p.strokeColor = Color.black; p.lineWidth = 2; p.Stroke();
        };
        document.rootVisualElement.pickingMode = PickingMode.Ignore;
        document.rootVisualElement.Add(arrow);
        go.AddComponent<CursorFollower>().Init(arrow);
        return "cursor on";
    }

    class CursorFollower : MonoBehaviour
    {
        VisualElement arrow;
        public void Init(VisualElement arrow) => this.arrow = arrow;
        void LateUpdate()
        {
            if (arrow?.panel == null) return;
            Vector2 screen = CursorPosition;
            Vector2 panel = RuntimePanelUtils.ScreenToPanel(arrow.panel, new Vector2(screen.x, Screen.height - screen.y));
            arrow.style.left = panel.x;
            arrow.style.top = panel.y;
        }
    }

    // ---------- Game state
    public static string Tiles(string selection)
    {
        var type = (Tile.SelectionType)System.Enum.Parse(typeof(Tile.SelectionType), selection);
        Vector3 player = GameScene.Player.transform.position;
        return string.Join(" ", Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude).Where(t => t.Selection == type)
            .OrderBy(t => Vector3.Distance(t.transform.position, player)).Select(Describe)) + " | player on " + Describe(GameScene.Player.GetComponent<TacticsMove>().CurrentTile);
    }

    /// <summary>
    /// Spawns a relic holding one artifact of the rarity (COMMON, RARE, EPIC, LEGENDARY) on a tile of the room
    /// </summary>
    public static string SpawnOrb(string rarity, int x, int z)
    {
#if UNITY_EDITOR
        var r = (ArtifactRarity)System.Enum.Parse(typeof(ArtifactRarity), rarity);
        ArtifactData data = UnityEditor.AssetDatabase.FindAssets("t:ArtifactData", new[] { "Assets/Data/Artifacts" })
            .Select(g => UnityEditor.AssetDatabase.LoadAssetAtPath<ArtifactData>(UnityEditor.AssetDatabase.GUIDToAssetPath(g)))
            .FirstOrDefault(d => d.rarity == r);
        Tile tile = TileAt(x, z);
        if (data == null || tile == null) return "no artifact or tile";
        var prefab = UnityEditor.AssetDatabase.LoadAssetAtPath<Collectable>("Assets/Prefabs/Collectables/Collectable.prefab");
        Collectable collectable = Object.Instantiate(prefab);
        collectable.SetArtifacts(new List<Artifact> { new Artifact(data) });
        collectable.transform.SetParent(GameScene.Map.CurrentRoom.transform);
        Vector3 top = tile.transform.position; top.y = tile.GetComponent<Collider>().bounds.max.y;
        collectable.transform.position = top;
        return $"{data.name} ({r}) on {Describe(tile)}";
#else
        return "editor only";
#endif
    }

    public static string Orbs() => string.Join(" ", Object.FindObjectsByType<Collectable>(FindObjectsInactive.Exclude)
        .Select(c => $"{typeof(Collectable).GetField("bestRarity", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(c)}@({c.transform.position.x:0},{c.transform.position.z:0})"));

    public static string ClearOrbs()
    {
        foreach (Collectable c in Object.FindObjectsByType<Collectable>(FindObjectsInactive.Exclude)) Object.Destroy(c.gameObject);
        return "cleared";
    }

    public static string EnemyHealth(int health)
    {
        foreach (EnemyStats enemy in Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude))
        {
            var field = typeof(EntityStats).GetField("currentHealth", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public);
            if (field != null) field.SetValue(enemy, health);
        }
        return string.Join(" ", Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude).Select(e => $"{e.name}:{e.CurrentHealth}"));
    }

    public static string Hud(string name) => Root(GameScene.UI.Hud).Query().Where(e => e.name == name || e.GetType().Name == name).ToList()
        .Select(e => $"{e.name}:{e.GetType().Name} {e.worldBound}").Aggregate("", (a, b) => a + b + "; ");
}
