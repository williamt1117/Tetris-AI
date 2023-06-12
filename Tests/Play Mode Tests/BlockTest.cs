using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Tilemaps;

public class BlockTest
{
    GameObject tileMapGameObject;
    Tilemap tileMap;
    TilemapRenderer tileMapRenderer;
    TileBase[] tb;

    [SetUp]
    public void Setup()
    {
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
    public void TestConstructor()
    {
		Block b = new Block(new Vector2Int(1, 2), TetrominoColor.Red, tileMap, tb, Vector2Int.zero);
        Assert.AreEqual(b.coords, new Vector2Int(1, 2));
        Assert.AreEqual(b.color, TetrominoColor.Red);
    }

    [Test]
	public void TestSetBlockType()
	{
		Block b = new Block(new Vector2Int(1, 2), TetrominoColor.Red, tileMap, tb, Vector2Int.zero);
		b.setBlockType(TetrominoColor.Orange);
		Assert.AreEqual(b.color, TetrominoColor.Orange);
	}

    [Test]
	public void TestMoveBlock()
	{
		Block b = new Block(new Vector2Int(1, 2), TetrominoColor.Red, tileMap, tb, Vector2Int.zero);
        Assert.AreEqual(tb[(int)TetrominoColor.Red], tileMap.GetTile(new Vector3Int(1, 2)));
        b.move(Vector2Int.down);
        Assert.AreEqual(new Vector2Int(1, 2) + Vector2Int.down, b.coords);
        Assert.IsNull(tileMap.GetTile(new Vector3Int(1, 2)));
        Assert.AreEqual(tb[(int)TetrominoColor.Red], tileMap.GetTile(new Vector3Int(b.coords.x, b.coords.y)));
        Assert.IsTrue(b.occupied);
	}
}
