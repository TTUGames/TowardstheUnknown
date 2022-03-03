using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuScript : MonoBehaviour
{
    /// <summary>
    /// Assign the default <c>Tile Material</c> to a <c>Tile</c> tag
    /// </summary>
    [MenuItem("Tools/Assign Tile \"Tile\" Material")]
    public static void AssignTileTileMaterial()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");
        Material material = Resources.Load<Material>("Tile");

        foreach (GameObject tile in tiles)
            tile.GetComponent<Renderer>().material = material;
    }

    /// <summary>
    /// Assign a TBA <c>Material</c> to a <c>Tile</c> tag
    /// </summary>
    [MenuItem("Tools/Assign Tile \"Another\" Material")]
    public static void AssignTileAnotherMaterial()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");
        Material material = Resources.Load<Material>("Another");

        foreach (GameObject tile in tiles)
            tile.GetComponent<Renderer>().material = material;
    }

    /// <summary>
    /// Assign to a <c>Tile</c> tag the <c>Tile</c> script
    /// </summary>
    [MenuItem("Tools/Assign Tile \"Tile\" Script")]
    public static void AssignTileTileScript()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");

        foreach (GameObject tile in tiles)
            tile.AddComponent<Tile>();
    }
}
