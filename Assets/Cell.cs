using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellManager
{


}

public class Cell
{
    public int x;
    public int y;
    public Terrain terrain;
}


public enum Terrain
{
    Plain,
    Forest,
    Hill,
    Mountain,
    River,
    Sea,
    Castle,
    Town,
    Farm,
}






public class Castle
{
    public string name;

    public float money;
    public float food;

    public float agriculture;
    public float commerce;
    public float defense;
}

public class Character
{
    public string name;
    public float martial;
    public float stewardship;
    public float intelligence;

    public float contribution;
    public float fame;
    public float loyalty;
    public float ambition;

    public float salary;
    public float money;

}

