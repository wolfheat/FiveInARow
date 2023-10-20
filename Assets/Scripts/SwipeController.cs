using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class SwipeController : MonoBehaviour
{
    [SerializeField] private CameraController cameraController;
    private Vector2 startTouch;
    private Vector2 endTouch;
    private Vector2 lastTouchPoint;

    private const float MoveDistaneLimit = 30f;
    private float distance = 0f;
    public bool isMouse = false;

    private Coroutine moveCoroutine;

    public static Action<Vector2> Clicked;

    private void OnEnable()
    {
        Inputs.Controls.Touch.TouchPress.started += StartTouch;
        Inputs.Controls.Touch.TouchPress.canceled += EndTouch;

        Inputs.Controls.Mouse.Click.started += StartMouse;
        Inputs.Controls.Mouse.Click.canceled += EndMouse;
    }
    private void OnDisable()
    {
        Inputs.Controls.Touch.TouchPress.started -= StartTouch;
        Inputs.Controls.Touch.TouchPress.canceled -= EndTouch;

        Inputs.Controls.Mouse.Click.started -= StartMouse;
        Inputs.Controls.Mouse.Click.canceled -= EndMouse;
    }   

    private void ExecuteTouch()
    {
        if (startTouch == null || endTouch == null) return;
        
        // If moved screen do not execute click
        if (distance > MoveDistaneLimit) return;

        // Click
        Clicked.Invoke(GetPointerPosition());
    }

    private void EndTouch(InputAction.CallbackContext context)
    {
        Debug.Log("Touch ended at pos: " + (Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>()));
        if (StateController.Instance.State == State.Lobby) return;

        endTouch = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
        //Debug.Log("End Touch at: " + endTouch);
        if(moveCoroutine!=null)StopCoroutine(moveCoroutine);
        ExecuteTouch();
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        Debug.Log("Touch started at pos: " + (Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>()));
        isMouse = false;
        if (StateController.Instance.State == State.Lobby) return;

        startTouch = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
        if (startTouch == Vector2.zero) return;

        cameraController.SetStartMovePosition(startTouch);
        moveCoroutine = StartCoroutine(MoveScreen());
        //Debug.Log("Start Touch at: " + startTouch);
    }
    
    private void EndMouse(InputAction.CallbackContext context)
    {
        Debug.Log("Mouse drag ended at pos: " + (Mouse.current.position.value));
        UIController.Instance.AddRPCInfo("End Mouse Movement.");
        if (StateController.Instance.State == State.Lobby) return;

        endTouch = Mouse.current.position.value;
        //Debug.Log("End Touch at: " + endTouch);
        if(moveCoroutine!=null)StopCoroutine(moveCoroutine);
        ExecuteTouch();
    }

    private void StartMouse(InputAction.CallbackContext context)
    {
        Debug.Log("Mouse drag started at pos: "+(Mouse.current.position.value));
        UIController.Instance.AddRPCInfo("Start Mouse Movement.");
        isMouse = true;
        if (StateController.Instance.State == State.Lobby) return;

        startTouch = Mouse.current.position.value;
        if (startTouch == Vector2.zero) return;

        cameraController.SetStartMovePosition(startTouch);
        moveCoroutine = StartCoroutine(MoveScreen());
        //Debug.Log("Start Touch at: " + startTouch);
    }

    private Vector2 GetPointerPosition()
    {
        return isMouse ? Mouse.current.position.value : Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
    }

    private IEnumerator MoveScreen()
    {
        distance = 0f;
        lastTouchPoint = GetPointerPosition();
        while (true)
        {            
            Vector2 currentTouchPoint = GetPointerPosition();
            distance += Vector2.Distance(currentTouchPoint,lastTouchPoint);
            cameraController.TouchMoveCamera(currentTouchPoint);
            lastTouchPoint = currentTouchPoint;
            yield return new WaitForFixedUpdate();
        }
    }
}
