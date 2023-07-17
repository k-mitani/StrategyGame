using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CellManager
{
    private int width;
    private int height;
    private Cell[,] cells;
    public CellManager(int w, int h)
    {
        width = w;
        height = h;
        cells = new Cell[h, w];
    }

    public Cell GetCell(CellPosition pos)
    {
        Debug.Assert(IsValidPosition(pos));
        return cells[pos.y, pos.x];
    }

    public bool IsValidPosition(CellPosition pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public IEnumerable<Cell> GetNeighbors(Cell cell)
    {
        return new[]
        {
            cell.position.Above,
            cell.position.Below,
            cell.position.Left,
            cell.position.Right,
        }.Where(p => IsValidPosition(p))
        .Select(p => GetCell(p));
    }
}

public struct CellPosition
{
    public static CellPosition At(int x, int y)
    {
        return new CellPosition { x = x, y = y };
    }

    public int x;
    public int y;

    public CellPosition Above => At(x, y - 1);
    public CellPosition Below => At(x, y + 1);
    public CellPosition Left => At(x - 1, y);
    public CellPosition Right => At(x + 1, y);

    public float Distance(CellPosition other)
    {
        return Mathf.Sqrt(Mathf.Pow(x - other.x, 2) + Mathf.Pow(y - other.y, 2));
    }
}

public class Cell
{
    public CellPosition position;
    public Terrain terrain;


    public Force force;
    public bool HasForce => force != null;

    public float Distance(Cell other) => position.Distance(other.position);
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
    public Cell Cell { get; set; }
    public string name;

    public float money;
    public float food;

    public float agriculture;
    public float agricultureProgress;
    public float commerce;
    public float commerceProgress;
    public float fortress;
    public float fortressProgress;

    public Country Country { get; set; }
    public Character[] Characters { get; set; }

    public void ImproveAgriculture(float val)
    {
        agricultureProgress += val;
        while (agricultureProgress > 100)
        {
            agriculture += 1;
            agricultureProgress -= 100;
        }
    }

    public void ImproveCommerce(float val)
    {
        commerceProgress += val;
        while (commerceProgress > 100)
        {
            commerce += 1;
            commerceProgress -= 100;
        }
    }

    public void ImproveFortress(float val)
    {
        fortressProgress += val;
        while (fortressProgress > 100)
        {
            fortress += 1;
            fortressProgress -= 100;
        }
    }
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

    public float commandPoints;

    public BattleFormation formation;
    public Castle location;
    public Country country;

    public void AddContribution(float val)
    {
        contribution += val;
    }


    public bool IsInCastle => State == CharacterState.InCastle;
    public CharacterState State { get; set; }
    public enum CharacterState
    {
        None,
        InCastle,
        Marching,
        UnderTreatment,
    }
}

