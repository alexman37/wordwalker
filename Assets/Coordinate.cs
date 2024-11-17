using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coordinate
{
    //Coordinate system for hexagons is ROWS and SUBS (basically columns)
    public int r;
    public int s;

    public Coordinate(int r, int s)
    {
        this.r = r;
        this.s = s;
    }
}
