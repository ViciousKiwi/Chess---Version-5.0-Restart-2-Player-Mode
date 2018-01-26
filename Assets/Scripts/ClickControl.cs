using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickControl : MonoBehaviour 
{
	public enum WINNER
	{
		BLACK = -1,
		EMPTY = 0,
		WHITE = 1
	}

	public WINNER whoWins = WINNER.EMPTY;

	const int MAXIMUMSCORE = 1000 + 9 + 5*2 +3*2 + 3*2 + 1*8;
	const int boardSize = 8;

	float xSizeOfTile;

	[HideInInspector]
	public GridSpawner BoardGrid;
	[HideInInspector]
	public PieceSpawner PiecesGrid;	

	int xPos;
	int yPos;

	public PieceInfo selectedPiece;

	[HideInInspector]
	public Vector2 newPosition = new Vector2();

	public PieceInfo.PIECECOLOR playerColorTurn; // black is -1, white is 1, 0 is empty
	public bool hasPlayerMoved;

	public List<BoardInfo> targetedTilesList = new List<BoardInfo>();

	public List<PieceInfo> whitePieces = new List<PieceInfo>();
	public List<PieceInfo> blackPieces = new List<PieceInfo>();

	public List<PieceInfo> movableWhitePieces = new List<PieceInfo>();
	public List<PieceInfo> movableBlackPieces = new List<PieceInfo>();

	//minimax
	public List<PieceInfo> nextBoardPositions = new List<PieceInfo>();

	public bool hasGameStarted = false;
	public bool twoPlayerMode = false;

	public int depthLimit = 3;

	public GameObject UI;
	public GameObject RestartUI;

	public PieceInfo[,] futurePiecesGrid = new PieceInfo[boardSize, boardSize];

	//Data Saving
	//public Vector2

	public PieceDataStorage[,] pieceData = new PieceDataStorage[boardSize,boardSize];

	public GameObject piecePrefab;
	public Transform futurePieces;

	void InitFutureBoard()
	{
		for(int j=0; j<boardSize; j++)
		{
			for(int i=0; i<boardSize; i++)
			{
				PieceInfo go = Instantiate(piecePrefab, new Vector3(i, // horCount / 2.0f * gridSize is offset
					j, 0.0f), Quaternion.identity, futurePieces).GetComponent<PieceInfo>(); 

				//Default pieceType
				go.pieceType = PieceInfo.PIECETYPE.EMPTY;
				go.pieceColor = PieceInfo.PIECECOLOR.EMPTY;
				go.pieceValue = 0;

				go.x = i;
				go.y = j;

				futurePiecesGrid[i, j] = go;
			}
		}
	}

	void InitDataStorage()
	{
		for (int j = 0; j < boardSize; j++)
		{
			for (int i = 0; i < boardSize; i++)
			{
				pieceData[i,j].pieceTypeData = (int)futurePiecesGrid[i, j].pieceType;
				pieceData[i,j].pieceColorData = (int)futurePiecesGrid[i, j].pieceColor;
				pieceData[i,j].pieceValueData = (int)futurePiecesGrid[i, j].pieceValue;
			}
		}
	}

	void Start () 
	{
		RestartUI.GetComponentInChildren<Text>().text = null;
		RestartUI.SetActive(false);
		hasGameStarted = false;
		xSizeOfTile = 14.224f;

		twoPlayerMode = false;

		playerColorTurn = PieceInfo.PIECECOLOR.WHITE;
		hasPlayerMoved = false;

		FindAllBoardPieces();

		InitFutureBoard();
		//InitDataStorage();

		//test
		GenerateNewFutureBoard();
		//InitDataStorage();
	}

	void GenerateNewFutureBoard()
	{
		for (int j = 0; j < boardSize; j++)
		{
			for (int i = 0; i < boardSize; i++)
			{
				futurePiecesGrid[i,j].pieceType = PiecesGrid.piecesGrid[i, j].pieceType;
				futurePiecesGrid[i,j].pieceColor = PiecesGrid.piecesGrid[i, j].pieceColor;
				futurePiecesGrid[i,j].pieceValue = PiecesGrid.piecesGrid[i, j].pieceValue;
			}
		}
	}

	void GenerateNewStorage()
	{
		for (int j = 0; j < boardSize; j++)
		{
			for (int i = 0; i < boardSize; i++)
			{
				pieceData[i,j].pieceTypeData = (int)futurePiecesGrid[i, j].pieceType;
				pieceData[i,j].pieceColorData = (int)futurePiecesGrid[i, j].pieceColor;
				pieceData[i,j].pieceValueData = (int)futurePiecesGrid[i, j].pieceValue;
			}
		}
	}

	void DisplayFutureBoard()
	{
		for(int j=0; j<boardSize; j++)
		{
			for(int i=0; i<boardSize; i++)
			{
				futurePiecesGrid[i, j].pieceType = (PieceInfo.PIECETYPE)pieceData[i, j].pieceTypeData;
				futurePiecesGrid[i, j].pieceColor = (PieceInfo.PIECECOLOR)pieceData[i, j].pieceColorData;
				futurePiecesGrid[i, j].pieceValue = pieceData[i, j].pieceValueData;
			}
		}
	}

	void DisplayWinner()
	{
		hasGameStarted = false;
		RestartUI.SetActive(true);
		RestartUI.GetComponentInChildren<Text>().text = whoWins + " PLAYER WINS!";
	}

	public void RestartButton()
	{
		ClearCurrentPossibleMoves();
		ClearCurrentlyMovablePieces();
		ClearTargetableTiles();

		CalculateScore(PiecesGrid);
		whoWins = WINNER.EMPTY;
		playerColorTurn = PieceInfo.PIECECOLOR.WHITE;


		PiecesGrid.ResetPieces();
		PiecesGrid.PiecesSprites();

		RestartUI.SetActive(false);
		hasPlayerMoved = false;
		hasGameStarted = true;
	}

	public void SinglePlayerMode()
	{
		twoPlayerMode = false;
		hasGameStarted = true;

		UI.SetActive(false);
	}

	public void TwoPlayerMode()
	{
		twoPlayerMode = true;
		hasGameStarted = true;

		UI.SetActive(false);
	}

	void Update () 
	{
		if (hasGameStarted)
		{
			xPos = (int) (Mathf.Ceil(Input.mousePosition.x/(1920.0f/xSizeOfTile) - 1.0f));
			yPos = (int) Mathf.Ceil(Input.mousePosition.y/(1080.0f/boardSize) - 1.0f);

			if (twoPlayerMode == false)
			{
				if (Input.GetMouseButtonUp(0))
				{
					if (playerColorTurn == PieceInfo.PIECECOLOR.WHITE)
					{
						GetSinglePlayerInput((int)playerColorTurn);
					}
				}

				if (playerColorTurn == PieceInfo.PIECECOLOR.BLACK && hasPlayerMoved == true)
				{
					AIChooseRandom();
				}
			}
			else
			{
				if (Input.GetMouseButtonUp(0))
				{
					GetTwoPlayerInput((int)playerColorTurn);
				}
			}

			if (Input.GetMouseButtonUp(1))
			{
			//	GenerateNewBoard();
			}

			if (Input.GetKeyDown(KeyCode.B))
			{
				FindNextBoardPositions((int)playerColorTurn);
			}

			if (Input.GetKeyDown(KeyCode.G))
			{
				GenerateNewFutureBoard();
				GenerateNewStorage();

			}
		}
	}

	void FindNextBoardPositions(int playerColor)
	{
		nextBoardPositions.Clear();
		FindCurrentlyMovablePieces();

		if(playerColor == -1)
		{
			foreach(PieceInfo movablePiece in movableBlackPieces)
			{
				nextBoardPositions.Add(movablePiece);
			}
		}
		else 
		{
			foreach(PieceInfo movablePiece in movableWhitePieces)
			{
				nextBoardPositions.Add(movablePiece);
			}
		}
	}

	//boardPosition is the entire BoardGrid
	PieceInfo MiniMax (PieceInfo boardPosition)
	{
		int highestScore = int.MinValue;
		int highestScoreIndex = -1;

		//nextBoardPositions = FindNextBoardPositions();

		for(int i=0; i<nextBoardPositions.Count; i++)
		{
			//not sure /// nahhh its totally wrong
			//boardPosition = nextBoardPositions[i];
			int score = Min(boardPosition, 0);
			if(score > highestScore)
			{
				highestScore = score;
				highestScoreIndex = i;
			}
		}

		return nextBoardPositions[highestScoreIndex];
	}

	//may want to change BoardINfo to PieceInfo
	int Max(BoardInfo boardPosition, int depth)
	{
		FindCurrentlyMovablePieces();

		if (depth >= depthLimit || CheckEndGame(futurePiecesGrid))
		{
			//return CalculateScore(FuturePiecesGrid.piecesGrid);
		}

		int highestScore = int.MinValue;

		//nextBoardPositions = ;
		
		foreach( PieceInfo boardPositions in nextBoardPositions)
		{
			int score = Min(boardPositions, depth+1);
			if(score > highestScore)
				highestScore = score;
		}
		return highestScore;
	}

	int Min(PieceInfo piecePosition, int depth)
	{
		if (depth >= depthLimit || CheckEndGame(futurePiecesGrid))
		{
			//return CalculateScore(piecePosition);
		}

		int lowestScore = int.MaxValue;

		//nextBoardPositions = FindAllBoardPieces(); //(boardPosition, false)

		foreach( PieceInfo piecePositions in nextBoardPositions)
		{
			int score = Min(piecePositions, depth+1);
			if(score < lowestScore)
				lowestScore = score;
		}

		return lowestScore;
	}

	int CalculateScore(PieceSpawner grid)
	{
		int calculatedTotalScore = 0, totalBlackScore = 0, totalWhiteScore = 0;

		FindAllBoardPieces();

		foreach (PieceInfo blackPiece in blackPieces)
		{
			totalBlackScore += (int)blackPiece.pieceValue;
		}

		foreach (PieceInfo whitePiece in whitePieces)
		{
			totalWhiteScore -= (int)whitePiece.pieceValue;
		}

		calculatedTotalScore = totalBlackScore + totalWhiteScore;

		return calculatedTotalScore;
	}

	bool CheckEndGame(PieceInfo[,] nextPieceGrid)
	{
		int kingCount = 0;
		bool isGameEnd = false;

		//if king is dead / no possible moves can be made
		for (int j = 0; j < boardSize; j++)
		{
			for (int i = 0; i < boardSize; i++)
			{

				if (nextPieceGrid[i, j].pieceType == PieceInfo.PIECETYPE.KING)
				{
					kingCount++;
				}
			}
		}

		if (kingCount < 2)
		{
			isGameEnd = true;
		}

		int currentScore = CalculateScore(PiecesGrid);
		if (currentScore >= 1000-39 || currentScore <= -1000+39)
		{
			isGameEnd = true;
			if (currentScore >= 1000-39)
			{
				whoWins = WINNER.BLACK;
			}
			else if (currentScore <= -1000+39)
			{
				whoWins = WINNER.WHITE;
			}

			DisplayWinner();
		}

		return isGameEnd;
	}

	void AIChooseRandom()
	{
		FindCurrentlyMovablePieces();

		int chosenPieceIndex = Random.Range(0, movableBlackPieces.Count-1);
		PieceInfo chosenPiece = movableBlackPieces[chosenPieceIndex];

		int chosenMoveIndex = Random.Range(0, chosenPiece.currentPossibleMovesList.Count-1);
		BoardInfo chosenMoveLocation = chosenPiece.currentPossibleMovesList[chosenMoveIndex];

		PiecesGrid.piecesGrid[chosenMoveLocation.x,chosenMoveLocation.y].pieceType = chosenPiece.pieceType;
		PiecesGrid.piecesGrid[chosenMoveLocation.x,chosenMoveLocation.y].pieceColor = chosenPiece.pieceColor;
		PiecesGrid.piecesGrid[chosenMoveLocation.x,chosenMoveLocation.y].pieceValue = chosenPiece.pieceValue;

		chosenPiece.pieceType = PieceInfo.PIECETYPE.EMPTY;
		chosenPiece.pieceColor = PieceInfo.PIECECOLOR.EMPTY;
		chosenPiece.currentPossibleMovesList.Clear();
		chosenPiece = null;

		playerColorTurn = PieceInfo.PIECECOLOR.WHITE;
		hasPlayerMoved = false;

		//refresh sprites
		PiecesGrid.PiecesSprites();

		CheckEndGame(PiecesGrid.piecesGrid);
	}

	void MoveAI()
	{
		//FindAllPossibleMoves();

		//NextPlayerTurn((int)playerColorTurn);
	}

	void GetSinglePlayerInput(int playerColor) // black is -1, white is +1
	{
		if (xPos >= 0 && yPos >= 0 && xPos < boardSize && yPos < boardSize) // prevents accessing invalid range
		{
			//on top to make sure it checks before the others. This will clear the map if they choose back their own tile
			if (selectedPiece != null && selectedPiece.pieceColor == playerColorTurn && BoardGrid.boardTileGrid[xPos,yPos].isTargeted == false) 	ClearTargetableTiles();

			//sets selected piece
			//if the piece selected is the player's color, select that piece
			if (PiecesGrid.piecesGrid[xPos,yPos].pieceColor == playerColorTurn)
			{
				selectedPiece = PiecesGrid.piecesGrid[xPos,yPos];

				PlayerMoves(playerColor);	
			}

			//applies selected piece to the target
			//if player selects a targetable boardposition AND the position is not their own color, 'move' the selected tile to the targeted tile
			else if (BoardGrid.boardTileGrid[xPos,yPos].isTargeted == true && PiecesGrid.piecesGrid[xPos,yPos].pieceColor != playerColorTurn)
			{
				PiecesGrid.piecesGrid[xPos,yPos].pieceType = selectedPiece.pieceType;
				PiecesGrid.piecesGrid[xPos,yPos].pieceColor = selectedPiece.pieceColor;
				PiecesGrid.piecesGrid[xPos,yPos].pieceValue = selectedPiece.pieceValue;

				selectedPiece.pieceType = PieceInfo.PIECETYPE.EMPTY;
				selectedPiece.pieceColor = PieceInfo.PIECECOLOR.EMPTY;
				selectedPiece.currentPossibleMovesList.Clear();
				selectedPiece = null;

				ClearTargetableTiles();
				playerColorTurn = PieceInfo.PIECECOLOR.BLACK;
				hasPlayerMoved = true;

				//refresh sprites
				PiecesGrid.PiecesSprites();

				FindAllBoardPieces();
			}
			CheckEndGame(PiecesGrid.piecesGrid);
		}
	}

//===================================================== 2 Player Mode
	void GetTwoPlayerInput(int playerColor) // black is -1, white is +1
	{
		if (xPos >= 0 && yPos >= 0 && xPos < boardSize && yPos < boardSize) // prevents accessing invalid range
		{
			//on top to make sure it checks before the others. This will clear the map if they choose back their own tile
			if (selectedPiece != null && selectedPiece.pieceColor == playerColorTurn && BoardGrid.boardTileGrid[xPos,yPos].isTargeted == false) 	ClearTargetableTiles();

			//sets selected piece
			//if the piece selected is the player's color, select that piece
			if (PiecesGrid.piecesGrid[xPos,yPos].pieceColor == playerColorTurn)
			{
				selectedPiece = PiecesGrid.piecesGrid[xPos,yPos];

				PlayerMoves(playerColor);	
			}

			//applies selected piece to the target
			//if player selects a targetable boardposition AND the position is not their own color, 'move' the selected tile to the targeted tile
			else if (BoardGrid.boardTileGrid[xPos,yPos].isTargeted == true && PiecesGrid.piecesGrid[xPos,yPos].pieceColor != playerColorTurn)
			{
				PiecesGrid.piecesGrid[xPos,yPos].pieceType = selectedPiece.pieceType;
				PiecesGrid.piecesGrid[xPos,yPos].pieceColor = selectedPiece.pieceColor;
				PiecesGrid.piecesGrid[xPos,yPos].pieceValue = selectedPiece.pieceValue;

				selectedPiece.pieceType = PieceInfo.PIECETYPE.EMPTY;
				selectedPiece.pieceColor = PieceInfo.PIECECOLOR.EMPTY;
				selectedPiece = null;

				ClearTargetableTiles();
				playerColorTurn = (PieceInfo.PIECECOLOR)(playerColor * -1);

				//refresh sprites
				PiecesGrid.PiecesSprites();

				CheckEndGame(PiecesGrid.piecesGrid);
			}
		}
	}

	void FindCurrentlyMovablePieces()
	{
		ClearCurrentlyMovablePieces();

		FindAllBoardPieces();
		FindAllPossibleMoves();

		//black
		foreach (PieceInfo blackPiece in blackPieces)
		{
			if (blackPiece.currentPossibleMovesList.Count > 0)
			{
				movableBlackPieces.Add(blackPiece);
			}
		}

		//white
		foreach (PieceInfo whitePiece in whitePieces)
		{
			if (whitePiece.currentPossibleMovesList.Count > 0)
			{
				movableWhitePieces.Add(whitePiece);
			}
		}
	}

	void ClearCurrentlyMovablePieces()
	{
		movableBlackPieces.Clear();
		movableWhitePieces.Clear();
	}

	void GetAllBoardPositions(BoardInfo curBoardPos, bool isblackPlayer)
	{
		ClearAllBoardPieceLists();

		if (isblackPlayer)
		{
			blackPieces.Clear();

			for (int j = 0; j<boardSize; j++)
			{
				for (int i = 0; i<boardSize; i++)
				{
					if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						blackPieces.Add(PiecesGrid.piecesGrid[i, j]);
					}
				}
			}
		}
		else
		{
			whitePieces.Clear();

			for (int j = 0; j<boardSize; j++)
			{
				for (int i = 0; i<boardSize; i++)
				{
					if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.WHITE)
					{
						whitePieces.Add(PiecesGrid.piecesGrid[i, j]);
					}
				}
			}
		}
	}

	void FindAllBoardPieces()
	{
		ClearAllBoardPieceLists();
		for (int j = 0; j<boardSize; j++)
		{
			for (int i = 0; i<boardSize; i++)
			{
				if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					whitePieces.Add(PiecesGrid.piecesGrid[i,j]);
				}
				else if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					blackPieces.Add(PiecesGrid.piecesGrid[i,j]);
				}
			}
		}
	}

	void FindFutureBoardPieces(PieceSpawner futureGrid)
	{
		ClearAllBoardPieceLists();
		for (int j = 0; j<boardSize; j++)
		{
			for (int i = 0; i<boardSize; i++)
			{
				if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					whitePieces.Add(PiecesGrid.piecesGrid[i,j]);
				}
				else if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					blackPieces.Add(PiecesGrid.piecesGrid[i,j]);
				}
			}
		}
	}

	void FindAllBlackPieces()
	{
		blackPieces.Clear();

		for (int j = 0; j<boardSize; j++)
		{
			for (int i = 0; i<boardSize; i++)
			{
				if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					blackPieces.Add(PiecesGrid.piecesGrid[i,j]);
				}
			}
		}
	}

	void FindAllWhitePieces()
	{
		whitePieces.Clear();

		for (int j = 0; j<boardSize; j++)
		{
			for (int i = 0; i<boardSize; i++)
			{
				if (PiecesGrid.piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					whitePieces.Add(PiecesGrid.piecesGrid[i,j]);
				}
			}
		}
	}

	void FindPossibleMovesForColor(int playerColor)
	{
		//clear all previous posible moves
		ClearCurrentPossibleMoves();

		//generate new current moves
		foreach (PieceInfo piece in blackPieces)
		{
			if (piece.pieceType == PieceInfo.PIECETYPE.PAWN)
			{
				AIPawnMovement(playerColor, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.KNIGHT)
			{
				AIKnightMovement(playerColor, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.BISHOP)
			{
				AIBishopMovement(playerColor, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.ROOK)
			{
				AIRookMovement(playerColor, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.QUEEN)
			{
				AIQueenMovement(playerColor, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.KING)
			{
				AIKingMovement(piece, 1, playerColor);
			}
		}
	}

	void FindAllPossibleMoves()
	{
		//clear all previous posible moves
		ClearCurrentPossibleMoves();

		//generate new current moves
		foreach (PieceInfo piece in blackPieces)
		{
			if (piece.pieceType == PieceInfo.PIECETYPE.PAWN)
			{
				AIPawnMovement(-1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.KNIGHT)
			{
				AIKnightMovement(-1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.BISHOP)
			{
				AIBishopMovement(-1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.ROOK)
			{
				AIRookMovement(-1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.QUEEN)
			{
				AIQueenMovement(-1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.KING)
			{
				AIKingMovement(piece, 1, -1);
			}
		}

		foreach (PieceInfo piece in whitePieces)
		{
			if (piece.pieceType == PieceInfo.PIECETYPE.PAWN)
			{
				AIPawnMovement(1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.KNIGHT)
			{
				AIKnightMovement(1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.BISHOP)
			{
				AIBishopMovement(1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.ROOK)
			{
				AIRookMovement(1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.QUEEN)
			{
				AIQueenMovement(1, piece);
			}

			else if (piece.pieceType == PieceInfo.PIECETYPE.KING)
			{
				AIKingMovement(piece, 1, 1);
			}
		}
	}

	void ClearAllBoardPieceLists()
	{
		blackPieces.Clear();
		whitePieces.Clear();
	}

	//=============================================== Clear TargetList
	void ClearTargetableTiles()
	{
		foreach (BoardInfo targetedTile in targetedTilesList)
		{
			targetedTile.isTargeted = false;
		}
		targetedTilesList.Clear();
	}

	//=============================================== Clear currentPossibleMovesList
	void ClearCurrentPossibleMoves()
	{
		for (int j = 0; j < boardSize; j++)
		{
			for (int i = 0; i<boardSize; i++)
			{
				PiecesGrid.piecesGrid[i, j].currentPossibleMovesList.Clear();
			}
		}
	}

	void PlayerMoves(int playerColor)
	{
		FindTargetableTiles(playerColor);
	}

//================================================= Find Targets
	void FindTargetableTiles(int playerColor)
	{
		if (selectedPiece.pieceType == PieceInfo.PIECETYPE.PAWN)
		{
			PawnMovement(playerColor);
		}
		else if (selectedPiece.pieceType == PieceInfo.PIECETYPE.KNIGHT)
		{
			KnightMovement(playerColor);
		}
		else if (selectedPiece.pieceType == PieceInfo.PIECETYPE.BISHOP)
		{
			BishopMovement(playerColor);
		}
		else if (selectedPiece.pieceType == PieceInfo.PIECETYPE.ROOK)
		{
			RookMovement(playerColor);
		}
		else if (selectedPiece.pieceType == PieceInfo.PIECETYPE.QUEEN)
		{
			QueenMovement(playerColor);
		}
		else if (selectedPiece.pieceType == PieceInfo.PIECETYPE.KING)
		{
			KingMovement(1);
		}
	}

//================================================ AI Movements

	void AIPawnMovement(int playerColor, PieceInfo piece) //black is -1, white is 1
	{
		//Dont change the PieceColor.Black and Whites here. Must be fixed
		if (piece != null)
		{
			// Forward 1
			if (playerColor == 1 && piece.y+1 < boardSize && PiecesGrid.piecesGrid[piece.x, piece.y+1].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				Vector2 upPiece = new Vector2();
				upPiece = ReturnedCheckUP(piece, 1);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upPiece.x, (int)upPiece.y]);
			}
			else if (playerColor == -1 && piece.y-1 >= 0 && PiecesGrid.piecesGrid[piece.x, piece.y-1].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				Vector2 downPiece = new Vector2();
				downPiece = ReturnedCheckDOWN(piece, 1);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downPiece.x, (int)downPiece.y]);
			}

			//Double Forward
			//if its y = 1, white  || if y = 6, black
			if (piece.y == 1 && piece.pieceColor == PieceInfo.PIECECOLOR.WHITE && piece.y+2 < boardSize && PiecesGrid.piecesGrid[piece.x, piece.y+2].pieceColor == PieceInfo.PIECECOLOR.EMPTY && piece.y+1 <boardSize && PiecesGrid.piecesGrid[piece.x, piece.y+1].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				if (PiecesGrid.piecesGrid[piece.x, piece.y+2].pieceType == PieceInfo.PIECETYPE.EMPTY)
				{
					piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x, piece.y+2]);
				}
			}
			else if (piece.y == 6 && piece.pieceColor == PieceInfo.PIECECOLOR.BLACK && piece.y-2 >= 0 && PiecesGrid.piecesGrid[piece.x, piece.y-2].pieceColor == PieceInfo.PIECECOLOR.EMPTY && piece.y-1 >= 0 && PiecesGrid.piecesGrid[piece.x, piece.y-1].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				if (PiecesGrid.piecesGrid[piece.x, piece.y-2].pieceType == PieceInfo.PIECETYPE.EMPTY)
				{
					piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x, piece.y-2]);
				}
			}

			//diagonals
			//white player
			if (playerColor == 1 && piece.y+1 < boardSize)
			{
				//upleft
				if (piece.x-1 >= 0 && PiecesGrid.piecesGrid[piece.x-1, piece.y+1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x-1, piece.y+1]);
				}

				//upright
				if (piece.x+1 < boardSize && PiecesGrid.piecesGrid[piece.x+1, piece.y+1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					//right //should be +1 x
					piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x+1, piece.y+1]);
				}
			}

			//Black Player
			if (playerColor == -1 && piece.y-1 >= 0)
			{
				//upleft
				if (piece.x-1 >= 0 && PiecesGrid.piecesGrid[piece.x-1, piece.y-1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					//should be x - 1 because it's left // this is the same for both black and white pawns
					piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x-1, piece.y-1]);
				}

				//upright
				if (piece.x+1 < boardSize && PiecesGrid.piecesGrid[piece.x+1, piece.y-1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x+1, piece.y-1]);
				}
			}
		}
	}



	void AIKnightMovement(int playerColor, PieceInfo piece)
	{
		if (piece.x+2 < boardSize && piece.y+1 < boardSize)
		{
			//if selected Knight's pieceColor == EMPTY OR == opposite of playerColor (I can do this by * -1.0f and then casting it into PIECECOLOR) 
			if (PiecesGrid.piecesGrid[piece.x+2, piece.y+1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.EMPTY ||  PiecesGrid.piecesGrid[piece.x+2, piece.y+1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x+2, piece.y+1]);
			}
		}

		if (piece.x-2 >= 0 && piece.y+1 < boardSize)
		{
			if (PiecesGrid.piecesGrid[piece.x-2, piece.y+1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.EMPTY || PiecesGrid.piecesGrid[piece.x-2, piece.y+1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x-2, piece.y+1]);
			}
		}

		if (piece.x+2 < boardSize && piece.y-1 >= 0)
		{
			if (PiecesGrid.piecesGrid[piece.x+2, piece.y-1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.EMPTY || PiecesGrid.piecesGrid[piece.x+2, piece.y-1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x+2, piece.y-1]);
			}
		}

		if (piece.x-2 >= 0 && piece.y-1 >= 0)
		{
			if (PiecesGrid.piecesGrid[piece.x-2, piece.y-1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.EMPTY || PiecesGrid.piecesGrid[piece.x-2, piece.y-1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x-2, piece.y-1]);
			}
		}

		if (piece.x+1 < boardSize && piece.y+2 < boardSize)
		{
			if (PiecesGrid.piecesGrid[piece.x+1, piece.y+2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x+1, piece.y+2]);
			}
		}

		if (piece.x+1 < boardSize && piece.y-2 >= 0)
		{
			if (PiecesGrid.piecesGrid[piece.x+1, piece.y-2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x+1, piece.y-2]);
			}
		}

		if (piece.x-1 >= 0 && piece.y+2 < boardSize)
		{
			if (PiecesGrid.piecesGrid[piece.x-1, piece.y+2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x-1, piece.y+2]);
			}
		}

		if (piece.x-1 >= 0 && piece.y-2 >= 0)
		{
			if (PiecesGrid.piecesGrid[piece.x-1, piece.y-2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[piece.x-1, piece.y-2]);
			}
		}
	}

	void AIBishopMovement(int playerColor, PieceInfo piece) // havent changed
	{
		PieceInfo tempPiece = null;

		int upLeftCount = 1;
		int upRightCount = 1;
		int downLeftCount = 1;
		int downRightCount = 1;

		if (upLeftCount == 1)	tempPiece = piece;

		//while the piece's upLeft Travel is not blocked (havent reach the end of map/havent reach piece of opposite color/ havent reach piece of the same color, continue searching
		while((piece.x - upLeftCount >= 0 && piece.y + upLeftCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.x - upLeftCount >= 0 && piece.y + upLeftCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x-1, tempPiece.y+1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x+1 <= piece.x && tempPiece.y-1 >= piece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 upLeftPiece = ReturnedCheckUPLEFT(piece, upLeftCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upLeftPiece.x, (int)upLeftPiece.y]);
			}

			upLeftCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x-upLeftCount+1, piece.y+upLeftCount-1];

			//if blocked, break the loop;
			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (downRightCount == 1)	tempPiece = piece;

		while((piece.x + downRightCount < boardSize && piece.y - downRightCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.x + downRightCount < boardSize && piece.y - downRightCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x+1, tempPiece.y-1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x-1 >= piece.x && tempPiece.y+1 <= piece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 downRightPiece = ReturnedCheckDOWNRIGHT(piece, downRightCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downRightPiece.x, (int)downRightPiece.y]);
			}

			downRightCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x+downRightCount-1, piece.y-downRightCount+1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (upRightCount == 1)	tempPiece = piece;

		while((piece.x + upRightCount < boardSize && piece.y + upRightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.x + upRightCount < boardSize && piece.y + upRightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x+1, tempPiece.y+1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x-1 >= piece.x && tempPiece.y-1 >= piece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 upRightPiece = ReturnedCheckUPRIGHT(piece, upRightCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upRightPiece.x, (int)upRightPiece.y]);
			}

			upRightCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x+upRightCount-1, piece.y+upRightCount-1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (downLeftCount == 1)		tempPiece = piece;

		while((piece.x - downLeftCount > -1 && piece.y - downLeftCount > -1 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.x - downLeftCount > -1 && piece.y - downLeftCount > -1 && PiecesGrid.piecesGrid[tempPiece.x-1, tempPiece.y-1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x+1 >= piece.x && tempPiece.y+1 <= piece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 downLeftPiece = ReturnedCheckDOWNLEFT(piece, downLeftCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downLeftPiece.x, (int)downLeftPiece.y]);
			}

			downLeftCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x-downLeftCount+1, piece.y-downLeftCount+1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}
	}

	void AIRookMovement(int playerColor, PieceInfo piece)
	{
		PieceInfo tempPiece = null;

		int upCount = 1;
		int downCount = 1;
		int leftCount = 1;
		int rightCount = 1;

		if (upCount == 1)
		{
			tempPiece = piece;
		}

		//GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.EMPTY || 
		while((piece.y + upCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.y + upCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y+1].pieceColor != playerColorTurn))
		{
			if (tempPiece.y-1 >= piece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 upPiece = ReturnedCheckUP(piece, upCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upPiece.x, (int)upPiece.y]);
			}

			upCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x, piece.y+upCount-1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (downCount == 1)
		{
			tempPiece = piece;
		}

		while((piece.y - downCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.y - downCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y-1].pieceColor != playerColorTurn))
		{
			if (tempPiece.y+1 >= piece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 downPiece = ReturnedCheckDOWN(piece, downCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downPiece.x, (int)downPiece.y]);
			}

			downCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x, piece.y-downCount+1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (leftCount == 1)
		{
			tempPiece = piece;
		}

		while((piece.x - leftCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.x - leftCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x-1, tempPiece.y].pieceColor != playerColorTurn))
		{
			if (tempPiece.x+1 >= piece.x || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 leftPiece = ReturnedCheckLEFT(piece, leftCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)leftPiece.x, (int)leftPiece.y]);
			}

			leftCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x - leftCount + 1, piece.y];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (rightCount == 1)
		{
			tempPiece = piece;
		}

		while((piece.x + rightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor)) && (piece.x + rightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x+1, tempPiece.y].pieceColor != playerColorTurn))
		{
			if (tempPiece.x-1 <= piece.x || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				Vector2 rightPiece = ReturnedCheckRIGHT(piece, rightCount);

				piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)rightPiece.x, (int)rightPiece.y]);
			}

			rightCount++;

			tempPiece = PiecesGrid.piecesGrid[piece.x + rightCount - 1, piece.y];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}
	}


	void AIQueenMovement(int playerColor, PieceInfo piece)
	{
		AIBishopMovement(playerColor, piece);
		AIRookMovement(playerColor, piece);
	}


	void AIKingMovement(PieceInfo piece, int count, int playerColor) //1
	{
		//up
		if (piece.y+count < boardSize && 
			PiecesGrid.piecesGrid[piece.x, piece.y+count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 upPiece = new Vector2();
			upPiece = ReturnedCheckUP(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upPiece.x, (int)upPiece.y]);
		}
		//down
		if (piece.y-count >= 0 && 
			PiecesGrid.piecesGrid[piece.x, piece.y-count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 downPiece = new Vector2();
			downPiece = ReturnedCheckDOWN(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downPiece.x, (int)downPiece.y]);
		}

		//left
		if (piece.x-count >= 0 && 
			PiecesGrid.piecesGrid[piece.x-count, piece.y].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 leftPiece = new Vector2();
			leftPiece = ReturnedCheckLEFT(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)leftPiece.x, (int)leftPiece.y]);
		}

		//right
		if (piece.x+count < boardSize && 
			PiecesGrid.piecesGrid[piece.x+count, piece.y].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 rightPiece = new Vector2();
			rightPiece = ReturnedCheckRIGHT(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)rightPiece.x, (int)rightPiece.y]);
		}

		//============================================ Diagonal Checking ===========================================================

		//upLeft
		if (piece.x-count >= 0 && piece.y+count < boardSize && 
			PiecesGrid.piecesGrid[piece.x-count, piece.y+count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 upLeftPiece = new Vector2();
			upLeftPiece = ReturnedCheckUPLEFT(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upLeftPiece.x, (int)upLeftPiece.y]);
		}

		//upRight
		if (piece.x+count < boardSize && piece.y+count < boardSize && 
			PiecesGrid.piecesGrid[piece.x+count, piece.y+count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 upRightPiece = new Vector2();
			upRightPiece = ReturnedCheckUPRIGHT(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)upRightPiece.x, (int)upRightPiece.y]);
		}

		//downLeft
		if (piece.x-count >= 0 && piece.y-count >= 0 && 
			PiecesGrid.piecesGrid[piece.x-count, piece.y-count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 downLeftPiece = new Vector2();
			downLeftPiece = ReturnedCheckDOWNLEFT(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downLeftPiece.x, (int)downLeftPiece.y]);
		}

		//downRight
		if (piece.x+count < boardSize && piece.y-count >= 0 && 
			PiecesGrid.piecesGrid[piece.x+count, piece.y-count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)playerColor)
		{
			Vector2 downRightPiece = new Vector2();
			downRightPiece = ReturnedCheckDOWNRIGHT(piece, count);

			piece.currentPossibleMovesList.Add(BoardGrid.boardTileGrid[(int)downRightPiece.x, (int)downRightPiece.y]);
		}
	}
	//==================================================================== end

