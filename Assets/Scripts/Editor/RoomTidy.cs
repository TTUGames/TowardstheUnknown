using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Lays out the hierarchy of the room prefabs the same way, edition first: <c>Board</c> (the tiles in Floor, Exits, Void and
/// Walls, and the walls' colliders), <c>Shared</c> (what both editions show), <c>Anniversary</c> and <c>Classic</c> (listed
/// by an <see cref="EditionOnly"/> of the room's root), each split into category groups (Rocks, Props, Nature, Lights...).
/// Every object is filed by explicit rules (<see cref="Category"/>), keeps its world transform and gets a readable numbered
/// name (<c>Rock_01</c>, <c>Tile_x_y</c> after its cell). The containers left empty go. What a variant can't change (an
/// object of its base prefab) is reported, to tidy in the base. A report run changes nothing; an apply run checks that each
/// edition shows exactly the same components at the same places (<see cref="Fingerprint"/>) and that a bake of the layout
/// would change nothing, and saves the prefab only then. A second run finds nothing to do
/// </summary>
public static class RoomTidy
{
    private const string RoomFolder = "Assets/Prefabs/Rooms";
    private static readonly string[] BaseRooms = {
        "Assets/Prefabs/LevelDesign/Room.prefab", "Assets/Prefabs/LevelDesign/CombatRoom.prefab", "Assets/Prefabs/LevelDesign/TreasureRoom.prefab",
    };
    private const string Shared = "Shared";
    private static readonly Dictionary<string, string> LegacyGroups = new Dictionary<string, string> {
        { "Tilemap", RoomLayoutBaker.BoardName }, { "Décor", Shared }, { "Decor", Shared },
    };
    // The groups of Board and of the editions (the categories of Category, and Groups for the groups kept whole)
    private static readonly HashSet<string> SubGroups = new HashSet<string> {
        "Floor", "Exits", "Void", "Walls", "Groups",
        "Grass", "Water", "Volumes", "Lanterns", "Lamps", "Torches", "Lights", "Nature", "Crystals", "Rocks", "Fences", "FX", "Props",
    };
    // The groups the tool keeps as they are
    private static readonly HashSet<string> KeptGroups = new HashSet<string> { "SpawnLayouts" };
    // Names that say nothing: the object is named after its source, mesh or components instead
    private static readonly HashSet<string> JunkNames = new HashSet<string> { "GameObject", "Cube", "Sphere", "Plane", "Cylinder", "Quad", "Object", "NewGameObject" };
    // Pack prefixes and suffixes dropped from the names
    private static readonly HashSet<string> JunkTokens = new HashSet<string> { "ZLPC", "EM", "PRE", "LOD0", "Variant" };

    [MenuItem("Tools/Level Design/Tidy Room")]
    private static void TidySelection() => Debug.Log(Run(SelectedRooms(), apply: true));

    [MenuItem("Tools/Level Design/Tidy Room (Report)")]
    private static void ReportSelection() => Debug.Log(Run(SelectedRooms(), apply: false));

    [MenuItem("Tools/Level Design/Tidy Room", true)]
    [MenuItem("Tools/Level Design/Tidy Room (Report)", true)]
    private static bool HasSelectedRoom() => SelectedRooms().Count > 0;

    /// <summary>
    /// The room prefabs of the selection: the prefab assets selected in the Project window, or the prefab open in Prefab Mode
    /// </summary>
    private static List<string> SelectedRooms() {
        List<string> paths = Selection.GetFiltered<GameObject>(SelectionMode.Assets)
            .Select(AssetDatabase.GetAssetPath).Where(IsRoom).ToList();
        if (paths.Count == 0 && PrefabStageUtility.GetCurrentPrefabStage() is PrefabStage stage && IsRoom(stage.assetPath)) paths.Add(stage.assetPath);
        return paths;
    }

    private static bool IsRoom(string path) =>
        path.EndsWith(".prefab") && AssetDatabase.LoadAssetAtPath<GameObject>(path) is GameObject prefab && prefab.GetComponent<Room>() != null;

    /// <summary>
    /// Every room prefab: the base rooms, then the rooms of Prefabs/Rooms
    /// </summary>
    public static List<string> AllRooms() =>
        BaseRooms.Concat(AssetDatabase.FindAssets("t:Prefab", new[] { RoomFolder }).Select(AssetDatabase.GUIDToAssetPath).OrderBy(p => p)).ToList();

    /// <summary>
    /// Tidies the rooms, their bases first; a report run (<paramref name="apply"/> false) lists what it would do, saving nothing
    /// </summary>
    public static string Run(IEnumerable<string> paths, bool apply) {
        PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
        StringBuilder report = new StringBuilder();
        foreach (string path in paths.Distinct().OrderBy(VariantDepth).ThenBy(p => p)) {
            if (apply && stage != null && stage.assetPath == path) {
                report.AppendLine($"{Path.GetFileNameWithoutExtension(path)}: open in Prefab Mode, close it first");
                continue;
            }
            report.Append(Tidy(path, apply));
        }
        if (apply) AssetDatabase.SaveAssets();
        return report.ToString();
    }

    private static int VariantDepth(string path) {
        int depth = 0;
        for (GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path); prefab != null; depth++)
            prefab = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
        return depth;
    }

    /// <summary>
    /// Tidies one room prefab and reports what it did (or would do)
    /// </summary>
    public static string Tidy(string path, bool apply) {
        GameObject root = PrefabUtility.LoadPrefabContents(path);
        try {
            Tidier tidier = new Tidier(root);
            GameEdition[] editions = { GameEdition.Anniversary, GameEdition.Classic };
            var before = editions.ToDictionary(e => e, e => Fingerprint(root, e));
            tidier.Run();

            List<string> problems = new List<string>(tidier.Problems);
            foreach (GameEdition edition in editions) {
                string difference = Compare(before[edition], Fingerprint(root, edition));
                if (difference != null) problems.Add($"the {edition} would change: {difference}");
            }
            RoomLayout layout = root.GetComponent<Room>().Layout;
            if (layout != null) {
                string bake = RoomLayoutBaker.Bake(root, layout, true, out int changes);
                if (changes != 0) problems.Add("a bake would change the tiles: " + bake);
            }

            StringBuilder report = new StringBuilder();
            string name = Path.GetFileNameWithoutExtension(path);
            bool changed = tidier.Actions.Count > 0;
            string verdict = !changed ? "tidy" : problems.Count > 0 ? "NOT SAVED" : apply ? "tidied" : "to tidy";
            report.AppendLine($"=== {name}: {verdict} ({tidier.Actions.Count} changes)");
            foreach (string problem in problems) report.AppendLine("  ! " + problem);
            foreach (string action in tidier.Actions) report.AppendLine("  " + action);
            foreach (string note in tidier.Notes) report.AppendLine("  note: " + note);
            if (apply && changed && problems.Count == 0) PrefabUtility.SaveAsPrefabAsset(root, path);
            return report.ToString();
        }
        finally {
            PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private class Item
    {
        public GameObject gameObject;
        public string group;    // Board/Floor, Shared/Rocks...
        public string kind;     // the name before the number: Rock, Mushroom...
        public string name;     // the name it gets, null to keep it
    }

    private class Tidier
    {
        public readonly List<string> Actions = new List<string>();
        public readonly List<string> Notes = new List<string>();
        /// <summary>What forbids saving the room</summary>
        public readonly List<string> Problems = new List<string>();

        private readonly GameObject root;
        private readonly Room room;
        private readonly List<Item> items = new List<Item>();
        private readonly List<Transform> containers = new List<Transform>();
        private readonly HashSet<Object> referenced = new HashSet<Object>();
        private readonly Dictionary<GameObject, GameEdition> listed = new Dictionary<GameObject, GameEdition>();
        private Dictionary<Tile, RoomLayout.Cell> terrain;

        public Tidier(GameObject root) {
            this.root = root;
            room = root.GetComponent<Room>();
        }

        public void Run() {
            FindReferences();
            RenameLegacyGroups();
            terrain = RoomLayoutBaker.ClassifyAll(root);
            foreach (Transform child in root.transform.Cast<Transform>().ToList()) Collect(child);
            NameItems();
            foreach (Item item in items) Place(item);
            RemoveEmptyContainers();
            MergeEditionOnly();
            SortGroups();
        }

        // What the components of the room reference, but for the EditionOnly lists: a container referenced is kept whole
        private void FindReferences() {
            foreach (Component component in root.GetComponentsInChildren<Component>(true)) {
                if (component == null || component is Transform) continue;
                if (component is EditionOnly editionOnly) {
                    if (editionOnly.transform != root.transform) continue;
                    SerializedObject serialized = new SerializedObject(editionOnly);
                    GameEdition edition = (GameEdition)serialized.FindProperty("edition").enumValueIndex;
                    SerializedProperty objects = serialized.FindProperty("objects");
                    for (int i = 0; i < objects.arraySize; i++)
                        if (objects.GetArrayElementAtIndex(i).objectReferenceValue is GameObject shown) listed[shown] = edition;
                    continue;
                }
                SerializedProperty property = new SerializedObject(component).GetIterator();
                while (property.Next(true))
                    if (property.propertyType == SerializedPropertyType.ObjectReference && property.objectReferenceValue != null) {
                        Object value = property.objectReferenceValue;
                        referenced.Add(value is Component c ? c.gameObject : value);
                    }
            }
        }

        private void RenameLegacyGroups() {
            foreach (Transform child in root.transform.Cast<Transform>().ToList()) {
                if (!LegacyGroups.TryGetValue(child.name, out string name) || root.transform.Find(name) != null) continue;
                if (Owned(child.gameObject)) Rename(child.gameObject, name);
                else Notes.Add($"{child.name} belongs to the base prefab: tidy the base first, to name it {name}");
            }
        }

        // A group of the root, under its name or, in a variant whose base isn't tidied yet, its legacy name
        private Transform TopGroup(string name) =>
            root.transform.Find(name) ?? root.transform.Cast<Transform>().FirstOrDefault(t => LegacyGroups.TryGetValue(t.name, out string n) && n == name);

        // Sorts the objects under a transform: tiles and objects are items, empty objects are containers walked through
        private void Collect(Transform transform) {
            if (transform.parent == root.transform && KeptGroups.Contains(transform.name)) return;
            GameObject gameObject = transform.gameObject;
            bool topGroup = transform.parent == root.transform && IsGroup(transform);
            if (topGroup && !IsPlain(transform))
                Problems.Add($"{PathOf(transform)} carries {string.Join(", ", gameObject.GetComponents<Component>().Where(c => !(c is Transform)).Select(c => c ? c.GetType().Name : "a missing script"))}: move it to an object of its own");
            bool container = topGroup || IsPlain(transform);
            bool locked = container && (!gameObject.activeSelf || referenced.Contains(gameObject)) && !IsGroup(transform);
            if (container && !locked) {
                containers.Add(transform);
                foreach (Transform child in transform.Cast<Transform>().ToList()) Collect(child);
                return;
            }
            if (locked) Notes.Add($"{PathOf(transform)} kept whole ({(!gameObject.activeSelf ? "inactive" : "referenced")})");
            items.Add(Classify(gameObject, locked));
        }

        // Whether a transform is one of the groups the tool makes: named as one, and for a category a plain empty object
        // (an object named like its category, a water named Water, is not its group)
        private bool IsGroup(Transform transform) {
            if (transform.parent == root.transform)
                return transform.name is RoomLayoutBaker.BoardName or Shared or nameof(GameEdition.Anniversary) or nameof(GameEdition.Classic)
                    || LegacyGroups.ContainsKey(transform.name);
            return transform.parent.parent == root.transform && IsGroup(transform.parent) && SubGroups.Contains(transform.name) && IsPlain(transform);
        }

        // An object holding nothing but its transform, not a prefab instance
        private static bool IsPlain(Transform transform) =>
            !PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject) && transform.GetComponents<Component>().Length == 1;

        private Item Classify(GameObject gameObject, bool locked) {
            Item item = new Item { gameObject = gameObject };
            if (gameObject.TryGetComponent(out Tile tile)) {
                RoomLayout.Cell type = terrain[tile];
                item.group = RoomLayoutBaker.BoardName + "/" + RoomLayoutBaker.GroupOf(type);
                if (room.Layout != null) item.name = RoomLayoutBaker.TileName(room.Layout.LocalToCell(room.transform.InverseTransformPoint(tile.transform.position)));
                return item;
            }
            string source = SourcePath(gameObject);
            if (source == "Assets/Prefabs/LevelDesign/Wall.prefab") {
                item.group = RoomLayoutBaker.BoardName + "/Walls";
                item.kind = "Wall";
                return item;
            }
            GameEdition? edition = EditionOf(gameObject.transform);
            //A group kept whole goes in Groups, one level below the categories, so that the next run keeps it whole too
            if (locked) item.group = (edition?.ToString() ?? Shared) + "/Groups";
            else {
                item.kind = Kind(gameObject, source);
                item.group = (edition?.ToString() ?? Shared) + "/" + Category(gameObject, source, item.kind);
            }
            return item;
        }

        // The edition an object is shown in: the one of the EditionOnly of the root listing it or a parent, none for both
        private GameEdition? EditionOf(Transform transform) {
            for (; transform != null && transform != root.transform; transform = transform.parent)
                if (listed.TryGetValue(transform.gameObject, out GameEdition edition)) return edition;
            return null;
        }

        // Numbers the items of each kind across the room, by group and position; the numbers of the base's objects are kept
        private void NameItems() {
            foreach (var kind in items.Where(i => i.kind != null).GroupBy(i => i.kind)) {
                HashSet<int> taken = new HashSet<int>();
                Regex numbered = new Regex("^" + Regex.Escape(kind.Key) + @"_(\d+)$");
                foreach (Item item in kind.Where(i => !Owned(i.gameObject))) {
                    Match match = numbered.Match(item.gameObject.name);
                    if (match.Success) taken.Add(int.Parse(match.Groups[1].Value));
                }
                int next = 1;
                foreach (Item item in kind.Where(i => Owned(i.gameObject)).OrderBy(i => i.group).ThenBy(i => Key(i.gameObject.transform))) {
                    while (taken.Contains(next)) next++;
                    item.name = $"{kind.Key}_{next++:00}";
                }
            }
        }

        private string Key(Transform transform) {
            Vector3 p = root.transform.InverseTransformPoint(transform.position);
            return $"{Mathf.RoundToInt(p.x * 100) + 100000:D7}{Mathf.RoundToInt(p.z * 100) + 100000:D7}{Mathf.RoundToInt(p.y * 100) + 100000:D7}";
        }

        private void Place(Item item) {
            GameObject gameObject = item.gameObject;
            Transform group = Group(item.group);
            if (gameObject.transform.parent != group) {
                if (!Owned(gameObject)) Notes.Add($"{PathOf(gameObject.transform)} belongs to the base prefab: file it in {item.group} there");
                else {
                    Actions.Add($"move {PathOf(gameObject.transform)} -> {item.group}");
                    gameObject.transform.SetParent(group, true);
                    //An object shown by the EditionOnly now shows with its group
                    if (!gameObject.activeSelf && listed.ContainsKey(gameObject)) gameObject.SetActive(true);
                }
            }
            if (item.name != null && gameObject.name != item.name) {
                if (Owned(gameObject)) Rename(gameObject, item.name);
                else Notes.Add($"{PathOf(gameObject.transform)} belongs to the base prefab: name it {item.name} there");
            }
        }

        // The group at a path (Shared/Rocks), made if missing and possible; an edition's group is listed by its EditionOnly
        private Transform Group(string path) {
            Transform parent = root.transform;
            foreach (string name in path.Split('/')) {
                Transform group = parent == root.transform ? TopGroup(name) : parent.Cast<Transform>().FirstOrDefault(t => t.name == name && IsPlain(t));
                if (group == null) {
                    group = new GameObject(name).transform;
                    group.SetParent(parent, false);
                    Actions.Add($"create {PathOf(group)}");
                    if (parent == root.transform && System.Enum.TryParse(name, out GameEdition edition)) listed[group.gameObject] = edition;
                }
                ResetTransform(group);
                parent = group;
            }
            return parent;
        }

        // A group sits at the room's origin; its children keep their place
        private void ResetTransform(Transform group) {
            if (group.localPosition == Vector3.zero && group.localRotation == Quaternion.identity && group.localScale == Vector3.one) return;
            var children = group.Cast<Transform>().Select(c => (c, c.position, c.rotation, c.lossyScale)).ToList();
            if (PrefabUtility.IsPartOfPrefabInstance(group)) PrefabUtility.RevertObjectOverride(group, InteractionMode.AutomatedAction);
            group.localPosition = Vector3.zero;
            group.localRotation = Quaternion.identity;
            group.localScale = Vector3.one;
            foreach (var (child, position, rotation, scale) in children) {
                child.SetPositionAndRotation(position, rotation);
                Vector3 parentScale = child.parent.lossyScale;
                child.localScale = new Vector3(scale.x / parentScale.x, scale.y / parentScale.y, scale.z / parentScale.z);
            }
            Actions.Add($"reset the transform of {PathOf(group)}, its children kept in place");
        }

        // The EditionOnly of the root list their edition's group instead of the objects now in it
        private void MergeEditionOnly() {
            foreach (GameEdition edition in new[] { GameEdition.Anniversary, GameEdition.Classic }) {
                Transform group = root.transform.Find(edition.ToString());
                EditionOnly[] components = root.GetComponents<EditionOnly>()
                    .Where(c => (GameEdition)new SerializedObject(c).FindProperty("edition").enumValueIndex == edition).ToArray();
                if (group == null && components.Length == 0) continue;
                EditionOnly editionOnly = components.FirstOrDefault();
                if (editionOnly == null) {
                    editionOnly = root.AddComponent<EditionOnly>();
                    SerializedObject added = new SerializedObject(editionOnly);
                    added.FindProperty("edition").enumValueIndex = (int)edition;
                    added.ApplyModifiedPropertiesWithoutUndo();
                    Actions.Add($"add an EditionOnly {edition} on the root");
                }
                SerializedObject serialized = new SerializedObject(editionOnly);
                SerializedProperty objects = serialized.FindProperty("objects");
                List<GameObject> kept = new List<GameObject>();
                if (group != null) kept.Add(group.gameObject);
                for (int i = 0; i < objects.arraySize; i++) {
                    GameObject shown = objects.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
                    if (shown == null || kept.Contains(shown) || (group != null && shown.transform.IsChildOf(group))) continue;
                    kept.Add(shown);
                    Notes.Add($"the EditionOnly {edition} still lists {PathOf(shown.transform)}");
                }
                bool same = objects.arraySize == kept.Count && kept.Select((o, i) => objects.GetArrayElementAtIndex(i).objectReferenceValue == o).All(b => b);
                if (!same) {
                    if (PrefabUtility.IsPartOfPrefabInstance(editionOnly) && !PrefabUtility.IsAddedComponentOverride(editionOnly))
                        Notes.Add($"the EditionOnly {edition} belongs to the base prefab: its list is overridden here");
                    objects.arraySize = kept.Count;
                    for (int i = 0; i < kept.Count; i++) objects.GetArrayElementAtIndex(i).objectReferenceValue = kept[i];
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    Actions.Add($"the EditionOnly {edition} lists {string.Join(", ", kept.Select(o => PathOf(o.transform)))}");
                }
                foreach (EditionOnly duplicate in components.Skip(1)) {
                    SerializedObject other = new SerializedObject(duplicate);
                    bool empty = new[] { "objects", "behaviours", "renderers", "particleSystems" }.All(f => other.FindProperty(f).arraySize == 0);
                    if (!empty) Notes.Add($"a second EditionOnly {edition} on the root still lists objects");
                }
                if (group == null && kept.Count == 0 && Owned(editionOnly) && NothingElse(serialized)) {
                    Object.DestroyImmediate(editionOnly);
                    Actions.Add($"remove the empty EditionOnly {edition}");
                }
            }
        }

        private static bool NothingElse(SerializedObject editionOnly) =>
            new[] { "behaviours", "renderers", "particleSystems" }.All(f => editionOnly.FindProperty(f).arraySize == 0);

        // The containers and groups left empty go; a referenced one stays
        private void RemoveEmptyContainers() {
            IEnumerable<Transform> groups = root.transform.Cast<Transform>().Where(t => IsGroup(t))
                .SelectMany(t => t.Cast<Transform>().Where(IsEmptyGroup).Append(t));
            foreach (Transform transform in containers.AsEnumerable().Reverse().Concat(groups.ToList()).Distinct().ToList()) {
                if (transform == null || transform.childCount > 0 || !IsEmptyGroup(transform)) continue;
                if (transform.parent == root.transform && (transform.name is RoomLayoutBaker.BoardName or Shared || LegacyGroups.ContainsKey(transform.name))) continue;
                if (referenced.Contains(transform.gameObject)) {
                    Notes.Add($"{PathOf(transform)} is empty but referenced");
                    continue;
                }
                if (!Owned(transform.gameObject)) {
                    Notes.Add($"{PathOf(transform)} is empty and belongs to the base prefab");
                    continue;
                }
                Actions.Add($"remove the empty {PathOf(transform)}");
                listed.Remove(transform.gameObject);
                Object.DestroyImmediate(transform.gameObject);
            }
        }

        private static bool IsEmptyGroup(Transform transform) =>
            transform.childCount == 0 && transform.GetComponents<Component>().Length == 1 && !PrefabUtility.IsAnyPrefabInstanceRoot(transform.gameObject);

        // Board, Shared, Anniversary, Classic, then the kept groups; the board's groups and the categories by name
        private void SortGroups() {
            string[] order = { RoomLayoutBaker.BoardName, Shared, nameof(GameEdition.Anniversary), nameof(GameEdition.Classic) };
            List<Transform> top = root.transform.Cast<Transform>().ToList();
            List<Transform> sorted = top.OrderBy(t => System.Array.IndexOf(order, t.name) is int i && i >= 0 ? i : order.Length).ToList();
            Reorder(root.transform, sorted);
            foreach (Transform group in root.transform.Cast<Transform>().Where(t => IsGroup(t)).ToList()) {
                Reorder(group, group.Cast<Transform>().OrderBy(t => t.name, System.StringComparer.Ordinal).ToList());
                foreach (Transform category in group.Cast<Transform>().Where(t => IsGroup(t)).ToList())
                    Reorder(category, category.Cast<Transform>().OrderBy(t => t.name, System.StringComparer.Ordinal).ToList());
            }
        }

        // Puts the children in order, moving only what the prefab owns (a base's child keeps its place)
        private void Reorder(Transform parent, List<Transform> sorted) {
            List<Transform> current = parent.Cast<Transform>().ToList();
            if (current.SequenceEqual(sorted)) return;
            bool moved = false;
            for (int i = 0; i < sorted.Count; i++)
                if (sorted[i].GetSiblingIndex() != i && Owned(sorted[i].gameObject)) {
                    sorted[i].SetSiblingIndex(i);
                    moved = true;
                }
            if (moved && !parent.Cast<Transform>().SequenceEqual(current)) Actions.Add($"sort the children of {PathOf(parent)}");
        }

        private void Rename(GameObject gameObject, string name) {
            Actions.Add($"rename {PathOf(gameObject.transform)} -> {name}");
            gameObject.name = name;
        }

        // Whether this prefab owns the object or component (a variant can't move, rename nor remove its base's)
        private bool Owned(Object target) {
            if (target is Component component)
                return !PrefabUtility.IsPartOfPrefabInstance(component) || PrefabUtility.IsAddedComponentOverride(component)
                    || PrefabUtility.GetOutermostPrefabInstanceRoot(component.gameObject) != root;
            return PrefabUtility.GetOutermostPrefabInstanceRoot((GameObject)target) != root;
        }

        private string PathOf(Transform transform) {
            List<string> names = new List<string>();
            for (; transform != null && transform != root.transform; transform = transform.parent) names.Add(transform.name);
            names.Reverse();
            return names.Count > 0 ? string.Join("/", names) : "the root";
        }
    }

    /// <summary>
    /// The asset an object comes from: the prefab or model of an instance (through the base rooms), none for a plain object
    /// </summary>
    private static string SourcePath(GameObject gameObject) {
        if (!PrefabUtility.IsAnyPrefabInstanceRoot(gameObject)) return null;
        Object source = gameObject;
        while ((source = PrefabUtility.GetCorrespondingObjectFromSource(source)) != null) {
            string path = AssetDatabase.GetAssetPath(source);
            GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null || asset.GetComponent<Room>() == null) return path;
        }
        return null;
    }

    /// <summary>
    /// The name of an object before its number: its source's (a prefab or model), its mesh's, its own or its components'
    /// </summary>
    private static string Kind(GameObject gameObject, string source) {
        if (source != null) return Clean(Path.GetFileNameWithoutExtension(source));
        string own = Clean(gameObject.name);
        if (gameObject.TryGetComponent(out Light light)) return light.type == LightType.Directional ? "DirectionalLight" : JunkNames.Contains(own) ? "Light" : own;
        Mesh mesh = gameObject.TryGetComponent(out MeshFilter filter) ? filter.sharedMesh
            : gameObject.TryGetComponent(out SkinnedMeshRenderer skinned) ? skinned.sharedMesh : null;
        if (mesh != null && !JunkNames.Contains(Clean(mesh.name))) return Clean(mesh.name);
        if (!JunkNames.Contains(own)) return own;
        Component main = gameObject.GetComponents<Component>().FirstOrDefault(c => c is MonoBehaviour || c is Renderer);
        return main != null ? main.GetType().Name : "Object";
    }

    /// <summary>
    /// A name without its copy number, "Variant", the pack's codes and prefixes: <c>ZLPC_Rock_A_6 (2)</c> gives <c>Rock</c>,
    /// <c>Light Crystal</c> <c>LightCrystal</c>
    /// </summary>
    public static string Clean(string name) {
        name = Regex.Replace(name, @"\s*\((\d+|Clone)\)", "");
        IEnumerable<string> tokens = Regex.Split(name, @"[\s_\-\.]+")
            .Where(t => t.Length > 0 && !JunkTokens.Contains(t) && !Regex.IsMatch(t, @"^(\d+|[A-Z]|[A-Z]?\d+[a-z]?)$"));
        string cleaned = string.Concat(tokens.Select(t => char.ToUpperInvariant(t[0]) + t.Substring(1)));
        return cleaned.Length > 0 ? cleaned : "Object";
    }

    /// <summary>
    /// The category group of an object, by the first rule it meets: its components (grass, water, volume, a light alone,
    /// particles), then its source's folder and its name
    /// </summary>
    private static string Category(GameObject gameObject, string source, string kind) {
        bool Named(params string[] words) => words.Any(w => kind.Contains(w));
        if (gameObject.GetComponent<GrassPatch>() != null) return "Grass";
        if (gameObject.GetComponent<WaterSurface>() != null || gameObject.GetComponent<WaterDrip>() != null
            || source != null && source.Contains("/Environment/Water/")) return "Water";
        if (gameObject.GetComponent<Volume>() != null) return "Volumes";
        if (Named("Lantern")) return "Lanterns";
        if (Named("Lamp")) return "Lamps";
        if (Named("Torch", "Candle")) return "Torches";
        if (gameObject.GetComponent<Light>() != null && gameObject.GetComponent<Renderer>() == null) return "Lights";
        if (source != null && source.Contains("/Environment/Nature/")
            || Named("Tree", "Vine", "Root", "Mushroom", "Plant", "Wisteria", "Branch", "Lily", "Sprout", "Moss", "Rose", "Flower")) return "Nature";
        if (Named("Crystal")) return "Crystals";
        if (Named("Rock", "Cliff", "Ground", "Stairs")) return "Rocks";
        if (Named("Fence")) return "Fences";
        if (gameObject.GetComponent<ParticleSystem>() != null || gameObject.GetComponent<UnityEngine.VFX.VisualEffect>() != null) return "FX";
        return "Props";
    }

    /// <summary>
    /// What an edition shows of a room: every component alive in it (the EditionOnly components applied), with its world
    /// place relative to the root and what it draws (mesh, materials, light), whatever the names and the hierarchy
    /// </summary>
    public static Dictionary<string, List<(Matrix4x4 place, string name)>> Fingerprint(GameObject root, GameEdition edition) {
        Dictionary<GameObject, bool> active = new Dictionary<GameObject, bool>();
        Dictionary<Object, bool> enabled = new Dictionary<Object, bool>();
        foreach (EditionOnly editionOnly in root.GetComponentsInChildren<EditionOnly>(true)) {
            SerializedObject serialized = new SerializedObject(editionOnly);
            bool shown = (GameEdition)serialized.FindProperty("edition").enumValueIndex == edition;
            foreach (string field in new[] { "objects", "behaviours", "renderers", "particleSystems" }) {
                SerializedProperty list = serialized.FindProperty(field);
                for (int i = 0; i < list.arraySize; i++) {
                    Object target = list.GetArrayElementAtIndex(i).objectReferenceValue;
                    if (target is GameObject gameObject) active[gameObject] = shown;
                    else if (target != null) enabled[target] = shown;
                }
            }
        }
        bool Active(Transform t) {
            for (; t != null; t = t.parent)
                if (!(active.TryGetValue(t.gameObject, out bool on) ? on : t.gameObject.activeSelf)) return false;
            return true;
        }

        Dictionary<string, List<(Matrix4x4 place, string name)>> components = new Dictionary<string, List<(Matrix4x4 place, string name)>>();
        Matrix4x4 toRoot = root.transform.worldToLocalMatrix;
        foreach (Component component in root.GetComponentsInChildren<Component>(true)) {
            if (component == null || component is Transform || component is EditionOnly || !Active(component.transform)) continue;
            bool on = enabled.TryGetValue(component, out bool shown) ? shown
                : component is Behaviour behaviour ? behaviour.enabled : component is Renderer r ? r.enabled : !(component is Collider c) || c.enabled;
            string key = component.GetType().Name + (on ? "" : " (off)");
            switch (component) {
                case MeshFilter filter: key += " " + (filter.sharedMesh ? filter.sharedMesh.name + AssetDatabase.GetAssetPath(filter.sharedMesh) : "none"); break;
                case Renderer renderer: key += " " + string.Join(",", renderer.sharedMaterials.Select(m => m ? m.name : "none")); break;
                case Light light: key += $" {light.type} {light.color} {light.intensity:0.###} {light.range:0.###}"; break;
                case ParticleSystem system: key += system.main.playOnAwake ? " plays" : ""; break;
            }
            if (!components.TryGetValue(key, out var places)) components[key] = places = new List<(Matrix4x4, string)>();
            places.Add((toRoot * component.transform.localToWorldMatrix, component.name));
        }
        return components;
    }

    /// <summary>
    /// The first difference between two fingerprints, null if the same (places compared within a millimeter)
    /// </summary>
    public static string Compare(Dictionary<string, List<(Matrix4x4 place, string name)>> before, Dictionary<string, List<(Matrix4x4 place, string name)>> after) {
        foreach (string key in before.Keys.Union(after.Keys)) {
            var a = before.TryGetValue(key, out var x) ? x : new List<(Matrix4x4 place, string name)>();
            var b = after.TryGetValue(key, out var y) ? new List<(Matrix4x4 place, string name)>(y) : new List<(Matrix4x4 place, string name)>();
            if (a.Count != b.Count) return $"{key}: {a.Count} before, {b.Count} after";
            foreach (var (place, name) in a) {
                int match = b.FindIndex(m => Enumerable.Range(0, 16).All(i => Mathf.Abs(m.place[i] - place[i]) < 1e-3f));
                if (match < 0) return $"{key} of {name} moved from {place.GetColumn(3)}";
                b.RemoveAt(match);
            }
        }
        return null;
    }
}
