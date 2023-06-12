using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AIInputHandler : MonoBehaviour
{
	[System.NonSerialized] public float timeSinceSoftDrop;
	[System.NonSerialized] public float timeSinceLeftRightShift;
	[System.NonSerialized] public float timeSinceThink;

	public float softDropSpeed = 15f;
	public float leftRightSpeed = 25f;

	[System.NonSerialized] public bool holdUsed;

	public float thinkTime = 0.2f;

	public bool hardDropAIMoves = true;

	public Tuning customTuning;

	private MapHandler mh;

	private Queue<Move> moveQueue;

	public void resetVariables()
	{
		timeSinceSoftDrop = 0;
		timeSinceLeftRightShift = 0;
		timeSinceThink = 0;
		holdUsed = false;
	}

	void Start()
	{
		mh = GetComponent<MapHandler>();
		moveQueue = new Queue<Move>();
		
		resetVariables();
	}

	void Update()
	{

		bool[,] boardOccupied = new bool[mh.boardSizeX, mh.boardSizeY + 4];
		for (int x = 0; x < mh.boardSizeX; x++)
		{
			for (int y = 0; y < mh.boardSizeY + 4; y++)
			{
				boardOccupied[x, y] = mh.board[x, y].occupied;
			}
		}

		timeSinceThink += Time.deltaTime;
		if (!mh.ended && moveQueue.Count <= 0 && timeSinceThink >= thinkTime)
		{
			AIMoveSelector.queueAIMoves(moveQueue, mh, boardOccupied, hardDropAIMoves, holdUsed, customTuning);
			timeSinceThink = 0;
		}

		
		Move curMove = Move.None;
		if (moveQueue.Count > 0)
		{
			curMove = moveQueue.Dequeue();
		}

		timeSinceLeftRightShift += Time.deltaTime;
		timeSinceSoftDrop += Time.deltaTime;
		
		if (curMove == Move.Left) {
			mh.attemptMoveLeft();
		}
		if (curMove == Move.Right) {
			mh.attemptMoveRight();
		}
		if (curMove == Move.RotateCCW) {//Rotate Counter-clockwise
			mh.attemptRotateCounterClockwise();
		}
		if (curMove == Move.RotateCW) {//Rotate Clockwise
			mh.attemptRotateClockwise();
		}
		if (curMove == Move.HardDrop) {//Hard-Drop
			mh.hardDrop();
			resetVariables();
		}
		if (curMove == Move.SoftDrop) {//Soft Drop
			if (timeSinceSoftDrop >= 1.0f / softDropSpeed) {
				mh.updateBoard();
				timeSinceSoftDrop = 0;
			}
		}
		if (curMove == Move.Hold && holdUsed == false) {
			mh.useHold();
			resetVariables();
			holdUsed = true;
		}
	}
}