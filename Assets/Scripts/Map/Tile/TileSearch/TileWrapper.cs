using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileWrapper
{
    public Tile tile;
    public Tile parent;
    public int distance;

    public TileWrapper(Tile tile, Tile parent, int distance) {
        this.tile = tile;
        this.parent = parent;
        this.distance = distance;
	}
}
