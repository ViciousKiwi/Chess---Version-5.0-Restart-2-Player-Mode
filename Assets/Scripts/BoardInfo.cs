using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardInfo : MonoBehaviour 
{
	public enum NEIGHBOURINDEX
	{
		NORTH = 0,
		NORTHEAST,
		EAST,
		SOUTHEAST,
		SOUTH,
		SOUTHWEST,
		WEST,
		NORTHWEST,

		TOTAL
	}

	public enum STARTCOLOR
	{
		BLACK,
		WHITE,

		TOTAL
	}

	public STARTCOLOR startColor;
	public bool isTargeted;

	public int x, y;

	//public List <BoardInfo> neighbourList = new List<BoardInfo>();  

}
