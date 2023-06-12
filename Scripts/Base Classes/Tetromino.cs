using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Tetromino
{
    private MapHandler mapHandlerScript;

    public Vector2Int coords;
    public List<Block> blockList;
    public TetrominoColor color;
    public Vector2 pivot;
    protected Vector2Int[,] kickTable;
    public Vector2Int[] relativeTileCoords;
    private TileBase[] tiles;
    private bool inBoard;

    private Tilemap tilemap;

	private static readonly int[] kickTableHash = {
        -1, 0, -1, 7, 1, -1, 2, -1, -1, 3, -1, 5, 6, -1, 4, -1
    };
    public int rotation;

    public Tetromino(Vector2Int coords, Tilemap tilemap, MapHandler mh, TileBase[] tiles)
    {
        this.coords = coords;
        kickTable = new Vector2Int[8,5];
        blockList = new List<Block>();
        rotation = 0;
        relativeTileCoords = new Vector2Int[4];
        this.tilemap = tilemap;
        this.mapHandlerScript = mh;
        this.tiles = tiles;
    }

    public void draw(bool inBoard) {
        foreach (var relativePos in relativeTileCoords)
        {
            Block b = null;
            if (inBoard)
            {
                b = mapHandlerScript.updateBlock(coords + relativePos, color);
            }
            else
            {
				b = new Block(coords + relativePos, color, tilemap, tiles, mapHandlerScript.boardCoords);
			}
            
            blockList.Add(b);
        }
    }

    public void moveDirection(Vector2Int dir, List<Block> newBlockList, bool inBoard) {
        coords += dir;
		if (!inBoard)
		{
            foreach (var block in blockList)
			    block.move(dir);
            delete();
            draw(false);
            return;      
		}
		foreach (var block in blockList)
        {
            TetrominoColor c = block.coords.y >= mapHandlerScript.boardSizeY ? TetrominoColor.None : TetrominoColor.Black;
            block.setBlockType(c);
        }
        blockList = newBlockList;
        foreach (var block in blockList) {
            block.setBlockType(color);
        }
    }

    public void delete()
    {
        foreach (var block in blockList)
        {
            TetrominoColor c = TetrominoColor.None;
            if (inBoard && block.coords.y >= mapHandlerScript.boardSizeY)
            {
                c = TetrominoColor.Black;
            }
            block.setBlockType(c);
        }
        blockList.Clear();
    }

    public void rotate(Vector2Int newCoords, List<Block> newBlockList, int quarters) {
        coords = newCoords;
        foreach (var block in blockList) {
			TetrominoColor c = block.coords.y >= mapHandlerScript.boardSizeY ? TetrominoColor.None : TetrominoColor.Black;
			block.setBlockType(c);
		}
        blockList = newBlockList;
        foreach (var block in blockList) {
            block.setBlockType(color);
        }
        rotation = (quarters + rotation) % 4;
    }

    public Vector2Int[] wallKickRow(int startRotation, int endRotation) {
        Vector2Int[] returnArr = new Vector2Int[5];
        int hash = startRotation * 4 + endRotation;
        int row = kickTableHash[hash];
        if (row < 0) return null;
        for (int col = 0; col < 5; col++) {
            returnArr[col] = kickTable[row, col];
        }
        return returnArr;
    }

    public Tetromino clone(Tilemap tm)
    {
        Tetromino t = new Tetromino(coords, tm, mapHandlerScript, tiles);
        t.kickTable = kickTable;
        for (int i = 0; i < relativeTileCoords.Length; i++)
        {
            t.relativeTileCoords[i] = blockList[i].coords - coords;
        }
        t.pivot = pivot;
        t.rotation = rotation;
        t.color = color;
        return t;
    }
}