//======================================================================== Returning Checking


	public Vector2 ReturnedCheckUP(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x, piece.y+count);

		return movedPiecePosition;
	}

	public Vector2 ReturnedCheckDOWN(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x, piece.y-count);

		return movedPiecePosition;
	}

	public Vector2 ReturnedCheckLEFT(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x-count, piece.y);

		return movedPiecePosition;
	}
		
	public Vector2 ReturnedCheckRIGHT(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x+count, piece.y);

		return movedPiecePosition;
	}

	public Vector2 ReturnedCheckUPLEFT(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x-count, piece.y+count);

		return movedPiecePosition;
	}

	public Vector2 ReturnedCheckUPRIGHT(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x+count, piece.y+count);

		return movedPiecePosition;
	}

	public Vector2 ReturnedCheckDOWNRIGHT(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x+count, piece.y-count);

		return movedPiecePosition;
	}

	public Vector2 ReturnedCheckDOWNLEFT(PieceInfo piece, int count)
	{
		Vector2 movedPiecePosition = new Vector2(piece.x-count, piece.y-count);

		return movedPiecePosition;
	}

	//====================================================================


//==================================================================== Player Moves
	void PawnMovement(int playerColor) //black is -1, white is 1
	{
		//Dont change the PieceColor.Black and Whites here. Must be fixed
		if (selectedPiece != null)
		{
			// Forward 1
			if (playerColor == 1 && selectedPiece.y+1 < boardSize && PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+1].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				CheckUP(1);
			}
			else if (playerColor == -1 && selectedPiece.y-1 >= 0 && PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y-1].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				CheckDOWN(1);
			}

			//Double Forward
			//if its y = 1, white  || if y = 6, black
			if (selectedPiece.y == 1 && selectedPiece.pieceColor == PieceInfo.PIECECOLOR.WHITE && selectedPiece.y+2 < boardSize && PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+playerColor*2].pieceColor == PieceInfo.PIECECOLOR.EMPTY && PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+playerColor].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				if (PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+2].pieceType == PieceInfo.PIECETYPE.EMPTY)
				{
					BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y+2].isTargeted = true;
					targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y+2]);
				}
			}
			else if (selectedPiece.y == 6 && selectedPiece.pieceColor == PieceInfo.PIECECOLOR.BLACK && selectedPiece.y-2 >= 0 && PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+playerColor*2].pieceColor == PieceInfo.PIECECOLOR.EMPTY && PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+playerColor].pieceColor == PieceInfo.PIECECOLOR.EMPTY)
			{
				if (PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y-2].pieceType == PieceInfo.PIECETYPE.EMPTY)
				{
					BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y-2].isTargeted = true;
					targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y-2]);
				}
			}

			//diagonals
			//white player
			if (playerColor == 1 && selectedPiece.y+1 < boardSize)
			{
				//upleft
				if (selectedPiece.x-1 >= 0 && PiecesGrid.piecesGrid[selectedPiece.x-1, selectedPiece.y+1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					//should be x - 1 because it's left // this is the same for both black and white pawns
					BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y+1].isTargeted = true;
					targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y+1]);
				}

				//upright
				if (selectedPiece.x+1 < boardSize && PiecesGrid.piecesGrid[selectedPiece.x+1, selectedPiece.y+1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.BLACK)
				{
					//right //should be +1 x
					BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y+1].isTargeted = true;
					targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y+1]);
				}
			}

			//Black Player
			if (playerColor == -1 && selectedPiece.y-1 >= 0)
			{
				//upleft
				if (selectedPiece.x-1 >= 0 && PiecesGrid.piecesGrid[selectedPiece.x-1, selectedPiece.y-1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					//should be x - 1 because it's left // this is the same for both black and white pawns
					BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y-1].isTargeted = true;
					targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y-1]);
				}

				//upright
				if (selectedPiece.x+1 < boardSize && PiecesGrid.piecesGrid[selectedPiece.x+1, selectedPiece.y-1].GetComponent<PieceInfo>().pieceColor == PieceInfo.PIECECOLOR.WHITE)
				{
					BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y-1].isTargeted = true;
					targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y-1]);
				}
			}
		}
	}

	void KnightMovement(int playerColor)
	{
		if (selectedPiece.x+2 < boardSize && selectedPiece.y+1 < boardSize)
		{
			//if selected Knight's pieceColor == EMPTY OR == opposite of playerColor (I can do this by * -1.0f and then casting it into PIECECOLOR) 
			if (PiecesGrid.piecesGrid[selectedPiece.x+2, selectedPiece.y+1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x+2, selectedPiece.y+1].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+2, selectedPiece.y+1]);
			}
		}

		if (selectedPiece.x-2 >= 0 && selectedPiece.y+1 < boardSize)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x-2, selectedPiece.y+1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x-2, selectedPiece.y+1].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-2, selectedPiece.y+1]);
			}
		}

		if (selectedPiece.x+2 < boardSize && selectedPiece.y-1 >= 0)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x+2, selectedPiece.y-1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x+2, selectedPiece.y-1].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+2, selectedPiece.y-1]);
			}
		}

		if (selectedPiece.x-2 >= 0 && selectedPiece.y-1 >= 0)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x-2, selectedPiece.y-1].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x-2, selectedPiece.y-1].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-2, selectedPiece.y-1]);
			}
		}

		if (selectedPiece.x+1 < boardSize && selectedPiece.y+2 < boardSize)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x+1, selectedPiece.y+2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y+2].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y+2]);
			}
		}

		if (selectedPiece.x+1 < boardSize && selectedPiece.y-2 >= 0)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x+1, selectedPiece.y-2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y-2].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+1, selectedPiece.y-2]);
			}
		}

		if (selectedPiece.x-1 >= 0 && selectedPiece.y+2 < boardSize)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x-1, selectedPiece.y+2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y+2].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y+2]);
			}
		}

		if (selectedPiece.x-1 >= 0 && selectedPiece.y-2 >= 0)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x-1, selectedPiece.y-2].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(playerColor))
			{
				BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y-2].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-1, selectedPiece.y-2]);
			}
		}
	}

	void BishopMovement(int playerColor) // havent changed
	{
		PieceInfo tempPiece = null;

		int upLeftCount = 1;
		int upRightCount = 1;
		int downLeftCount = 1;
		int downRightCount = 1;

		if (upLeftCount == 1)	tempPiece = selectedPiece;

		//while the selectedPiece's upLeft Travel is not blocked (havent reach the end of map/havent reach piece of opposite color/ havent reach piece of the same color, continue searching
		while((selectedPiece.x - upLeftCount >= 0 && selectedPiece.y + upLeftCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.x - upLeftCount >= 0 && selectedPiece.y + upLeftCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x-1, tempPiece.y+1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x+1 <= selectedPiece.x && tempPiece.y-1 >= selectedPiece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckUPLEFT(upLeftCount);
			}

			upLeftCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x-upLeftCount+1, selectedPiece.y+upLeftCount-1];

			//if blocked, break the loop;
			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (downRightCount == 1)	tempPiece = selectedPiece;

		while((selectedPiece.x + downRightCount < boardSize && selectedPiece.y - downRightCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.x + downRightCount < boardSize && selectedPiece.y - downRightCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x+1, tempPiece.y-1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x-1 >= selectedPiece.x && tempPiece.y+1 <= selectedPiece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckDOWNRIGHT(downRightCount);
			}

			downRightCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x+downRightCount-1, selectedPiece.y-downRightCount+1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (upRightCount == 1)	tempPiece = selectedPiece;

		while((selectedPiece.x + upRightCount < boardSize && selectedPiece.y + upRightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.x + upRightCount < boardSize && selectedPiece.y + upRightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x+1, tempPiece.y+1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x-1 >= selectedPiece.x && tempPiece.y-1 >= selectedPiece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckUPRIGHT(upRightCount);
			}

			upRightCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x+upRightCount-1, selectedPiece.y+upRightCount-1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (downLeftCount == 1)		tempPiece = selectedPiece;

		while((selectedPiece.x - downLeftCount > -1 && selectedPiece.y - downLeftCount > -1 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.x - downLeftCount > -1 && selectedPiece.y - downLeftCount > -1 && PiecesGrid.piecesGrid[tempPiece.x-1, tempPiece.y-1].pieceColor != playerColorTurn))
		{
			if (tempPiece.x+1 >= selectedPiece.x && tempPiece.y+1 <= selectedPiece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckDOWNLEFT(downLeftCount);
			}

			downLeftCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x-downLeftCount+1, selectedPiece.y-downLeftCount+1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}
	}

	void RookMovement(int playerColor)
	{
		PieceInfo tempPiece = null;

		int upCount = 1;
		int downCount = 1;
		int leftCount = 1;
		int rightCount = 1;

		if (upCount == 1)
		{
			tempPiece = selectedPiece;
		}

		while((selectedPiece.y + upCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.y + upCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y+1].pieceColor != playerColorTurn))
		{
			if (tempPiece.y-1 >= selectedPiece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckUP(upCount);
			}

			upCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+upCount-1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (downCount == 1)
		{
			tempPiece = selectedPiece;
		}

		while((selectedPiece.y - downCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.y - downCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y-1].pieceColor != playerColorTurn))
		{
			if (tempPiece.y+1 >= selectedPiece.y || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckDOWN(downCount);
			}

			downCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y-downCount+1];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (leftCount == 1)
		{
			tempPiece = selectedPiece;
		}

		while((selectedPiece.x - leftCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.x - leftCount >= 0 && PiecesGrid.piecesGrid[tempPiece.x-1, tempPiece.y].pieceColor != playerColorTurn))
		{
			if (tempPiece.x+1 >= selectedPiece.x || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckLEFT(leftCount);
			}

			leftCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x - leftCount + 1, selectedPiece.y];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}

		if (rightCount == 1)
		{
			tempPiece = selectedPiece;
		}

		while((selectedPiece.x + rightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor)) && (selectedPiece.x + rightCount < boardSize && PiecesGrid.piecesGrid[tempPiece.x+1, tempPiece.y].pieceColor != playerColorTurn))
		{
			if (tempPiece.x-1 <= selectedPiece.x || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor != playerColorTurn)
			{
				CheckRIGHT(rightCount);
			}

			rightCount++;

			tempPiece = PiecesGrid.piecesGrid[selectedPiece.x + rightCount - 1, selectedPiece.y];

			if (PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == (PieceInfo.PIECECOLOR)(-playerColor) || PiecesGrid.piecesGrid[tempPiece.x, tempPiece.y].pieceColor == playerColorTurn)
			{
				break;
			}
		}
	}

	void QueenMovement(int playerColor)
	{
		BishopMovement(playerColor);
		RookMovement(playerColor);
	}
		
	void KingMovement(int count) //1
	{
		CheckUP(count);
		CheckDOWN(count);
		CheckLEFT(count);
		CheckRIGHT(count);

		CheckUPLEFT(count);
		CheckUPRIGHT(count);
		CheckDOWNLEFT(count);
		CheckDOWNRIGHT(count);
	}

	void CheckUP(int count)
	{
		if (selectedPiece.y+count < boardSize)
		{
			// !White
			if (PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y+count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y+count].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y+count]);
			}
		}
	}

	void CheckDOWN(int count)
	{
		if (selectedPiece.y-count >= 0)
		{
			// !White
			if (PiecesGrid.piecesGrid[selectedPiece.x, selectedPiece.y-count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y-count].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x, selectedPiece.y-count]);
			}
		}
	}

	void CheckLEFT(int count)
	{
		if (selectedPiece.x-count >= 0)
		{
			// !White
			if (PiecesGrid.piecesGrid[selectedPiece.x-count, selectedPiece.y].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x-count, selectedPiece.y].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-count, selectedPiece.y]);
			}
		}
	}

		
	void CheckRIGHT(int count)
	{
		if (selectedPiece.x+count >= 0)
		{
			// !White
			if (PiecesGrid.piecesGrid[selectedPiece.x+count, selectedPiece.y].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x+count, selectedPiece.y].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+count, selectedPiece.y]);
			}
		}
	}

	//================================================== Diagonal Checking ============================================================

	void CheckUPLEFT(int count)
	{
		if (selectedPiece.x-count >= 0 && selectedPiece.y+count < boardSize)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x-count, selectedPiece.y+count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x-count, selectedPiece.y+count].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-count, selectedPiece.y+count]);
			}
		}
	}

	void CheckUPRIGHT(int count)
	{
		if (selectedPiece.x+count >= 0 && selectedPiece.y+count < boardSize)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x+count, selectedPiece.y+count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x+count, selectedPiece.y+count].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+count, selectedPiece.y+count]);
			}
		}
	}

	void CheckDOWNRIGHT(int count)
	{
		if (selectedPiece.x+count < boardSize && selectedPiece.y-count >= 0)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x+count, selectedPiece.y-count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x+count, selectedPiece.y-count].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x+count, selectedPiece.y-count]);
			}
		}
	}

	void CheckDOWNLEFT(int count)
	{
		if (selectedPiece.x-count >= 0 && selectedPiece.y-count >= 0)
		{
			if (PiecesGrid.piecesGrid[selectedPiece.x-count, selectedPiece.y-count].GetComponent<PieceInfo>().pieceColor != (PieceInfo.PIECECOLOR)(int)playerColorTurn)
			{
				BoardGrid.boardTileGrid[selectedPiece.x-count, selectedPiece.y-count].isTargeted = true;
				targetedTilesList.Add(BoardGrid.boardTileGrid[selectedPiece.x-count, selectedPiece.y-count]);
			}
		}
	}
}