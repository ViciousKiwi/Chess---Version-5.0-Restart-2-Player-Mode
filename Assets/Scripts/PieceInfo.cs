using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceInfo : MonoBehaviour 
{
	public enum PIECETYPE
	{
		EMPTY = 0,
		PAWN,

		KNIGHT,
		BISHOP,
		ROOK,

		QUEEN,
		KING,

		TOTAL
	}

	public enum PIECECOLOR
	{
		BLACK = -1,
		EMPTY = 0,
		WHITE = 1,

		TOTAL
	}

	public PIECETYPE pieceType;
	public PIECECOLOR pieceColor;

	public int x, y;

	public float pieceValue;

	public SpriteRenderer pieceSpriteRenderer;

	public List<BoardInfo> currentPossibleMovesList = new List<BoardInfo>();

	void Awake()
	{
		pieceSpriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	}
}
