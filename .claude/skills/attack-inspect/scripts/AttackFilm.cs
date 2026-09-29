using Object = UnityEngine.Object;
using System.Collections;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// Films an attack in Play mode: casts an artifact (by asset name) from the player on the nearest enemy, as the player's own
/// cast does (weapon, glow, queue), with a fixed game step per frame (Time.captureDeltaTime), and saves every frame for the
/// time asked, plus the screen positions of the player and the target in frames.txt. Run through the unity CLI (run_script);
/// build the contact sheet with sheet.py.
/// </summary>
public static class AttackFilm
{
    /// <summary>
    /// Overwrites fields of an ability's data in memory from JSON, e.g. {"impactDelay":0.45,"timing":{"enabled":true,"swingStart":0.3}}
    /// (a nested object is replaced whole: give all its fields). The asset on disk is unchanged until saved: write the values
    /// to the asset file, then reimport it (Reload)
    /// </summary>
    public static string Set(string ability, string json)
    {
#if UNITY_EDITOR
        var data = FindData(ability);
        if (data == null) return "no ability " + ability;
        JsonUtility.FromJsonOverwrite(json, data);
        return $"{data.name}: {JsonUtility.ToJson(data.timing)} impact {data.impactDelay} duration {data.duration}";
#else
        return "editor only";
#endif
    }

    /// <summary>
    /// Reloads abilities from their files (names separated by commas, * for all), dropping the values Set in memory
    /// </summary>
    public static string Reload(string abilities)
    {
#if UNITY_EDITOR
        if (abilities == "*")
            abilities = string.Join(",", UnityEditor.AssetDatabase.FindAssets("t:AbilityData", new[] { "Assets/Data" })
                .Select(g => System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GUIDToAssetPath(g))));
        foreach (string name in abilities.Split(','))
        {
            var data = FindData(name.Trim());
            if (data != null) UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(data), UnityEditor.ImportAssetOptions.ForceUpdate);
        }
        return "reloaded " + abilities;
#else
        return "editor only";
#endif
    }

#if UNITY_EDITOR
    private static AbilityData FindData(string name) =>
        UnityEditor.AssetDatabase.LoadAssetAtPath<AbilityData>($"Assets/Data/Artifacts/{name}.asset") ??
        UnityEditor.AssetDatabase.LoadAssetAtPath<AbilityData>($"Assets/Data/EnemyPatterns/{name}.asset");
#endif

    /// <summary>
    /// Puts the player on the free tile the nearest to it at the distance from the nearest enemy (1: next to it), facing it
    /// </summary>
    public static string Approach(int distance)
    {
        PlayerTurn player = GameScene.Player;
        EnemyStats target = Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude)
            .OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
        if (target == null) return "no enemy";
        Vector3 from = target.transform.position;
        Tile tile = Object.FindObjectsByType<Tile>(FindObjectsInactive.Exclude)
            .Where(t => t.GetEntity() == null && Mathf.RoundToInt(Mathf.Abs(t.transform.position.x - from.x) + Mathf.Abs(t.transform.position.z - from.z)) == distance)
            .OrderBy(t => (t.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
        if (tile == null) return "no free tile at " + distance;
        var move = player.GetComponent<TacticsMove>();
        move.CurrentTile?.SetEntity(null);
        player.transform.position = new Vector3(tile.transform.position.x, player.transform.position.y, tile.transform.position.z);
        move.SetCurrentTileFromRaycast();
        Vector3 look = from - player.transform.position;
        look.y = 0;
        player.transform.rotation = Quaternion.LookRotation(look);
        return $"player on {move.CurrentTile?.name}, {distance} from {target.name}";
    }

    /// <param name="ability">The asset name of the artifact (Data/Artifacts)</param>
    /// <param name="folder">An absolute folder, emptied of its frames first</param>
    /// <param name="step">Game seconds between two frames</param>
    /// <param name="seconds">Game seconds filmed from the cast</param>
    public static string Shoot(string ability, string folder, float step, float seconds)
    {
        PlayerTurn player = GameScene.Player;
        if (player == null) return "not in a game";
        if (ActionManager.IsBusy) return "the queue is busy";
#if UNITY_EDITOR
        var data = UnityEditor.AssetDatabase.LoadAssetAtPath<ArtifactData>($"Assets/Data/Artifacts/{ability}.asset");
#else
        ArtifactData data = null;
#endif
        if (data == null) return "no artifact " + ability;
        EnemyStats target = Object.FindObjectsByType<EnemyStats>(FindObjectsInactive.Exclude)
            .OrderBy(e => (e.transform.position - player.transform.position).sqrMagnitude).FirstOrDefault();
        Tile tile = data.target == EntityType.PLAYER || target == null
            ? player.GetComponent<TacticsMove>().CurrentTile
            : target.GetComponent<TacticsMove>().CurrentTile;

        System.IO.Directory.CreateDirectory(folder);
        foreach (string file in System.IO.Directory.GetFiles(folder, "frame-*.png")) System.IO.File.Delete(file);
        var artifact = new Artifact(data);
        MethodInfo cast = typeof(PlayerAttack).GetMethod("Cast", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(Artifact), typeof(Tile) }, null);
        Time.captureDeltaTime = step;
        cast.Invoke(player.playerAttack, new object[] { artifact, tile });
        ActionManager.Run(Film());
        return $"filming {ability} on {tile.GetEntity()?.name ?? "its tile"}: {Mathf.CeilToInt(seconds / step)} frames";

        IEnumerator Film()
        {
            var log = new System.Text.StringBuilder();
            Camera camera = Camera.main;
            int frames = Mathf.CeilToInt(seconds / step);
            for (int i = 0; i <= frames; i++)
            {
                yield return new WaitForEndOfFrame();
                ScreenCapture.CaptureScreenshot($"{folder}/frame-{i:000}.png");
                Vector3 p = camera.WorldToScreenPoint(player.transform.position + Vector3.up * 0.5f);
                Vector3 t = target != null ? camera.WorldToScreenPoint(target.transform.position + Vector3.up * 0.5f) : p;
                log.AppendLine(System.FormattableString.Invariant($"{i} {i * step:0.000} {p.x:0} {Screen.height - p.y:0} {t.x:0} {Screen.height - t.y:0} {Time.timeScale:0.00}"));
            }
            Time.captureDeltaTime = 0;
            System.IO.File.WriteAllText($"{folder}/frames.txt", $"{Screen.width} {Screen.height}\n" + log);
            Debug.Log($"[film] {ability} done: {frames + 1} frames in {folder}");
        }
    }
}
