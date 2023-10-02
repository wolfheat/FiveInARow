using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{
    [SerializeField] TileMapController tileMapController;
    [SerializeField] InfoController infoController;
    [SerializeField] CameraController cameraController;
    [SerializeField] LineRendererController winLine;

    private void OnEnable()
    {
        Inputs.Controls.Main.Scroll.started += Scroll;
        Inputs.Controls.Main.Space.started += SpaceInput;
        SwipeController.Clicked += ClickedTile;
        GameLobby.GameStarted += GameStarted;
    }
    
    private void OnDisable()
    {
        Inputs.Controls.Main.Scroll.started -= Scroll;
        Inputs.Controls.Main.Space.started -= SpaceInput;
        SwipeController.Clicked -= ClickedTile;
        GameLobby.GameStarted -= GameStarted;
    }

    private void GameStarted()
    {
        Debug.Log("Game started");
        UIController.Instance.HideAllLobbies();
        ResetGame();
    }

    private void Start()
    {
        // Dont start Game BY default
    }

    // Inputs
    private void SpaceInput(InputAction.CallbackContext context)
    {
        // Reset
        ResetGame();
    }

    private void ResetGame()
    {
        tileMapController.ResetGame();
        infoController.ShowPanel(false);
        winLine.gameObject.SetActive(false);
        cameraController.SetPosition(tileMapController.GetCenterPosition());

        // State
        StateController.Instance.State = State.Playing;
    }

    private void Scroll(InputAction.CallbackContext context)
    {
        bool scrollingUp = context.ReadValue<Vector2>().y > 0;
        if (scrollingUp)
            cameraController.ZoomOut();
        else
            cameraController.ZoomIn();
    }

    public void PlaceMarkerRandom(int playerIndex)
    {
        Vector3Int pos = new Vector3Int(Random.Range(0,10), Random.Range(0, 10),1);
        Debug.Log("Place random marker at "+pos+" playerindex: "+playerIndex);
        tileMapController.ChangeTileAtIndex(pos,playerIndex==0?Tiletype.X:Tiletype.O);
    }
    
    public void PlaceMarkerByTiletype(Vector3Int pos, Tiletype tileType = Tiletype.X)
    {

        Debug.Log("Place marker at "+pos+" "+tileType);
        tileMapController.ChangeTileAtIndex(pos,tileType);
    }
    public void PlaceMarkerByPlayerIndex(Vector3Int pos, int playerIndex)
    {
        Tiletype tileType = playerIndex == 0?Tiletype.X:Tiletype.O;
        Debug.Log("Place marker at "+pos+" "+tileType);
        tileMapController.ChangeTileAtIndex(pos,tileType);
    }
    
    private void ClickedTile()
    {
        if (StateController.Instance.State != State.Playing) return;

        Tiletype tiletype = Inputs.Controls.Main.Shift.IsPressed()?Tiletype.O:Tiletype.X;
        tileMapController.RequestChangeTile(tiletype);
            
    }

    // Actions
    public void HandleWin(string winner)
    {        
        // Win Line
        winLine.gameObject.SetActive(true);
        Vector3[] linePositions = tileMapController.TileMapLineIndexesAsWorldPositions();
        winLine.SetPositions(linePositions);

        // Info Panel
        infoController.ShowPanel();
        infoController.SetWinner(winner);

        //State
        StateController.Instance.State = State.Paused;

    }

    public string GetRandomPositionString()
    {
        Vector3Int pos = new Vector3Int(Random.Range(0, 10), Random.Range(0, 10), 1);
        string posAsString = ""+pos.x+pos.y;
        Debug.Log("Vector3Int as string: "+posAsString);
        return posAsString;
    }
}
