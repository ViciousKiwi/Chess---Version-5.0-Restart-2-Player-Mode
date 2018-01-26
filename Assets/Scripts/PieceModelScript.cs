using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PIECESPRITEINDEX
{
	WHITE_PAWN = 0,
	WHITE_KNIGHT,
	WHITE_BISHOP,
	WHITE_ROOK,
	WHITE_QUEEN,
	WHITE_KING,

	BLACK_PAWN,
	BLACK_KNIGHT,
	BLACK_BISHOP,
	BLACK_ROOK,
	BLACK_QUEEN,
	BLACK_KING,

	TOTAL
}

public class PieceModelScript : MonoBehaviour 
{
	public List<Sprite> spriteList = new List<Sprite>();
}
