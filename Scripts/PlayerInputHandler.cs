using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInputHandler : MonoBehaviour
{
    [System.NonSerialized] public float timeSinceSoftDrop;
    [System.NonSerialized] public float timeSinceLeftRightShift;

	[System.NonSerialized] public bool holdUsed;

	public float inputForgivenessTime = 0.15f;
	public float softDropSpeed = 15f;
	public float leftRightSpeed = 25f;

    private MapHandler mh;

	public KeyCode moveLeftKey = KeyCode.LeftArrow;
	public KeyCode moveRightKey = KeyCode.RightArrow;
	public KeyCode softDropKey = KeyCode.DownArrow;
	public KeyCode hardDropKey = KeyCode.UpArrow;
	public KeyCode rotateClockwiseKey = KeyCode.X;
	public KeyCode rotateCounterClockwiseKey = KeyCode.Z;
	public KeyCode holdKey = KeyCode.Space;
	public KeyCode resetKey = KeyCode.R;

	public void resetVariables() {
		timeSinceSoftDrop = 0;
		timeSinceLeftRightShift = 0;
		holdUsed = false;
	}

    void Start()
    {
        mh = GetComponent<MapHandler>();

		resetVariables();
	}

    void Update()
    {
		timeSinceLeftRightShift += Time.deltaTime;
		timeSinceSoftDrop += Time.deltaTime;
		if (Input.GetKeyDown(moveLeftKey)) {
			mh.attemptMoveLeft();
			timeSinceLeftRightShift = 0;
		}
		if (Input.GetKey(moveLeftKey) && timeSinceLeftRightShift >= inputForgivenessTime + 1.0f / leftRightSpeed) {
			mh.attemptMoveLeft();
			timeSinceLeftRightShift = inputForgivenessTime;
		}
		if (Input.GetKeyDown(moveRightKey)) {
			mh.attemptMoveRight();
		}
		if (Input.GetKey(moveRightKey) && timeSinceLeftRightShift >= inputForgivenessTime + 1.0f / leftRightSpeed) {
			mh.attemptMoveRight();
			timeSinceLeftRightShift = inputForgivenessTime;
		}
		if (Input.GetKeyDown(rotateCounterClockwiseKey)) {//Rotate Counter-clockwise
			mh.attemptRotateCounterClockwise();
		}
		if (Input.GetKeyDown(rotateClockwiseKey)) {//Rotate Clockwise
			mh.attemptRotateClockwise();
		}
		if (Input.GetKeyDown(hardDropKey)) {//Hard-Drop
			mh.hardDrop();
			resetVariables();
		}
		if (Input.GetKey(softDropKey)) {//Soft Drop
			if (timeSinceSoftDrop >= 1.0f / softDropSpeed) {
				mh.updateBoard();
				timeSinceSoftDrop = 0;
			}
		}
		if (Input.GetKeyDown(holdKey) && holdUsed == false) {
			mh.useHold();
			resetVariables();
			holdUsed = true;
		}
		if (Input.GetKeyDown(resetKey))
		{
			mh.resetMap();
		}
	}
}
