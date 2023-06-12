using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OTetromino : Tetromino
{
    public OTetromino(Vector2Int coords, Tilemap tilemap, MapHandler mh, TileBase[] tiles) : base(coords, tilemap, mh, tiles)
	{
        color = TetrominoColor.Yellow;
        pivot = new Vector2(0.5f, 0.5f);

		relativeTileCoords = new Vector2Int[] {
			new Vector2Int(0, 0),
			new Vector2Int(1, 0),
			new Vector2Int(1, 1),
			new Vector2Int(0, 1)
		};

		kickTable = new Vector2Int[,] {
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero},
			{Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero, Vector2Int.zero}
		};
	}
}
