using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridSpawner : MonoBehaviour 
{
	public GameObject boardTilePrefab;
	public Transform boardTiles;
	ClickControl clickController;

	public static int horCount = 8;
	public static int verCount = 8;

	public BoardInfo[,] boardTileGrid = new BoardInfo[horCount, verCount]; 

	PieceInfo currentTile;

	// Use this for initialization
	void Start () 
	{
		InitGrid();
		//GetNeighbours();
	}
	
	// Update is called once per frame
	void Update ()
	{
		TileColor();

		if (Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	public void InitGrid()
	{
		for(int j=0; j<verCount; j++)
		{
			for(int i=0; i<horCount; i++)
			{
				BoardInfo go = Instantiate(boardTilePrefab, new Vector3(i, // horCount / 2.0f * gridSize is offset
					j, 1.0f), Quaternion.identity, boardTiles).GetComponent<BoardInfo>(); // z = 1.0f so it goes behind

				go.x = i;
				go.y = j;

				if (j%2 == 0 && i%2 == 0 || j%2 == 1 && i%2 == 1)
				{
					go.startColor = BoardInfo.STARTCOLOR.BLACK;
				}
				else
				{
					go.startColor = BoardInfo.STARTCOLOR.WHITE;
				}

				go.isTargeted = false;

				boardTileGrid[i,j] = go;
			}
		}
	}

	/*
	void GetNeighbours()
	{
		for(int j=0; j<verCount; j++)
		{
			for(int i=0; i<horCount; i++)
			{
				//north
				if (j+1 <verCount)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i,j+1]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//northeast
				if( i+1 < horCount && j+1 < verCount)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i+1,j+1]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//east
				if (i+1 < verCount)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i+1,j]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//southeast
				if (i+1 < verCount && j-1 >= 0)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i+1,j-1]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//south
				if (j-1 >= 0)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i,j-1]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//southwest
				if (i-1 >= 0 && j-1 >= 0)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i-1,j-1]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//west
				if (i-1 >= 0)
					boardTileGrid[i,j].neighbourList.Add(boardTileGrid[i-1,j]);
				else boardTileGrid[i,j].neighbourList.Add(null);

				//northwest
				if (i-1 >= 0 && j+1 < verCount)
					boardTileGrid[i,j].neighbourList.Insert(7, boardTileGrid[i-1,j+1]);
				else boardTileGrid[i,j].neighbourList.Add(null);
			}
		}
	}
	*/

	void TileColor()
	{
		for ( int i = 0; i < verCount; i++)
		{
			for ( int j = 0; j < horCount; j++)
			{
				if (boardTileGrid[i,j] == null) return;

				if (boardTileGrid[i,j].GetComponent<BoardInfo>().isTargeted == true)
				{
					boardTileGrid[i,j].GetComponent<SpriteRenderer>().color = Color.green;
				}
				else 
				{
					if (boardTileGrid[i,j].GetComponent<BoardInfo>().startColor == BoardInfo.STARTCOLOR.BLACK)
					{
						boardTileGrid[i,j].GetComponent<SpriteRenderer>().color = new Vector4(209f/255f, 139f/255f, 71f/255f, 1f);
					}
					else if (boardTileGrid[i,j].GetComponent<BoardInfo>().startColor == BoardInfo.STARTCOLOR.WHITE)
					{
						boardTileGrid[i,j].GetComponent<SpriteRenderer>().color = new Vector4(255f/255f, 206f/255f, 158f/255f, 1f);
					}
				}
			}
		}
	}
}