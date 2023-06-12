using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tuning
{
	public float lowBoard;
	public float noAirPockets;
	public float linesCleared;
	public float elevationDesparity;
	public float minimizeWells;
	public float tetris;
	public float rightWell;
	public float noBlocksAboveAir;

	public Tuning clone()
	{
		Tuning t = new Tuning();
		t.lowBoard = lowBoard;
		t.noAirPockets = noAirPockets;
		t.linesCleared = linesCleared;
		t.elevationDesparity = elevationDesparity;
		t.minimizeWells = minimizeWells;
		t.tetris = tetris;
		t.rightWell = rightWell;
		t.noBlocksAboveAir = noBlocksAboveAir;
		return t;
	}
}