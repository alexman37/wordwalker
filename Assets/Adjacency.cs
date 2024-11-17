using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adjacency
{
    public enum Direction
    {
        E,
        SE,
        SW,
        W,
        NW,
        NE
    }

    public Direction direction;
    public Tile tile;

    public Adjacency(Direction dir, Tile t)
    {
        direction = dir;
        tile = t;
    }

    public override string ToString()
    {
        return direction + ": " + tile;
    }
}
