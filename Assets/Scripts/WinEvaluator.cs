using UnityEngine;
using UnityEngine.Tilemaps;

public class WinEvaluator
{
    Tilemap tilemap;
    
    // Win Line variables
    int counter = 0;
    public Tile CountedTile { get; private set; }
    public Vector2Int[] Line { get; private set; } = new Vector2Int[2];

    Tile vacant = null;

    private const int WinCondition = 5;

    public bool CheckGameComplete(Tilemap t, Tile v)
    {
        tilemap = t;
        vacant = v;
        //Horizontal
        for (int j = 0; j < tilemap.size.y; j++)
            for (int i = 0; i < tilemap.size.x; i++)
                if (CountTiles(i, j)) return true;

        for (int k = 0; k < tilemap.size.x; k++)
        {
            //Vertical
            for (int j = 0; j < tilemap.size.y; j++)
                if (CountTiles(k, j)) return true;

            //Diagonals 
            for (int i = k, j = 0; i < tilemap.size.x && j < tilemap.size.y; i++, j++)
                if (CountTiles(i, j)) return true;
            for (int i = k, j = tilemap.size.y - 1; i < tilemap.size.x && j >= 0; i++, j--)
                if (CountTiles(i, j)) return true;
            for (int i = k, j = 0; i >= 0 && j < tilemap.size.y; i--, j++)
                if (CountTiles(i, j)) return true;
            for (int i = k, j = tilemap.size.y - 1; i >= 0 && j >= 0; i--, j--)
                if (CountTiles(i, j)) return true;
        }
        return false;
    }

    private bool CountTiles(int i, int j)
    {
        Tile currentTile = tilemap.GetTile<Tile>(new Vector3Int(i, j, 1));

        if (currentTile != vacant)
        {
            if (currentTile != CountedTile)
            {
                Line[0] = new Vector2Int(i, j);
                counter = 0;
                CountedTile = currentTile;
            }
            counter++;
            if (counter == WinCondition)
            {
                Line[1] = new Vector2Int(i, j);
                return true;
            }
        }
        else
        {
            counter = 0;
            CountedTile = null;
        }
        return false;
    }

 }
