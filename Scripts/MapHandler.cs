using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class MapHandler : MonoBehaviour
{
    private System.Random randomGeneratorTetrominoQueue;
    private System.Random randomGeneratorGarbageColumn;

    public GameObject garbageRecipient;
    private MapHandler garbageRecipientMapHandler;

    public int boardSizeX = 10;  
    public int boardSizeY = 20;
    [System.NonSerialized] public Vector2Int boardCoords;
    public int boardSeed = -1;
    public TextAsset boardFile;

    public float gravity = 3f;
    public float gravityDelta = 2f;
    private float gravityTimer;
    private float timeSinceDownShift;
    public float solidificationTime = 0.5f;

    public bool ended = false;

    public bool solidifyTetrominoes = true;
	[System.NonSerialized] public Tetromino activeTetromino;
    private int activeTetrominoNum;
    private Tetromino ghostTetromino;
    private Tetromino heldTetromino;
    private Tetromino[] queuedTetrominoes;
    [System.NonSerialized] public int[] queuedTetrominoesNums;
	[System.NonSerialized] public int heldTetrominoNum;
    [System.NonSerialized] public int score;

	public GameObject cameraGameObject;

    private AIInputHandler aiInputHandler;
    private PlayerInputHandler playerInputHandler;
    public TileBase[] tiles;

    public Block[,] board;

    public Tilemap mainTilemap;
    public Tilemap ghostTilemap;
    public Tilemap uiTilemap;

    public List<int> garbageQueue;
    private int lastGarbageColumn;

    public Block updateBlock(Vector2Int coords, TetrominoColor c)
    {
        board[coords.x, coords.y].setBlockType(c);
        return board[coords.x, coords.y];
    }

    public static List<Vector2Int> moveVectors(List<Vector2Int> vectors, Vector2Int dir)
    {
        List<Vector2Int> newVectors = new List<Vector2Int>();
        foreach (var v in vectors)
        {
            newVectors.Add(v + dir);
        }
        return newVectors;
    }

    public bool canMoveDirection(Tetromino t, Vector2Int dir)
    {
        foreach(var block in t.blockList)
        {
            Vector2Int newCoords = block.coords + dir;
            if (newCoords.x < 0 || newCoords.x >= boardSizeX) return false;
            if (newCoords.y < 0 || newCoords.y >= boardSizeY+4) return false;
            if (board[newCoords.x, newCoords.y].occupied && 
            !t.blockList.Contains(board[newCoords.x, newCoords.y])) {
                return false;
            }

        }
        return true;
    }

    public bool canMoveDirectionGhost(Tetromino t, Vector2Int dir)
    {
        foreach(var block in t.blockList)
        {
            Vector2Int newCoords = block.coords + dir;
            if (newCoords.x < 0 || newCoords.x >= boardSizeX)
                return false;
            if (newCoords.y < 0 || newCoords.y >= boardSizeY+4)
                return false;
            if (board[newCoords.x, newCoords.y].occupied && 
            !activeTetromino.blockList.Contains(board[newCoords.x, newCoords.y]))
            {
                return false;
            }
        }
        return true;
    }

    public void moveTetromino(Tetromino t, Vector2Int dir, bool inBoard) {
        List<Block> newBlockList = new List<Block>();
        if (inBoard)
        {
            foreach (var block in t.blockList)
            {
                Vector2Int newCoords = block.coords + dir;
                newBlockList.Add(board[newCoords.x, newCoords.y]);
            }
        }
        t.moveDirection(dir, newBlockList, inBoard);
    }

    public static List<Vector2Int> rotateVectors(List<Vector2Int> vectors, Vector2 pivot, int quarters) {
        List<Vector2Int> newVectors = new List<Vector2Int>();
        foreach (var v in vectors) {
            Vector2 relativeCoords = v - pivot;
			switch (quarters) {
				case 1:
					relativeCoords = new Vector2(relativeCoords.y, -relativeCoords.x);
					break;
				case 2:
					relativeCoords = new Vector2(-relativeCoords.x, -relativeCoords.y);
					break;
				case 3:
					relativeCoords = new Vector2(-relativeCoords.y, relativeCoords.x);
					break;
			}
            newVectors.Add(Vector2Int.RoundToInt(relativeCoords + pivot));
		}
        return newVectors;
    }

    //checks in multiples of 90 degrees clockwise, returns the kickTable index they used, and returns -1 if no rotation is possible
    public int canRotate(Tetromino t, int quarters) {
        Vector2 pivot = t.coords + t.pivot;
        List<Vector2Int> initialVectorList = new List<Vector2Int>();
        foreach (var block in t.blockList)
            initialVectorList.Add(block.coords);

        int initialRotation = t.rotation;
        int finalRotation = (t.rotation + quarters) % 4;
        Vector2Int[] kickRow = t.wallKickRow(initialRotation, finalRotation);
        for (int i = 0; i < kickRow.Length; i++) {
            List<Vector2Int> vectors = rotateVectors(initialVectorList, pivot, quarters);
            vectors = moveVectors(vectors, kickRow[i]);
            bool valid = true;
            foreach (var newCoords in vectors) {
                if (newCoords.x < 0 || newCoords.x >= boardSizeX) valid = false;
                if (newCoords.y < 0 || newCoords.y >= boardSizeY + 4) valid = false;
                if (valid == false) break;
                if (board[newCoords.x, newCoords.y].occupied && !initialVectorList.Contains(newCoords)) valid = false;
            }
            if (valid) return i;
        }
        return -1;
    }

    public void rotateTetromino(Tetromino t, int quarters, int kickIndex) {
        List<Block> newBlockList = new List<Block>();
        List<Vector2Int> vectorList = new List<Vector2Int>();

        int initialRotation = t.rotation;
        int finalRotation = (t.rotation + quarters) % 4;
        Vector2Int[] kickRow = t.wallKickRow(initialRotation, finalRotation);

        Vector2 pivot = t.coords + t.pivot;
        foreach (var block in t.blockList)
            vectorList.Add(block.coords);

        vectorList = rotateVectors(vectorList, pivot, quarters);
        vectorList = moveVectors(vectorList, kickRow[kickIndex]);

        foreach (var newCoords in vectorList) {
			newBlockList.Add(board[newCoords.x, newCoords.y]);
		}
		t.rotate(t.coords + kickRow[kickIndex], newBlockList, quarters);
    }

    public Tetromino numberToTetromino(Vector2Int spawnLocation, Tilemap tilemap, int r)
    {
        switch (r)
        {
            case 0:
                return new LTetromino(spawnLocation, tilemap, this, tiles);
            case 1:
                return new JTetromino(spawnLocation, tilemap, this, tiles);
            case 2:
                return new ITetromino(spawnLocation, tilemap, this, tiles);
			case 3:
                return new OTetromino(spawnLocation, tilemap, this, tiles);
            case 4:
                return new STetromino(spawnLocation, tilemap, this, tiles);
            case 5:
                return new TTetromino(spawnLocation, tilemap, this, tiles);
            case 6:
                return new ZTetromino(spawnLocation, tilemap, this, tiles);
        }
        return null;
    }

    public TileBase colorToTile(TetrominoColor color)
    {
        int idx = (int)color;
        if (color == TetrominoColor.None)
            return null;
        return tiles[idx];
    }

    public void clearTetromino(Tetromino t)
    {
        foreach (var block in t.blockList)
        {
            block.setBlockType(TetrominoColor.Black);
        }
    }

    private Vector2Int spawnCoords()
    {
        Vector2 spawn = new Vector2(Mathf.Ceil(boardSizeX/2.0f)-1, boardSizeY);
        return Vector2Int.RoundToInt(spawn);
    }

    private bool isLineFull(int y)
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            if (board[x, y].occupied == false) return false;
        }
        return true;
    }

    private void moveLineDownRecursive(int y)
    {
        if (y >= boardSizeY)
        {
            return;
        }
        for (int x = 0; x < boardSizeX; x++)
        {
            board[x, y-1].setBlockType(board[x, y].color);
        }
        moveLineDownRecursive(y+1);
    }

    private void moveLineUpRecursive(int y)
    {
        if (y < 0)
        {
            return;
        }
        for (int x = 0; x < boardSizeX; x++)
        {
            board[x, y + 1].setBlockType(board[x, y].color);
        }
        moveLineUpRecursive(y-1);
    }

    private void clearLine(int y) {
        for (int x = 0; x < boardSizeX; x++) {
            board[x, y].setBlockType(TetrominoColor.Black);
        }
    }

    private void updateGhostTetromino() {
        if (ghostTetromino != null) ghostTetromino.delete();
        ghostTetromino = activeTetromino.clone(ghostTilemap);
        ghostTetromino.draw(false);
        while (canMoveDirectionGhost(ghostTetromino, Vector2Int.down)) {
            moveTetromino(ghostTetromino, Vector2Int.down, false);
        }
    }

    private void updateHoldTetromino() {
        if (heldTetromino != null) heldTetromino.delete();
        heldTetromino = numberToTetromino(new Vector2Int(-4, boardSizeY), uiTilemap, heldTetrominoNum);
        heldTetromino.draw(false);
    }

    private void updateQueuedTetrominoes() {
        for (int i = 0; i < queuedTetrominoesNums.Length; i++) {
            if (queuedTetrominoes[i] != null) queuedTetrominoes[i].delete();
            queuedTetrominoes[i] = numberToTetromino(new Vector2Int(Mathf.RoundToInt(boardSizeX + 2), 
                                                                    Mathf.RoundToInt(boardSizeY - i * 3)),
                                                                    uiTilemap,
                                                                    queuedTetrominoesNums[i]);
            queuedTetrominoes[i].draw(false);
        }
    }

    private int pullFromQueue() {
        int pulledNum = queuedTetrominoesNums[0];
        for (int i = 1; i < queuedTetrominoesNums.Length; i++) {
            queuedTetrominoesNums[i - 1] = queuedTetrominoesNums[i];
        }
        queuedTetrominoesNums[queuedTetrominoesNums.Length-1] = randomGeneratorTetrominoQueue.Next(0, 7);
        updateQueuedTetrominoes();
        return pulledNum;
    }

    void Start()
    {
        randomGeneratorTetrominoQueue = boardSeed == -1 ? new System.Random() : new System.Random(boardSeed);
        randomGeneratorGarbageColumn = boardSeed == -1 ? new System.Random() : new System.Random(boardSeed);

        heldTetrominoNum = -1;
        score = 0;
        gravityTimer = 0;
        boardCoords = new Vector2Int(Mathf.RoundToInt(transform.position.x), Mathf.RoundToInt(transform.position.y));
        if (garbageRecipient != null)
        {
            garbageRecipientMapHandler = garbageRecipient.GetComponent<MapHandler>();
        }

        //Adjust camera position
        if (cameraGameObject != null)
        {
            Transform cameraTf = cameraGameObject.GetComponent<Transform>();
            cameraTf.position = new Vector3(boardCoords.x + boardSizeX / 2.0f, 
                boardCoords.y + (boardSizeY + 4) / 2.0f, -10);
            Camera cameraComponent = cameraGameObject.GetComponent<Camera>();
            cameraComponent.orthographicSize = ((boardSizeY + 4) / 2.0f) + 3;
        }

        //Get Components
        aiInputHandler = GetComponent<AIInputHandler>();
        playerInputHandler = GetComponent<PlayerInputHandler>();

        //Setup map
        board = new Block[boardSizeX, boardSizeY + 4];

        TetrominoColor[,] boardColor = MapConstructor.constructBoard(boardSizeX, boardSizeY, boardFile);

        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
            {
                TetrominoColor c = boardColor != null ? boardColor[x, y] : TetrominoColor.Black;
                board[x, y] = new Block(new Vector2Int(x, y), c, mainTilemap, tiles, boardCoords);
            }
            for (int y = boardSizeY; y < boardSizeY + 4; y++)
            {
				board[x, y] = new Block(new Vector2Int(x, y), TetrominoColor.None, mainTilemap, tiles, boardCoords);
			}
        }

        queuedTetrominoesNums = new int[5];
        queuedTetrominoes = new Tetromino[5];
        for (int i = 0; i < queuedTetrominoesNums.Length; i++) {
            queuedTetrominoesNums[i] = randomGeneratorTetrominoQueue.Next(0, 7);
        }
        activeTetrominoNum = pullFromQueue();
        activeTetromino = numberToTetromino(spawnCoords(), mainTilemap, activeTetrominoNum);
        activeTetromino.draw(true);
        updateGhostTetromino();
        garbageQueue = new List<int>();
	}  

    private bool isGameOver()
    {
        for (int x = 0; x < boardSizeX; x++)
        {
            if (board[x, boardSizeY].occupied)
            {
                bool blockPartOfActiveTetromino = false;
                foreach (var block in activeTetromino.blockList)
                {
                    if (block.coords == new Vector2Int(x, boardSizeY))
                        blockPartOfActiveTetromino = true;
                }
                if (!blockPartOfActiveTetromino)
                    return true;
            }
        }
        return false;
    }

    void placeGarbageLine(int x)
    {
        moveLineUpRecursive(boardSizeY - 1);
        for (int i = 0; i < boardSizeX; i++)
        {
            TetrominoColor color = i == x ? TetrominoColor.Black : TetrominoColor.Gray;
            board[i, 0].setBlockType(color);
        }
    }

    private int linesClearedToGarbageLines(int linesCleared)
    {
		switch (linesCleared)
		{
			case 2:
                return 1;
			case 3:
				return 2;
			case 4:
                return 4;
		}
        return 0;
	}

    private void subtractGarbageLines(ref int garbageLines)
    {
        if (garbageLines <= 0)
        {
            return;
        }

		while (garbageLines > 0 && garbageQueue.Count > 0)
		{
			if (garbageQueue[0] <= garbageLines)
			{
				garbageLines -= garbageQueue[0];
				garbageQueue.RemoveAt(0);
			}
			else
			{
				garbageQueue[0] -= garbageLines;
                garbageLines = 0;
			}
		}
	}


    private bool processGarbage()
    {
        if (garbageQueue.Count <= 0)
        {
            return false;
        }

        int linesOfGarbage = garbageQueue[0];
        garbageQueue.RemoveAt(0);
        int garbageColumn = Random.Range(0, boardSizeX - 1);
        if (garbageColumn >= lastGarbageColumn)
        {
            garbageColumn++;
        }
        lastGarbageColumn = garbageColumn;
        for (int i = 0; i < linesOfGarbage; i++)
        {
            placeGarbageLine(garbageColumn);
            if (isGameOver())
                return true;
        }
        return false;
    }

    void sendGarbage(int garbageLines, MapHandler mh)
    {
        if (mh == null)
        {
            return;
        }
        mh.garbageQueue.Add(garbageLines);
    }

    private void updateScore(int lines)
    {
        int scoreDelta = 0;
        switch (lines)
        {
            case 1: scoreDelta = 1; break;
            case 2: scoreDelta = 3; break;
            case 3: scoreDelta = 5; break;
            case 4: scoreDelta = 8; break;
        }
        score += scoreDelta;
    }

    bool resetRound(bool holdUsed)
    {
        int linesCleared = 0;
        for (int y = 0; y < boardSizeY+3; y++) {
			if (isLineFull(y))
			{
                linesCleared++;
				clearLine(y);
				moveLineDownRecursive(y + 1);
				y--;
			}   
        }
        activeTetromino.blockList.Clear();
        if (isGameOver()) return true;
        updateScore(linesCleared);

        int garbageLines = linesClearedToGarbageLines(linesCleared);
        subtractGarbageLines(ref garbageLines);
        if (linesCleared <= 0 && !holdUsed)
        {
            if (processGarbage())
            {
                return true;
            }
        }
        sendGarbage(linesCleared, garbageRecipientMapHandler);

        activeTetrominoNum = pullFromQueue();
        activeTetromino = numberToTetromino(spawnCoords(), mainTilemap, activeTetrominoNum);
        activeTetromino.draw(true);
        updateGhostTetromino();
        return false;
    }

    public void gameOver()
    {
        ended = true;
        mainTilemap.RefreshAllTiles();
        ghostTilemap.RefreshAllTiles();
        uiTilemap.RefreshAllTiles();
        enabled = false;
    }

    public void updateBoard() {
        if (isGameOver()) return;
        if (canMoveDirection(activeTetromino, Vector2Int.down))
        {   
            moveTetromino(activeTetromino, Vector2Int.down, true);
            updateGhostTetromino();
            timeSinceDownShift = 0;
        }
        if (timeSinceDownShift >= solidificationTime && solidifyTetrominoes)
        {
            if (playerInputHandler != null)
                playerInputHandler.resetVariables();
            if (aiInputHandler != null)
                aiInputHandler.resetVariables();
			if (resetRound(false))
            {
                gameOver();
            }
        }
    }

    public void attemptMoveLeft() {
		if (isGameOver()) return;
		if (canMoveDirection(activeTetromino, Vector2Int.left))
        {
            moveTetromino(activeTetromino, Vector2Int.left, true);
            if (playerInputHandler != null)
                playerInputHandler.timeSinceLeftRightShift = 0;
            if (aiInputHandler != null)
                aiInputHandler.timeSinceLeftRightShift = 0;
            updateGhostTetromino();
        }
    }

	public void attemptMoveRight() {
		if (isGameOver()) return;
		if (canMoveDirection(activeTetromino, Vector2Int.right))
        {
			moveTetromino(activeTetromino, Vector2Int.right, true);
            if (playerInputHandler != null)
                playerInputHandler.timeSinceLeftRightShift = 0;
            if (aiInputHandler != null)
                aiInputHandler.timeSinceLeftRightShift = 0;
            updateGhostTetromino();
		}
	}

	public void attemptRotateClockwise() {
		if (isGameOver()) return;
		int kickIndex = canRotate(activeTetromino, 1);
        if (kickIndex >= 0)
        {
            rotateTetromino(activeTetromino, 1, kickIndex);
            updateGhostTetromino();
        }
    }

    public void attemptRotateCounterClockwise() {
		if (isGameOver()) return;
		int kickIndex = canRotate(activeTetromino, 3);
        if (kickIndex >= 0)
        {
            rotateTetromino(activeTetromino, 3, kickIndex);
            updateGhostTetromino();
        }
    }

	public void hardDrop() {
		if (isGameOver()) return;
		while (canMoveDirection(activeTetromino, Vector2Int.down))
        {
			moveTetromino(activeTetromino, Vector2Int.down, true);
		}
        if (resetRound(false))
        {
            gameOver();
        }
        timeSinceDownShift = 0;
	}

    public void useHold() {
		if (isGameOver()) return;
		clearTetromino(activeTetromino);
        if (heldTetrominoNum == -1)
        {
            heldTetrominoNum = activeTetrominoNum;
            if (resetRound(true))
            {
                gameOver();
            }
        } else {
            (heldTetrominoNum, activeTetrominoNum) = (activeTetrominoNum, heldTetrominoNum);
            activeTetromino = numberToTetromino(spawnCoords(), mainTilemap, activeTetrominoNum);
            activeTetromino.draw(true);
        }
        updateGhostTetromino();
        updateHoldTetromino();
        timeSinceDownShift = 0;
    }

	public void resetMap()
	{
        Random.InitState(boardSeed);
        for (int x = 0; x < boardSizeX; x++)
        {
            for (int y = 0; y < boardSizeY; y++)
                updateBlock(new Vector2Int(x, y), TetrominoColor.Black);
            for (int y = boardSizeY; y < boardSizeY + 4; y++)
                updateBlock(new Vector2Int(x, y), TetrominoColor.None);
        }

		for (int i = 0; i < queuedTetrominoesNums.Length; i++)
			queuedTetrominoesNums[i] = randomGeneratorTetrominoQueue.Next(0, 7);
        
		activeTetrominoNum = pullFromQueue();
		activeTetromino = numberToTetromino(spawnCoords(), mainTilemap, activeTetrominoNum);
		activeTetromino.draw(true);
		updateGhostTetromino();
        garbageQueue.Clear();
        timeSinceDownShift = 0;
        gravity = 3.0f;
        enabled = true;
        ended = false;
        score = 0;
	}

	void Update()
	{
        gravityTimer += Time.deltaTime;
        timeSinceDownShift += Time.deltaTime;
		gravity += gravityDelta / 100.0f * Time.deltaTime;

		if (gravityTimer >= 1.0f/gravity)
        {
            updateBoard();
            gravityTimer = 0;
        }
	}
}
