using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMoveSelector
{
	static void moveLineDownRecursive(int y, bool[,] board)
	{
		if (y >= board.GetLength(1))
			return;

		for (int x = 0; x < board.GetLength(0); x++)
		{
			board[x, y - 1] = board[x, y];
			board[x, y] = false;
		}
		moveLineDownRecursive(y + 1, board);
	}

	public static bool isFullRow(bool[,] board, int height)
	{
		for (int x = 0; x < board.GetLength(0); x++)
		{
			if (board[x, height] == false)
			{
				return false;
			}
		}
		return true;
	}

	public static int clearBoardLines(bool[,] board)
	{
		int linesCleared = 0;
		for (int y = board.GetLength(1) - 1; y >= 0; y--)
		{
			if (isFullRow(board, y))
			{
				for (int x = 0; x < board.GetLength(0); x++)
				{
					board[x, y] = false;
				}
				moveLineDownRecursive(y + 1, board);
				linesCleared++;
			}
		}
		return linesCleared;
	}

	public static int calculateBoardHeight(bool[,] board)
	{
		int boardHeight = 0;
		bool evaluated = false;
		for (int y = board.GetLength(1) - 1; y >= 0 && !evaluated; y--)
		{
			for (int x = 0; x < board.GetLength(0) && !evaluated; x++)
			{
				if (board[x, y] == true)
				{
					boardHeight = y + 1;
					evaluated = true;
				}
			}
		}
		return boardHeight;
	}

	public static int countBoardAirPockets(bool[,] board)
	{
		int airPocketCount = 0;
		for (int x = 0; x < board.GetLength(0); x++)
		{
			bool encounteredRoof = false;
			for (int y = board.GetLength(1) - 1; y >= 0; y--)
			{
				if (board[x, y] == true)
				{
					encounteredRoof = true;
				}
				else if (board[x, y] == false && encounteredRoof)
				{
					airPocketCount++;
				}
			}
		}
		return airPocketCount;
	}

	public static int[] calculateElevationMap(bool[,] board)
	{
		int[] elevation = new int[board.GetLength(0)];
		for (int x = 0; x < elevation.Length; x++)
		{
			elevation[x] = 0;
			for (int y = board.GetLength(1) - 1; y >= 0; y--)
			{
				if (board[x, y] == true)
				{
					elevation[x] = y + 1;
					break;
				}
			}
		}
		return elevation;
	}

	public static int calculateElevationDesparity(int[] elevation)
	{
		int elevationDesparity = 0;
		for (int i = 0; i < elevation.Length - 1; i++)
		{
			elevationDesparity += Mathf.Abs(elevation[i + 1] - elevation[i]);
		}
		return elevationDesparity;
	}

	public static int countBoardWells(int[] elevation)
	{
		int wells = 0;
		for (int i = 1; i < elevation.Length - 1; i++)
		{
			if (elevation[i - 1] - elevation[i] >= 3 && elevation[i + 1] - elevation[i] >= 3)
			{
				wells++;
			}
		}
		if (elevation[1] - elevation[0] >= 3)
		{
			wells++;
		}
		if (elevation[elevation.Length - 2] - elevation[elevation.Length - 1] >= 3)
		{
			wells++;
		}

		return wells;
	}

	public static int countBlocksAboveAir(bool[,] board)
	{
		int blocksAboveAir = 0;
		for (int x = 0; x < board.GetLength(0); x++)
		{
			bool foundAir = false;
			for (int y = 0; y < board.GetLength(1); y++)
			{
				if (board[x, y] == false)
				{
					foundAir = true;
				}
				else if (foundAir)
				{
					blocksAboveAir++;
				}
			}
		}
		return blocksAboveAir;
	}

	public static float evaluateBoard(bool[,] board, Tuning tuning)
	{
		int linesCleared = clearBoardLines(board);
		int boardHeight = calculateBoardHeight(board);
		int airPocketCount = countBoardAirPockets(board);
		int blocksAboveAir = countBlocksAboveAir(board);

		int[] elevation = calculateElevationMap(board);
		int elevationDesparity = calculateElevationDesparity(elevation);
		int wellCount = countBoardWells(elevation);

		float lowBoardScore = (board.GetLength(1) - boardHeight) * tuning.lowBoard;
		float airPocketsScore = -1 * (airPocketCount) * tuning.noAirPockets;
		float linesClearedScore = linesCleared * tuning.linesCleared;
		float elevationDesparityScore = -1 * elevationDesparity * tuning.elevationDesparity;
		float minimizeWellsScore = -1 * wellCount * tuning.minimizeWells;
		float tetrisScore = linesCleared >= 4 ? tuning.tetris : 0;
		float rightWellScore = -1 * elevation[board.GetLength(0) - 1] * tuning.rightWell;
		float blocksAboveAirScore = -1 * blocksAboveAir * tuning.noBlocksAboveAir;

		return lowBoardScore + airPocketsScore + linesClearedScore + elevationDesparityScore + minimizeWellsScore + tetrisScore + rightWellScore + blocksAboveAirScore;
	}

	static bool canMoveDown(List<Vector2Int> tetromino, bool[,] board, MapHandler mh)
	{
		foreach (var coords in tetromino)
		{
			Vector2Int newCoords = coords + Vector2Int.down;
			if (newCoords.x < 0 || newCoords.x >= mh.boardSizeX) return false;
			if (newCoords.y < 0) return false;
			if ((board[newCoords.x, newCoords.y] == true) && !tetromino.Contains(newCoords)) return false;
		}
		return true;
	}

	static bool[,] hardDropTetrominoAsVectors(List<Vector2Int> tetromino, bool[,] board, MapHandler mh)
	{
		List<Vector2Int> newTetromino = new List<Vector2Int>();
		foreach (var coords in tetromino)
			newTetromino.Add(coords);

		while (canMoveDown(newTetromino, board, mh))
		{
			List<Vector2Int> replacer = new List<Vector2Int>();
			foreach (var coords in newTetromino)
			{
				replacer.Add(coords + Vector2Int.down);
			}
			newTetromino = replacer;
		}

		bool[,] boardCopy = board.Clone() as bool[,];

		foreach (var coords in newTetromino)
		{
			boardCopy[coords.x, coords.y] = true;
		}
		return boardCopy;
	}

	public static void queueAIMoves(Queue<Move> moveQueue, MapHandler mh, bool[,] boardOccupied, bool hardDropAIMoves, bool holdUsed, Tuning t)
	{
		Tetromino activeTetromino = mh.activeTetromino;
		int holdTetrominoNum = mh.heldTetrominoNum;
		int[] tetrominoQueue = mh.queuedTetrominoesNums;

		Queue<Move> bestMoves = new Queue<Move>();
		float bestBoardScore = float.MinValue;

		if (holdTetrominoNum == -1)
		{
			moveQueue.Enqueue(Move.Hold);
			return;
		}

		bool[,] boardWithoutActiveTetromino = boardOccupied.Clone() as bool[,];
		foreach (var block in activeTetromino.blockList)
			boardWithoutActiveTetromino[block.coords.x, block.coords.y] = false;

		for (int rotations = 0; rotations < 4; rotations++)
		{
			Queue<Move> rotatingMoves = new Queue<Move>();
			for (int i = 0; i < rotations; i++)
				rotatingMoves.Enqueue(Move.RotateCW);

			List<Vector2Int> rotatedCoordsList = new List<Vector2Int>();
			foreach (var block in activeTetromino.blockList) rotatedCoordsList.Add(block.coords);
			rotatedCoordsList = MapHandler.rotateVectors(rotatedCoordsList, activeTetromino.pivot + activeTetromino.coords, rotations);
			for (int x = 0; x < mh.boardSizeX; x++)
			{
				Queue<Move> shiftingMoves = new Queue<Move>();
				int count = Mathf.Abs(x - activeTetromino.coords.x);
				for (int i = 0; i < count; i++)
				{
					if (x < activeTetromino.coords.x) shiftingMoves.Enqueue(Move.Left);
					if (x > activeTetromino.coords.x) shiftingMoves.Enqueue(Move.Right);
				}
				List<Vector2Int> offsetCoords = MapHandler.moveVectors(rotatedCoordsList, new Vector2Int(x - activeTetromino.coords.x, 0));
				bool validPosition = true;
				foreach (var coords in offsetCoords)
				{
					if (coords.x < 0 || coords.x >= mh.boardSizeX)
					{
						validPosition = false; continue;
					}
					if (boardWithoutActiveTetromino[coords.x, coords.y] == true) validPosition = false;
				}
				if (!validPosition) continue;

				bool[,] boardSubstate = hardDropTetrominoAsVectors(offsetCoords, boardWithoutActiveTetromino, mh);
				float boardSubstateScore = evaluateBoard(boardSubstate, t);
				if (boardSubstateScore > bestBoardScore)
				{
					bestMoves.Clear();
					foreach (var move in rotatingMoves)
					{
						bestMoves.Enqueue(move);
					}
					foreach (var move in shiftingMoves)
					{
						bestMoves.Enqueue(move);
					}
					if (hardDropAIMoves) bestMoves.Enqueue(Move.HardDrop);
					bestBoardScore = boardSubstateScore;
				}
			}
		}

		if (holdUsed)
		{
			while (bestMoves.Count > 0)
			{
				moveQueue.Enqueue(bestMoves.Dequeue());
			}
			return;
		}

		//Repeat with hold piece
		Tetromino holdTetromino = mh.numberToTetromino(activeTetromino.coords, mh.uiTilemap, holdTetrominoNum);

		for (int rotations = 0; rotations < 4; rotations++)
		{
			List<Move> rotatingMoves = new List<Move>();
			for (int i = 0; i < rotations; i++)
				rotatingMoves.Add(Move.RotateCW);

			List<Vector2Int> rotatedCoordsList = new List<Vector2Int>();
			foreach (var relativeCoords in holdTetromino.relativeTileCoords)
				rotatedCoordsList.Add(relativeCoords + holdTetromino.coords);

			rotatedCoordsList = MapHandler.rotateVectors(rotatedCoordsList, holdTetromino.pivot + holdTetromino.coords, rotations);
			for (int x = 0; x < mh.boardSizeX; x++)
			{
				Queue<Move> shiftingMoves = new Queue<Move>();
				int count = Mathf.Abs(x - holdTetromino.coords.x);
				for (int i = 0; i < count; i++)
				{
					if (x < holdTetromino.coords.x) shiftingMoves.Enqueue(Move.Left);
					if (x > holdTetromino.coords.x) shiftingMoves.Enqueue(Move.Right);
				}
				List<Vector2Int> offsetCoords = MapHandler.moveVectors(rotatedCoordsList, new Vector2Int(x - holdTetromino.coords.x, 0));
				bool validPosition = true;
				foreach (var coords in offsetCoords)
				{
					if (coords.x < 0 || coords.x >= mh.boardSizeX)
					{
						validPosition = false; continue;
					}
					if (boardWithoutActiveTetromino[coords.x, coords.y] == true) validPosition = false;
				}
				if (!validPosition) continue;

				bool[,] boardSubstate = hardDropTetrominoAsVectors(offsetCoords, boardWithoutActiveTetromino, mh);
				float boardSubstateScore = evaluateBoard(boardSubstate, t);
				if (boardSubstateScore > bestBoardScore)
				{
					bestMoves.Clear();
					bestMoves.Enqueue(Move.Hold);
					foreach (var move in rotatingMoves)
					{
						bestMoves.Enqueue(move);
					}
					foreach (var move in shiftingMoves)
					{
						bestMoves.Enqueue(move);
					}
					if (hardDropAIMoves) bestMoves.Enqueue(Move.HardDrop);
					bestBoardScore = boardSubstateScore;
				}
			}
		}
		while (bestMoves.Count > 0)
		{
			moveQueue.Enqueue(bestMoves.Dequeue());
		}
	}
}
