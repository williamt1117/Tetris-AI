using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ITetromino : Tetromino
{
    public ITetromino(Vector2Int coords, Tilemap tilemap, MapHandler mh, TileBase[] tiles) : base(coords, tilemap, mh, tiles)
    {
        color = TetrominoColor.Cyan;
        pivot = new Vector2(0.5f, -0.5f);

        relativeTileCoords = new Vector2Int[]
        {
            new Vector2Int(-1, 0),
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0)
        };

        kickTable = new Vector2Int[,] {
            {Vector2Int.zero, new Vector2Int(-2, 0), new Vector2Int(1, 0), new Vector2Int(-2, -1), new Vector2Int(1, 2)},
            {Vector2Int.zero, new Vector2Int(2, 0), new Vector2Int(-1, 0), new Vector2Int(2, 1), new Vector2Int(-1, -2)},
			{Vector2Int.zero, new Vector2Int(-1, 0), new Vector2Int(2, 0), new Vector2Int(-1, 2), new Vector2Int(2, -1)},
			{Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(-2, 0), new Vector2Int(1, -2), new Vector2Int(-2, 1)},
			{Vector2Int.zero, new Vector2Int(2, 0), new Vector2Int(-1, 0), new Vector2Int(2, 1), new Vector2Int(-1, -2)},
			{Vector2Int.zero, new Vector2Int(-2, 0), new Vector2Int(1, 0), new Vector2Int(-2, -1), new Vector2Int(1, 2)},
			{Vector2Int.zero, new Vector2Int(1, 0), new Vector2Int(-2, 0), new Vector2Int(1, -2), new Vector2Int(-2, 1)},
			{Vector2Int.zero, new Vector2Int(-1, 0), new Vector2Int(2, 0), new Vector2Int(-1, 2), new Vector2Int(2, -1)}
		};
    }
}
