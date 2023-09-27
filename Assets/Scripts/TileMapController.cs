using System;
using System.Text;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class TileMapController : MonoBehaviour
{
    [SerializeField] Tilemap tilemap;
    [SerializeField] Tile[] tiles;
    Tile darkTile;
    Tile vacant;
    Tile blue;
    Tile X;
    Tile O;

    private const float CellSize = 0.32f;
    private const int StartSize = 15;
    private const int ExpandSize = 3;
    private const int WinCondition = 5;


    private void OnEnable()
    {
        Inputs.Controls.Main.Click.started += ClickedTile;  
        Inputs.Controls.Main.RClick.started += RClickedTile;  
        Inputs.Controls.Main.Space.started += SpaceInput;  
    }

    private void SpaceInput(InputAction.CallbackContext context)
    {
        ResetGame();
        FindObjectOfType<InfoController>().ShowPanel(false);
    }
    private void ResetGame()
    {
        Debug.Log("Reset");
        ResetTileMap();
        CreateStartGrid();
        CenterGrid();
        StateController.Instance.State = State.Playing;
    }
    private void ClickedTile(InputAction.CallbackContext context)
    {
        if(StateController.Instance.State == State.Playing)
            RequestChangeTile(X);
    }
    private void RClickedTile(InputAction.CallbackContext context)
    {
        if (StateController.Instance.State  == State.Playing)
            RequestChangeTile(O);
    }

    private void RequestChangeTile(Tile ChangeToType)
    {
        // Get Mouse Position
        Vector2 mousePosition = Mouse.current.position.value;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePosition);

        // Get Clicked Index
        Vector3Int tilePos = tilemap.WorldToCell(worldPos);
        Vector3Int clickedIndex = new Vector3Int(tilePos.x, tilePos.y,1);   


        if (IndexIsOutOfBounds(clickedIndex)) return;

        // Check if tile is allready used
        TileBase clickedTile = tilemap.GetTile(clickedIndex);
        if (TileIsUsed(clickedTile))return;

        // Change the tile
        tilemap.SetTile(clickedIndex,ChangeToType);

        // Expand game area
        ExpandIfOutside(clickedIndex);

        // Check for game complete
        if (CheckGameComplete())
        {
            Debug.Log("Someone Won");
            FindObjectOfType<InfoController>().ShowPanel();

            FindObjectOfType<InfoController>().SetWinner(countedTile.name);
            StateController.Instance.State = State.Paused;
        }
    }

    int counter = 0;
    Tile countedTile = null;

    private bool CheckGameComplete()
    {
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
            for (int i = k, j = tilemap.size.y-1; i < tilemap.size.x && j >= 0; i++, j--)
                if (CountTiles(i, j)) return true;
            for (int i = k, j = 0; i >= 0 && j < tilemap.size.y; i--, j++)
                if (CountTiles(i, j)) return true;
            for (int i = k, j = tilemap.size.y-1; i >= 0 && j >=0; i--, j--)
                if (CountTiles(i, j)) return true;
        }
        return false;
    }

    private bool CountTiles(int i,int j)
    {
        Tile currentTile = tilemap.GetTile<Tile>(new Vector3Int(i, j, 1));
        if (currentTile != vacant)
        {
            if (currentTile != countedTile)
            {
                counter = 0;
                countedTile = currentTile;
            }
            counter++;
            if (counter == WinCondition) return true;
        }
        else counter = 0;
        return false;
    }

    private void ExpandIfOutside(Vector3Int clickedIndex)
    {

        if(clickedIndex.x < ExpandSize)
        {
            int addColumns = ExpandSize - clickedIndex.x;
            
            // Move Right
            ShiftBlock(new Vector2Int(addColumns,0));
            //ShiftColumns(addColumns);

            // AddColumn
            AddTileBlock(new Vector2Int(0, 0), new Vector2Int(addColumns, tilemap.size.y));
        }
        else if(clickedIndex.x >= tilemap.size.x - ExpandSize - 1)
        {
            int addColumns = clickedIndex.x + ExpandSize - tilemap.size.x + 1;
            
            // AddColumn
            AddTileBlock(new Vector2Int(tilemap.size.x, 0), new Vector2Int(addColumns, tilemap.size.y));
        }

        if(clickedIndex.y < ExpandSize)
        {
            int addRows = ExpandSize - clickedIndex.y;

            // Move Right
            ShiftBlock(new Vector2Int(0, addRows));
            //ShiftRows(addRows);

            // AddRow
            AddTileBlock(new Vector2Int(0,0), new Vector2Int(tilemap.size.x,addRows));
        }
        else if(clickedIndex.y >= tilemap.size.y - ExpandSize - 1)
        {
            int addRows = clickedIndex.y + ExpandSize - tilemap.size.y + 1;

            // AddRow
            AddTileBlock(new Vector2Int(0,tilemap.size.y), new Vector2Int(tilemap.size.x, addRows));
        }
        CenterGrid();
    }

    private void ShiftBlock(Vector2Int amount)
    {
        for (int i = tilemap.size.x-1; i >= 0; i--)
        {
            for (int j = tilemap.size.y - 1; j >= 0; j--) 
            {
                tilemap.SetTile(new Vector3Int(i + amount.x, j + amount.y, 1), tilemap.GetTile(new Vector3Int(i, j, 1)));
            }
        }
    }
    private void AddTileBlock(Vector2Int startPos, Vector2Int amount)
    {
        for (int i = 0; i < amount.x; i++)
        {
            for (int j = 0; j < amount.y; j++)
            {
                tilemap.SetTile(new Vector3Int(startPos.x+i, startPos.y+j, 1), vacant);
            }
        }
    }
    
    private bool TileIsUsed(TileBase tile)
    {
        return tile != vacant;
    }

    private bool IndexIsOutOfBounds(Vector3Int clickedIndex)
    {
        if(clickedIndex.x < 0 || clickedIndex.x >= tilemap.size.x)
        {

            Debug.Log("Clicked Outside X wise: size: "+ tilemap.size.x);
            return true;
        }
        else if(clickedIndex.y <0 || clickedIndex.y >= tilemap.size.y)
        {
            Debug.Log("Clicked Outside Y wise: size: "+ tilemap.size.x);
            return true;
        }
        return false;
    }

    void Start()
    {
        vacant = tiles[0];
        X = tiles[1];
        O = tiles[2];
        darkTile = tiles[3];
        blue = tiles[4];

        ResetGame();

        tilemap.SetTile(new Vector3Int(5, 1, 1), darkTile);    
        tilemap.SetTile(new Vector3Int(0, 0, 1), darkTile);    
    }

    private void ResetTileMap()
    {
        tilemap.ClearAllTiles();
    }

    private void CenterGrid()
    {
        float width = tilemap.size.x * CellSize;
        float height = tilemap.size.y * CellSize;
        //Set Camera
        Vector3 camPos = new Vector3(width / 2, height / 2, Camera.main.transform.position.z);
        Camera.main.transform.position = camPos;
        //Debug.Log("Setting Camera to position: "+camPos);

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
}
