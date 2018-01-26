using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BOARDHORIZONTAL
{
	A = 0,
	B,
	C,
	D,
	E,
	F,
	G,
	H,

	TOTAL
}

public class PieceSpawner : MonoBehaviour 
{
	public GameObject piecesPrefab;
	public Transform piecesTiles;
	ClickControl clickController;
	public GameObject pieceModel;

	public static int horCount = 8;
	public static int verCount = 8;

	public PieceInfo[,] piecesGrid = new PieceInfo[horCount, verCount]; 

	void Start()
	{
		InitPieces();
		PiecesSprites();
	}

	void Update()
	{
		//PiecesSprites();
	}

	public void ResetPieces()
	{
		for(int j=0; j<verCount; j++)
		{
			for(int i=0; i<horCount; i++)
			{
				piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.EMPTY;
				piecesGrid[i, j].pieceColor = PieceInfo.PIECECOLOR.EMPTY;

				//Pawns
				if (j == 6 || j == 1)
				{
					piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.PAWN;
					piecesGrid[i, j].pieceValue = 1;
				}

				if (j == 7 || j == 0)
				{
					//Rooks
					if (i == (int)BOARDHORIZONTAL.A || i == (int)BOARDHORIZONTAL.H)
					{
						piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.ROOK;
						piecesGrid[i, j].pieceValue = 5;
					}

					//Knights
					else if (i == (int)BOARDHORIZONTAL.B || i == (int)BOARDHORIZONTAL.G)
					{
						piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.KNIGHT;
						piecesGrid[i, j].pieceValue = 3;
					}

					//Bishops
					else if (i == (int)BOARDHORIZONTAL.C || i == (int)BOARDHORIZONTAL.F)
					{
						piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.BISHOP;
						piecesGrid[i, j].pieceValue = 3;
					}

					//Queens
					else if (i == (int)BOARDHORIZONTAL.D)
					{
						piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.QUEEN;
						piecesGrid[i, j].pieceValue = 9;
					}

					//Kings
					else if (i == (int)BOARDHORIZONTAL.E)
					{
						piecesGrid[i, j].pieceType = PieceInfo.PIECETYPE.KING;
						piecesGrid[i, j].pieceValue = 1000;
					}
				}

				if (j <= 1)
				{
					piecesGrid[i, j].pieceColor = PieceInfo.PIECECOLOR.WHITE;
				}

				if (j >= 6)
				{
					piecesGrid[i, j].pieceColor = PieceInfo.PIECECOLOR.BLACK;
				}
			}
		}
	}

	public void InitPieces()
	{
		for(int j=0; j<verCount; j++)
		{
			for(int i=0; i<horCount; i++)
			{
				PieceInfo go = Instantiate(piecesPrefab, new Vector3(i, // horCount / 2.0f * gridSize is offset
					j, 0.0f), Quaternion.identity, piecesTiles).GetComponent<PieceInfo>(); 

				//Default pieceType
				go.pieceType = PieceInfo.PIECETYPE.EMPTY;

				go.x = i;
				go.y = j;

				//Pawns
				if (j == 6 || j == 1)
				{
					go.pieceType = PieceInfo.PIECETYPE.PAWN;
					go.pieceValue = 1;
				}


				if (j == 7 || j == 0)
				{
					//Rooks
					if (i == (int)BOARDHORIZONTAL.A || i == (int)BOARDHORIZONTAL.H)
					{
						go.pieceType = PieceInfo.PIECETYPE.ROOK;
						go.pieceValue = 5;
					}

					//Knights
					else if (i == (int)BOARDHORIZONTAL.B || i == (int)BOARDHORIZONTAL.G)
					{
						go.pieceType = PieceInfo.PIECETYPE.KNIGHT;
						go.pieceValue = 3;
					}

					//Bishops
					else if (i == (int)BOARDHORIZONTAL.C || i == (int)BOARDHORIZONTAL.F)
					{
						go.pieceType = PieceInfo.PIECETYPE.BISHOP;
						go.pieceValue = 3;
					}

					//Queens
					else if (i == (int)BOARDHORIZONTAL.D)
					{
						go.pieceType = PieceInfo.PIECETYPE.QUEEN;
						go.pieceValue = 9;
					}

					//Kings
					else if (i == (int)BOARDHORIZONTAL.E)
					{
						go.pieceType = PieceInfo.PIECETYPE.KING;
						go.pieceValue = 1000;
					}
				}


				if (j <= 1)
				{
					go.pieceColor = PieceInfo.PIECECOLOR.WHITE;
				}

				if (j >= 6)
				{
					go.pieceColor = PieceInfo.PIECECOLOR.BLACK;
				}

				piecesGrid[i,j] = go;
			}
		}
	}

	public void PiecesSprites()
	{
		for (int i = 0; i < verCount; i++)
		{
			for (int j = 0; j < horCount; j++)
			{
				if (piecesGrid[i,j] == null) return;

				if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.PAWN)
				{
					if (piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_PAWN];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_PAWN];
					}
					else
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite= pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_PAWN];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_PAWN];
					}
				}
				else if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.KNIGHT)
				{
					if (piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_KNIGHT];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_KNIGHT];
					}
					else
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_KNIGHT];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_KNIGHT];
					}
				}
				else if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.BISHOP)
				{
					if (piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_BISHOP];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_BISHOP];
					}
					else
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_BISHOP];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_BISHOP];
					}
				}
				else if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.ROOK)
				{
					if (piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_ROOK];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_ROOK];
					}
					else
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_ROOK];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_ROOK];
					}
				}
				else if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.QUEEN)
				{
					if (piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_QUEEN];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_QUEEN];
					}
					else
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_QUEEN];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_QUEEN];
					}
				}
				else if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.KING)
				{
					if (piecesGrid[i,j].pieceColor == PieceInfo.PIECECOLOR.BLACK)
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_KING];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.BLACK_KING];
					}
					else
					{
						piecesGrid[i,j].pieceSpriteRenderer.sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_KING];
						//piecesGrid[i,j].GetComponent<SpriteRenderer>().sprite = pieceModel.GetComponent<PieceModelScript>().spriteList[(int)PIECESPRITEINDEX.WHITE_KING];
					}
				}
				else if (piecesGrid[i,j].pieceType == PieceInfo.PIECETYPE.EMPTY)
				{
					piecesGrid[i,j].pieceSpriteRenderer.sprite = null;
				}
			}
		}
	}
}
