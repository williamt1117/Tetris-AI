using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Block
{
    public Vector2Int coords;

    public bool occupied;

    public TetrominoColor color;

    readonly TileBase[] tiles;

    readonly Tilemap tilemap;
    readonly Vector2Int boardOrigin;

    public Block(Vector2Int coords, TetrominoColor color, Tilemap tm, TileBase[] tiles, Vector2Int boardOrigin)
    {
        this.coords = coords;
        this.tilemap = tm;
        this.tiles = tiles;
        this.boardOrigin = boardOrigin;
        setBlockType(color);
    }

	private TileBase calculateTileBase(TetrominoColor c)
	{
		if (c == TetrominoColor.None) return null;
		return tiles[(int)c];
	}

	public void setBlockType(TetrominoColor color)
    {
        this.color = color;
        occupied = (color != TetrominoColor.Black) && (color != TetrominoColor.None);
        tilemap.SetTile(new Vector3Int(coords.x + boardOrigin.x, coords.y + boardOrigin.y), calculateTileBase(color));
    }

    public void move(Vector2Int dir)
    {
        TetrominoColor c = color;
        setBlockType(TetrominoColor.None);
        this.coords += dir;
        setBlockType(c);
    }

    public Block clone(Tilemap tm)
    {
        return new Block(coords, color, tm, tiles, boardOrigin);
    }

	public override string ToString()
	{
        return coords.ToString() + ", " + color.ToString();
	}
}
