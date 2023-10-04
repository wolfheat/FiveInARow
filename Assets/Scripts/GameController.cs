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

    public static GameController Instance { get; private set; }

    private void Start()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }


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
        UIController.Instance.ShowWaitingForAllPlayers();
    }

    // Inputs
    private void SpaceInput(InputAction.CallbackContext context)
    {
        // Reset
        ResetGame();
    }

    public void ResetGame()
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

    public bool PlaceMarkerByPlayerIndex(Vector2Int pos, int playerIndex)
    {
        bool gamecomple = tileMapController.ChangeTileAtIndex(pos,playerIndex);
        if (gamecomple) ShowWinLine();
        return gamecomple;
    }
    
    private void ClickedTile()
    {
        if (StateController.Instance.State != State.Playing) return;

        // Client clicks handle this
        Vector2Int posIndex = tileMapController.GetClickedIndex();
        bool validSpot = tileMapController.IsPlacementValid(posIndex);

        if (!validSpot)
        {
            Debug.Log("This is not a valid placement");
            return;
        }
        // Client accepts this placement send it to server
        NetworkCommunicator.Instance.SendPlacementServerRpc(posIndex);
    }

    // Actions
    public void ShowWinner(int winner)
    {
        string winnerName = winner == 0 ? "X" : "O";

        // Info Panel
        infoController.ShowPanel();
        infoController.SetWinner(winnerName);

        //State
        StateController.Instance.State = State.Paused;
    }

    public void ShowWinLine()
    {        
        // Win Line
        winLine.gameObject.SetActive(true);
        Vector3[] linePositions = tileMapController.TileMapLineIndexesAsWorldPositions();
        winLine.SetPositions(linePositions);
    }
    

    public Vector2Int GetRandomPosition()
    {
        int sizeX = tileMapController.TileMap.size.x;
        int sizeY = tileMapController.TileMap.size.y;
        Vector2Int pos = new Vector2Int(Random.Range(2, sizeX-2), Random.Range(2, sizeY-2));
        Debug.Log("Creating Random position: "+pos);
        return pos;
    }

    public bool ValidatePlacement(Vector2Int pos, int playerID)
    {
        return tileMapController.IsPlacementValid(pos);
    }
}
