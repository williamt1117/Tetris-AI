using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class AITest
{
    GameObject go;
    AIInputHandler aiInputHandler;
	Tuning t;

    [SetUp]
    public void Setup()
    {
		t = new Tuning();
		t.lowBoard = 1;
		t.noAirPockets = 10;
		t.linesCleared = 10;
		t.elevationDesparity = 1;
		t.minimizeWells = 10;
		t.tetris = 1000;
		t.rightWell = 10;
		t.noBlocksAboveAir = 10;
    }

	[Test]
	public void TestClearBoardLinesSimple()
	{
		//board = 
		//....
		//####
		//####
		//....
		bool[,] board =
		{
			{false, true, true, false},
			{false, true, true, false},
			{false, true, true, false},
		};
		bool[,] expectedBoard =
		{
			{false, false, false, false},
			{false, false, false, false},
			{false, false, false, false},
		};

		int linesCleared = AIMoveSelector.clearBoardLines(board);
		Assert.AreEqual(2, linesCleared);
		for (int x = 0; x < board.GetLength(0); x++)
		{
			for (int y = 0; y < board.GetLength(1); y++)
			{
				Assert.AreEqual(expectedBoard[x, y], board[x, y]);
			}
		}
	}

	[Test]
	public void TestClearBoardLinesComplex()
	{
		//board = 
		//....
		//####
		//####
		//....
		bool[,] board =
		{
			{true, true, true, false},
			{true, true, true, true},
			{false, false, true, false},
		};
		bool[,] expectedBoard =
		{
			{true, true, false, false},
			{true, true, true, false},
			{false, false, false, false},
		};

		int linesCleared = AIMoveSelector.clearBoardLines(board);
		Assert.AreEqual(1, linesCleared);
		for (int x = 0; x < board.GetLength(0); x++)
		{
			for (int y = 0; y < board.GetLength(1); y++)
			{
				Assert.AreEqual(expectedBoard[x, y], board[x, y]);
			}
		}
	}

	[Test]
	public void TestCalculateBoardHeight()
	{
		bool[,] board1 =
		{
			{true, true, true, false},
			{true, true, true, true},
			{false, false, true, false},
		};
		Assert.AreEqual(4, AIMoveSelector.calculateBoardHeight(board1));
		bool[,] board2 =
		{
			{true, false, true, false},
			{true, true, true, false},
			{false, false, true, false},
		};
		Assert.AreEqual(3, AIMoveSelector.calculateBoardHeight(board2));
		bool[,] board3 =
		{
			{true, false, false, false},
			{true, true, false, false},
			{false, false, false, false},
		};
		Assert.AreEqual(2, AIMoveSelector.calculateBoardHeight(board3));
		bool[,] board4 =
		{
			{false, false, false, false},
			{false, false, false, false},
			{false, false, false, false},
		};
		Assert.AreEqual(0, AIMoveSelector.calculateBoardHeight(board4));
	}

	[Test]
	public void TestCountAirPockets()
	{
		bool[,] board1 =
		{
			{false, true, false, false},
			{false, true, false, false},
			{false, true, false, false},
		};
		Assert.AreEqual(3, AIMoveSelector.countBoardAirPockets(board1));
		bool[,] board2 =
		{
			{false, false, true, false},
			{false, false, false, false},
			{false, false, true, false},
		};
		Assert.AreEqual(4, AIMoveSelector.countBoardAirPockets(board2));
		bool[,] board3 =
		{
			{false, false, false, false},
			{false, false, false, false},
			{false, false, false, false},
		};
		Assert.AreEqual(0, AIMoveSelector.countBoardAirPockets(board3));
	}

	[Test]
	public void TestCalculateElevationMap()
	{
		bool[,] board =
		{
			{false, true, false, false},
			{false, false, false, true},
			{false, false, true, false},
			{false, false, false, false},
			{false, false, true, false},
			{false, false, false, false},
		};
		int[] expectedElevation =
		{
			2, 4, 3, 0, 3, 0,
		};
		int[] elevation = AIMoveSelector.calculateElevationMap(board);
		Assert.AreEqual(expectedElevation.Length, elevation.Length);
		for (int i = 0; i < elevation.Length; i++)
		{
			Assert.AreEqual(elevation[i], expectedElevation[i]);
		}
	}

	[Test]
	public void TestCalculateElevationDesparity()
	{
		int[] elevation =
		{
			2, 4, 3, 0, 3, 0,
		};
		int expectedElevationDesparity = 2 + 1 + 3 + 3 + 3;
		int elevationDesparity = AIMoveSelector.calculateElevationDesparity(elevation);
		Assert.AreEqual(expectedElevationDesparity, elevationDesparity);
	}

	[Test]
	public void TestCountBoardWells()
	{
		int[] elevation1 =
		{
			5, 2, 4, 5,
		};
		Assert.AreEqual(0, AIMoveSelector.countBoardWells(elevation1));
		int[] elevation2 =
		{
			0, 3, 6, 6,
		};
		Assert.AreEqual(1, AIMoveSelector.countBoardWells(elevation2));
		int[] elevation3 =
		{
			3, 0, 0, 3,
		};
		Assert.AreEqual(0, AIMoveSelector.countBoardWells(elevation3));
		int[] elevation4 =
		{
			3, 0, 3, 6, 6, 2, 5, 6,
		};
		Assert.AreEqual(2, AIMoveSelector.countBoardWells(elevation4));
		int[] elevation5 =
		{
			0, 4, 5, 3, 0
		};
		Assert.AreEqual(2, AIMoveSelector.countBoardWells(elevation5));
	}

	[Test]
	public void TestEvaluateEmptyBoard()
	{
		bool[,] board =
		{
			{false, false, false, false},
			{false, false, false, false},
			{false, false, false, false},
		};

		//expected lowBoardScore: 4*1 = 4
		//expected airPocketScore: 0*10 = 0
		//expected linesClearedScore: 0*20 = 0
		//expected elevationDesparityScore: 0*1 = 0
		//expected wellMinimizingScore: 0*10 = 0
		//expected tetrisScore = 0*1000 = 0
		//expected rightWellScore = -1*0*10 = 0
		//expected noBlocksAboveAirScore = -1*0*10 = 0
		//expected totalScore: 4 + 0 + 0 + 0 + 0 + 0 + 0 + 0 = 4
		float boardScore = AIMoveSelector.evaluateBoard(board, t);
		Assert.AreEqual(4.0f, boardScore, float.Epsilon);
	}

	[Test] public void TestCompareBoards()
	{
		string path = "Board Presets/rightWellErrorPositionIBlock";
		TextAsset boardFile = Resources.Load<TextAsset>(path);
		TetrominoColor[,] boardColors = MapConstructor.constructBoard(10, 20, boardFile);
		bool[,] state0 = new bool[10, 20];
		bool[,] state1 = new bool[10, 20];
		bool[,] state2 = new bool[10, 20];
		for (int x = 0; x < 10; x++)
		{
			for (int y = 0; y < 20; y++)
			{
				state0[x, y] = boardColors[x, y] != TetrominoColor.Black;
				state1[x, y] = state0[x, y];
				state2[x, y] = state0[x, y];
			}
		}

		//load ITetromino into board1 at (7, 3) -> (7, 6)
		state1[7, 3] = true; state1[7, 4] = true; state1[7, 5] = true; state1[7, 6] = true;

		//loadITetromino into board2 at (9, 1) -> (9, 4)
		state2[9, 1] = true; state2[9, 2] = true; state2[9, 3] = true; state2[9, 4] = true;

		float state0Score = AIMoveSelector.evaluateBoard(state0, t);
		float state1Score = AIMoveSelector.evaluateBoard(state1, t);
		float state2Score = AIMoveSelector.evaluateBoard(state2, t); //state 2 should be higher scored then state 1

		Assert.Greater(state2Score, state1Score);
	}

	[Test]
	public void TestCountBlocksAboveAir()
	{
		bool[,] board =
		{
			{false, true, false, false},
			{false, true, true, false},
			{true, false, false, false},
		};
		Assert.AreEqual(3, AIMoveSelector.countBlocksAboveAir(board));
	}
}
