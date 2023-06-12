using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class AITrainer : MonoBehaviour
{
    [SerializeField]
    public Tuning defaultTuning;

    public GameObject boardPrefab;

    struct Game
    {
        public GameObject board;
        public MapHandler mapHandler;
        public AIInputHandler aiHandler;
        public Tuning tuning;
    }

    [Range(1, 20)] public int survivors = 10;
    [Range(2, 20)] public int mutations = 10;
    [Range(1f, 10f)] public float globalVolatility = 2; //max mutation will double or half value
    [Range(0f, 1f)] public float chanceOfMutation = 0.2f;
    public GameObject cameraGameObject;

    public Tilemap mainTilemap;
    public Tilemap ghostTilemap;
    public Tilemap uiTilemap;

    public GameObject gameParent;

    private const float verticalSpacing = 25;
    private const float horizontalSpacing = 25;
    private const int boardHeight = 20;
    private const int boardWidth = 10;

    public float checkEndedTimeToWait = 1.0f;
    private float checkEndedTimer = 0;

    public bool sendGarbage = false;
    public float garbageRampSpeed = 2.0f;
    public float garbageToSend = 0.0f;
    public float garbageSpacingTime = 15.0f;
    private float garbageSenderTimer = 0.0f;

    private Game[,] gameArray;

    private int generation = 1;

    private readonly string filePath = Application.dataPath + "/Resources/AI Training Log";
    private StreamWriter writer;

    private void outputTuningToFile(Tuning tuning, int highestScore)
    {
        String tuningText = "";
        tuningText += String.Format("Generation {0}, Score Reached: {1}\n", generation, highestScore);
        tuningText += String.Format("Low Board Importance: {0}\n", tuning.lowBoard);
        tuningText += String.Format("No Air Pockets Importance: {0}\n", tuning.noAirPockets);
        tuningText += String.Format("Lines Cleared Importance: {0}\n", tuning.linesCleared);
        tuningText += String.Format("Elevation Desparity Importance: {0}\n", tuning.elevationDesparity);
		tuningText += String.Format("Minimize Wells Importance: {0}\n", tuning.minimizeWells);
		tuningText += String.Format("Tetris Importance: {0}\n", tuning.tetris);
		tuningText += String.Format("Right Well Importance: {0}\n", tuning.rightWell);
		tuningText += String.Format("No Blocks Above Air Importance: {0}\n", tuning.noBlocksAboveAir);
		tuningText += new string('=', 25) + "\n";

        writer.Write(tuningText);
    }

    private Game createGame(Tuning tuning, Vector2 boardLocation, Vector2Int boardIndexing, int seed)
    {
        Game game = new Game();

        game.board = Instantiate(boardPrefab);
        game.board.name = String.Format("Survivor: {0}, Mutation: {1}", boardIndexing.y, boardIndexing.x);
        game.board.transform.parent = gameParent.transform;
		game.board.GetComponent<Transform>().position = boardLocation;
		game.mapHandler = game.board.GetComponent<MapHandler>();
        game.aiHandler = game.board.GetComponent <AIInputHandler>();

        game.mapHandler.mainTilemap = mainTilemap;
        game.mapHandler.ghostTilemap = ghostTilemap;
        game.mapHandler.uiTilemap = uiTilemap;

        game.aiHandler.customTuning = tuning.clone();
		game.tuning = tuning;

        game.mapHandler.boardSeed = seed;
        game.board.SetActive(true);

		return game;
    }

    private void resetGame(Game game, Tuning tuning, int seed)
    {
        game.board.SetActive(false);

        game.mapHandler.resetMap();

        game.aiHandler.customTuning = tuning.clone();
		game.tuning = tuning;

		game.board.SetActive(true);
    }

    private bool checkEnded()
    {
        for (int rowIdx = 0; rowIdx < survivors;  rowIdx++)
        {
            for (int colIdx = 0; colIdx < mutations; colIdx++)
            {
                if (!gameArray[rowIdx, colIdx].mapHandler.ended)
                    return false;
            }
        }
        return true;
    }

    private (Tuning[], int) findTopTunings()
    {
        Tuple<int, Tuning>[] evaluations = new Tuple<int, Tuning>[survivors*mutations];
        int i = 0;
        for (int rowIdx = 0; rowIdx < survivors; rowIdx++)
        {
            for (int colIdx = 0; colIdx < mutations; colIdx++)
            {
                int item1 = gameArray[rowIdx, colIdx].mapHandler.score;
                Tuning item2 = gameArray[rowIdx, colIdx].tuning.clone();
                evaluations[i++] = new Tuple<int, Tuning>(item1, item2);
            }
        }
        Array.Sort(evaluations, (x, y) => x.Item1.CompareTo(y.Item1));
        Array.Reverse(evaluations);
        int highestScore = evaluations[0].Item1;

        Tuning[] topTunings = new Tuning[survivors];
        for (int num = 0; num < survivors; num++)
        {
            topTunings[num] = evaluations[num].Item2;
        }
        return (topTunings, highestScore);
    }

	public static double NextGaussianDouble()
	{
		double u, v, S;

		do
		{
			u = 2.0 * (double)UnityEngine.Random.Range(0f, 1f) - 1.0;
			v = 2.0 * (double)UnityEngine.Random.Range(0f, 1f) - 1.0;
			S = u * u + v * v;
		}
		while (S >= 1.0);

		double fac = Math.Sqrt(-2.0 * Math.Log(S) / S);
		return u * fac;
	}

	private Tuning mutateTuning(Tuning tuning, float volatility)
    {
        Tuning newTuning = tuning.clone();

        //At 1 volatility, with a globalVolatility of 2, Random has a min of 0.5 and a max of 2
        float low = 1 - ((1.0f - (1.0f / globalVolatility)) * volatility);
        float high = 1 + ((globalVolatility - 1.0f) * volatility);
        bool mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
        if (mutate) newTuning.lowBoard = tuning.lowBoard * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.noAirPockets = tuning.noAirPockets * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.linesCleared = tuning.linesCleared * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.elevationDesparity = tuning.elevationDesparity * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.minimizeWells = tuning.minimizeWells * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.tetris = tuning.tetris * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.rightWell = tuning.rightWell * UnityEngine.Random.Range(low, high);
		mutate = UnityEngine.Random.Range(0f, 1f) <= chanceOfMutation;
		if (mutate) newTuning.noBlocksAboveAir = tuning.noBlocksAboveAir * UnityEngine.Random.Range(low, high);

		return newTuning;
	}
    void Start()
    {
		boardPrefab.SetActive(false);

        garbageToSend = 0;

		gameArray = new Game[survivors, mutations];
        int generationSeed = UnityEngine.Random.Range(0, int.MaxValue);
        for (int boardRowIdx = 0; boardRowIdx < survivors; boardRowIdx++)
        {
            float verticalPos = boardRowIdx * verticalSpacing;
            gameArray[boardRowIdx, 0] = createGame(defaultTuning, new Vector2(0, verticalPos),
                                                    new Vector2Int(0, boardRowIdx), generationSeed);
            for (int boardColIdx = 1; boardColIdx < mutations; boardColIdx++)
            {
                float horizontalPos = boardColIdx * horizontalSpacing;
                float volatility = (float)boardColIdx / ((float)mutations - 1.0f);
				Tuning tuning = mutateTuning(defaultTuning, volatility);
                gameArray[boardRowIdx, boardColIdx] = createGame(tuning, new Vector2(horizontalPos, verticalPos),
                                                                new Vector2Int(boardColIdx, boardRowIdx), generationSeed);
            }
        }

        Vector3 cameraCoords = new Vector3();
        cameraCoords.x = (horizontalSpacing * (mutations - 1) + boardWidth) / 2.0f;
        cameraCoords.y = (verticalSpacing * (survivors - 1) + boardHeight) / 2.0f;
        cameraCoords.z = -10;
        cameraGameObject.GetComponent<Transform>().position = cameraCoords;
        cameraGameObject.GetComponent<Camera>().orthographicSize = cameraCoords.y + 5;

        writer = new StreamWriter(filePath + "/" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt", true);
	}

    private void updateGames(Tuning[] survivorTuning)
    {
		int generationSeed = UnityEngine.Random.Range(0, int.MaxValue);
		for (int rowIdx = 0; rowIdx < survivors; rowIdx++)
        {
            resetGame(gameArray[rowIdx, 0], survivorTuning[rowIdx], generationSeed);
            for (int colIdx = 1; colIdx < mutations; colIdx++)
            {
                float volatility = (float)colIdx / ((float)mutations - 1.0f);
                Tuning tuning = mutateTuning(survivorTuning[rowIdx], volatility);
                resetGame(gameArray[rowIdx, colIdx], tuning, generationSeed);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        checkEndedTimer += Time.deltaTime;
        if (checkEndedTimer >= checkEndedTimeToWait)
        {
            if (checkEnded())
            {
                (Tuning[] topTunings, int highestScore) = findTopTunings();
                outputTuningToFile(topTunings[0], highestScore);
                updateGames(topTunings);
                garbageSenderTimer = 0;
                garbageToSend = 0;
                generation++;
            }
            checkEndedTimer = 0;
        }

		garbageSenderTimer += Time.deltaTime;
        garbageToSend += Time.deltaTime * (garbageRampSpeed / 100.0f);
        if (garbageSenderTimer >= garbageSpacingTime && sendGarbage)
        {
            int linesToSend = Mathf.FloorToInt(garbageToSend);
            for (int x = 0; x < mutations; x++)
            {
                for (int y = 0; y < survivors; y++)
                {
                    gameArray[x, y].mapHandler.garbageQueue.Add(linesToSend);
                }
            }
            garbageSenderTimer = 0;
        }
	}

	private void OnApplicationQuit()
	{
        writer.Close();
	}
}
