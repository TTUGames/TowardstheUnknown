using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    [SerializeField] private float width, height;

    [SerializeField] private TileScript tilePrefab;

    [SerializeField] private Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        GenerateGridFloor();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void GenerateGridFloor()
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
            {
                var spawnedTile = Instantiate(tilePrefab, new Vector3(x, 0f, y), Quaternion.identity);
                spawnedTile.name = $"tile {x} {y}";

                bool isOffset = (x % 2 == 0 && y % 2 != 0) || (x % 2 != 0 && y % 2 == 0);
                spawnedTile.Colorize(isOffset);
            }
    }
}
