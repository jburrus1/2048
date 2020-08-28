using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    Tile tile;

    public Cell()
    {
        tile = null;
    }

    public void SetTile(Tile tile)
    {
        this.tile = tile;
    }

    public Tile GetTile()
    {
        return tile;
    }

    public void ClearCell()
    {
        tile = null;
    }

    public bool HasTile()
    {
        return (tile != null);
    }
}
