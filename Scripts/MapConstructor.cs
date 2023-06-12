using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapConstructor : MonoBehaviour
{
    private const char red = 'R';
    private const char orange = 'O';
    private const char yellow = 'Y';
    private const char green = 'G';
    private const char blue = 'B';
    private const char cyan = 'C';
    private const char purple = 'P';
    private const char none = '.';

	public static TetrominoColor[,] constructBoard(int intendedX, int intendedY, TextAsset boardFile)
    {
		Dictionary<char, TetrominoColor> colorDict = new Dictionary<char, TetrominoColor>
		{
			{ red, TetrominoColor.Red },
			{ orange, TetrominoColor.Orange },
			{ yellow, TetrominoColor.Yellow },
			{ green, TetrominoColor.Green },
			{ blue, TetrominoColor.Blue },
			{ cyan, TetrominoColor.Cyan },
			{ purple, TetrominoColor.Purple },
			{ none, TetrominoColor.Black }
		};

        string[] splits = new string[] { "\r\n", "\r", "\n" };

		if (boardFile == null) return null;

        string[] lines = boardFile.text.Split(splits, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length != intendedY) return null;


        TetrominoColor[,] board = new TetrominoColor[intendedX, intendedY];
        for (int lineIdx = 0; lineIdx < lines.Length; lineIdx++)
        {
            string line = lines[lineIdx];
            if (line.Length != intendedX) return null; 
            for (int charIdx = 0; charIdx < line.Length; charIdx++)
            {
                char c = line[charIdx];
                if (!colorDict.ContainsKey(c)) return null;
                board[charIdx, intendedY-1-lineIdx] = colorDict[c];
            }
        }

        return board;
    }
}
