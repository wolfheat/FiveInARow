using System;
using UnityEngine;
using UnityEngine.InputSystem;

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

}
