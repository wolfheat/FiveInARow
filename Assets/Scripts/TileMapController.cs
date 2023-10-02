using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using TileMapExtensions;
using System;

public enum Tiletype{X,O}
public class TileMapController : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tile[] tiles;
    [SerializeField] LineRendererController winLine;
    [SerializeField] GameController gameController;
    Tile darkTile,vacant,X,O,blue;

    public Tilemap TileMap { get {return tilemap; }}

    WinEvaluator winEvaluator = new WinEvaluator();

    //Constants
    public float CellSize = 0.32f;
    private const int StartSize = 15;
    private const int ExpandSize = 3;
    private Vector3 centerOffsetVector;

    public static Action PositionChange;
    
    private void OnEnable()
    {
        centerOffsetVector = new Vector3 (CellSize/2, CellSize/2, 0);
    }

    void Start()
    {
        //Define tile types
        vacant = tiles[0];        X = tiles[1];        O = tiles[2];        darkTile = tiles[3];        blue = tiles[4];
        ResetGame();
    }

    public void ResetGame()
    {
        ResetTileMap();
        CreateStartGrid();
        PositionChange?.Invoke();
    }

    
    // Actions
    public void RequestChangeTile(Tiletype t)
    {
        Tile changeToType = TileTypeToTile(t);
        // Get Mouse Position
        //Vector2 mousePosition = Mouse.current.position.value;
        Vector2 mousePosition = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);

        // Get Clicked Index
        Vector3Int tilePos = tilemap.WorldToCell(worldPos);
        Vector3Int clickedIndex = new Vector3Int(tilePos.x, tilePos.y,1);

        if (IndexIsOutOfBounds(clickedIndex))
        {
            //Debug.Log("Index OOB: "+ clickedIndex);
            return;
        }

        ChangeTileAtIndex(clickedIndex, changeToType);

    }

    private Tile TileTypeToTile(Tiletype t)
    {
        return t == Tiletype.X ? X : O;
    }

    public void ChangeTileAtIndex(Vector3Int clickedIndex, Tiletype tileType)
    {
        ChangeTileAtIndex(clickedIndex, TileTypeToTile(tileType));
    }

    public void ChangeTileAtIndex(Vector3Int clickedIndex, Tile changeToType)
    {
        // Check if tile is occupied
        TileBase clickedTile = tilemap.GetTile(clickedIndex);

        if (TileIsOccupied(clickedTile))
        {
            //Debug.Log("Tile Occupied");

            return;
        }

        // Change the tile if free
        tilemap.SetTile(clickedIndex, changeToType);

        // Expand game area
        ExpandTileMapIfNewMarkerIsToCloseToBorder(clickedIndex);

        // Check for game complete
        if (winEvaluator.CheckGameComplete(tilemap, vacant))
            gameController.HandleWin(winEvaluator.CountedTile.name);
    }

    public Vector3[] TileMapLineIndexesAsWorldPositions()
    {
        return new Vector3[] { tilemap.CellToWorld((Vector3Int)winEvaluator.Line[0]) + centerOffsetVector, tilemap.CellToWorld((Vector3Int)winEvaluator.Line[1]) + centerOffsetVector };
    }
        
    private void ExpandTileMapIfNewMarkerIsToCloseToBorder(Vector3Int clickedIndex)
    {
        if(clickedIndex.x < ExpandSize)
        {
            int columnsToAdd = ExpandSize - clickedIndex.x;
            
            // Move Right
            tilemap = tilemap.ShiftTileMap(new Vector2Int(columnsToAdd, 0));

            // AddColumn
            tilemap = tilemap.AddTileMapBlock(new Vector2Int(0, 0), new Vector2Int(columnsToAdd, tilemap.size.y), vacant);

            // Move Camera accordingly
            cameraController.MoveCamera(new Vector2(CellSize*columnsToAdd,0));
        }
        else if(clickedIndex.x >= tilemap.size.x - ExpandSize - 1)
        {
            int columnsToAdd = clickedIndex.x + ExpandSize - tilemap.size.x + 1;
            
            // AddColumn
            tilemap = tilemap.AddTileMapBlock(new Vector2Int(tilemap.size.x, 0), new Vector2Int(columnsToAdd, tilemap.size.y), vacant);
        }

        if(clickedIndex.y < ExpandSize)
        {
            int rowsToAdd = ExpandSize - clickedIndex.y;

            // Move Right
            tilemap = tilemap.ShiftTileMap(new Vector2Int(0, rowsToAdd));

            // AddRow
            tilemap = tilemap.AddTileMapBlock(new Vector2Int(0, 0),new Vector2Int(tilemap.size.x, rowsToAdd),vacant);

            // Move Camera accordingly
            cameraController.MoveCamera(new Vector2(0,CellSize * rowsToAdd));
        }
        else if(clickedIndex.y >= tilemap.size.y - ExpandSize - 1)
        {
            int rowsToAdd = clickedIndex.y + ExpandSize - tilemap.size.y + 1;

            // AddRow
            tilemap = tilemap.AddTileMapBlock(new Vector2Int(0, tilemap.size.y), new Vector2Int(tilemap.size.x, rowsToAdd), vacant);
        }
    }

    private bool TileIsOccupied(TileBase tile)
    {
        return tile != vacant;
    }

    private bool IndexIsOutOfBounds(Vector3Int clickedIndex)
    {
        if(clickedIndex.x < 0 || clickedIndex.x >= tilemap.size.x)
            return true;
        else if(clickedIndex.y <0 || clickedIndex.y >= tilemap.size.y)
            return true;
        return false;
    }

    private void ResetTileMap()
    {
        tilemap.ClearAllTiles();
    }

    private void CreateStartGrid()
    {
        for (int i = 0; i < StartSize; i++)
        {
            for (int j = 0; j < StartSize; j++)
            {
                tilemap.SetTile(new Vector3Int(i, j, 1), vacant);    
            }
        }
    }

    public Vector3 GetCenterPosition()
    {
        float width = tilemap.size.x * CellSize;
        float height = tilemap.size.y * CellSize;
        return new Vector3(width / 2, height / 2, Camera.main.transform.position.z);
    }
}

