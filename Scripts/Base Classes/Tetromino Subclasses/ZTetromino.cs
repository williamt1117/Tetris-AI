using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ZTetromino : Tetromino
{
    public ZTetromino(Vector2Int coords, Tilemap tilemap, MapHandler mh, TileBase[] tiles) : base(coords, tilemap, mh, tiles)
	{
        color = TetrominoColor.Red;
        pivot = new Vector2(0, 0);

		relativeTileCoords = new Vector2Int[] {
			new Vector2Int(-1, 1),
			new Vector2Int(0, 1),
			new Vector2Int(0, 0),
			new Vector2Int(1, 0)
		};

		kickTable = new Vector2Int[,] {
			{Vector2Int.zero, new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, -2), new Vector2Int(-1, -2)},
			{Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, 2), new Vector2Int(1, 2)},
			{Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(1, -1), new Vector2Int(0, 2), new Vector2Int(1, 2)},
			{Vector2Int.zero, new Vector2Int(-1, 0), new Vector2Int(-1, 1), new Vector2Int(0, -2), new Vector2Int(-1, -2)},
			{Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, -2), new Vector2Int(1, -2)},
			{Vector2Int.zero, new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, 2), new Vector2Int(-1, 2)},
			{Vector2Int.zero, new Vector2Int(-1, 0), new Vector2Int(-1, -1), new Vector2Int(0, 2), new Vector2Int(-1, 2)},
			{Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, -2), new Vector2Int(1, -2)}
		};
	}
}
