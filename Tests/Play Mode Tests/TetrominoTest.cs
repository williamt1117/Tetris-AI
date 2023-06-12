using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public class TetrominoTest
{
	GameObject tileMapGameObject;
	Tilemap tileMap;
	TilemapRenderer tileMapRenderer;
	TileBase[] tb;

	GameObject mapHandlerGameObject;
	MapHandler mapHandlerScript;

	[SetUp]
    public void Setup()
    {
		mapHandlerGameObject = new GameObject();
		mapHandlerGameObject.SetActive(false);
		mapHandlerScript = mapHandlerGameObject.AddComponent<MapHandler>();

		tileMapGameObject = new GameObject();
		tileMap = tileMapGameObject.AddComponent<Tilemap>();
		tileMapRenderer = tileMapGameObject.AddComponent<TilemapRenderer>();
		tb = new TileBase[10];
		for (int i = 0; i < tb.Length; i++)
		{
			string path = string.Format("tetris-tileset_{0}", i);
			tb[i] = Resources.Load<TileBase>(path);
			Assert.NotNull(tb[i]);
		}
	}

    [Test]
    public void TestTetrominoConstructor()
    {
		Tetromino t = new ITetromino(new Vector2Int(1, 2), tileMap, mapHandlerScript, tb);
		Assert.AreEqual(new Vector2Int(1, 2), t.coords);
		Assert.AreEqual(TetrominoColor.Cyan, t.color);
		Assert.AreEqual(0, t.blockList.Count);
		Assert.AreEqual(0, t.rotation);
    }

	[Test]
	public void TestTetrominoDrawOutOfBoard()
	{
		Tetromino t = new ITetromino(new Vector2Int(1, 2), tileMap, mapHandlerScript, tb);
		t.draw(false);
		Assert.AreEqual(4, t.blockList.Count);
		for (int i = 0; i < 4; i++)
		{
			Block block = t.blockList[i];
			Assert.AreEqual(block.coords, t.relativeTileCoords[i] + t.coords);
			Assert.AreEqual(
				tileMap.GetTile(new Vector3Int(t.relativeTileCoords[i].x + t.coords.x, t.relativeTileCoords[i].y + t.coords.y)), 
				tb[(int)t.color]);
		}
	}

	[Test]
	public void TestTetrominoMoveDirectionOutOfBoard()
	{
		Tetromino t = new ITetromino(new Vector2Int(1, 2), tileMap, mapHandlerScript, tb);
		t.draw(false);
		t.moveDirection(Vector2Int.down, null, false);
		Assert.AreEqual(new Vector2Int(1, 2) + Vector2Int.down, t.coords);
		for (int i = 0; i < 4; i++)
		{
			Block block = t.blockList[i];
			Assert.AreEqual(
				tileMap.GetTile(new Vector3Int(t.relativeTileCoords[i].x + t.coords.x,
				t.relativeTileCoords[i].y + t.coords.y)), tb[(int)t.color]);
			Assert.IsNull(
				tileMap.GetTile(new Vector3Int(t.relativeTileCoords[i].x + t.coords.x - Vector2Int.down.x,
					t.relativeTileCoords[i].y + t.coords.y - Vector2Int.down.y)));
		}
	}

	[Test]
	public void TestDeleteTetrominoOutOfBoard()
	{
		Tetromino t = new ITetromino(new Vector2Int(1, 2), tileMap, mapHandlerScript, tb);
		t.draw(false);
		t.delete();
		for (int i = 0; i < 4; i++)
		{
			Assert.IsNull(tileMap.GetTile(new Vector3Int(t.coords.x + t.relativeTileCoords[i].x, 
				t.coords.y + t.relativeTileCoords[i].y)));
		}
	}
}
