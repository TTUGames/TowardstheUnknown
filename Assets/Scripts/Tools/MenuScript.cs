using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class MenuScript : MonoBehaviour
{
    [MenuItem("Tools/Assign Tile \"Tile\" Material")]
    public static void AssignTileTileMaterial()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");
        Material material = Resources.Load<Material>("Tile");

        foreach (GameObject tile in tiles)
            tile.GetComponent<Renderer>().material = material;
    }

    [MenuItem("Tools/Assign Tile \"Another\" Material")]
    public static void AssignTileAnotherMaterial()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");
        Material material = Resources.Load<Material>("Another");

        foreach (GameObject tile in tiles)
            tile.GetComponent<Renderer>().material = material;
    }

    [MenuItem("Tools/Assign Tile \"Tile\" Script")]
    public static void AssignTileTileScript()
    {
        GameObject[] tiles = GameObject.FindGameObjectsWithTag("tile");

        foreach (GameObject tile in tiles)
            tile.AddComponent<Tile>();
    }
}
