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

    private Coroutine moveCoroutine;

    public static Action Clicked;

    private void OnEnable()
    {
        Inputs.Controls.Touch.TouchPress.started += StartTouch;
        Inputs.Controls.Touch.TouchPress.canceled += EndTouch;
    }
    private void OnDisable()
    {
        Inputs.Controls.Touch.TouchPress.started -= StartTouch;
        Inputs.Controls.Touch.TouchPress.canceled -= EndTouch;
    }   

    private void ExecuteTouch()
    {
        if (startTouch == null || endTouch == null) return;
        
        // If moved screen do not execute click
        if (distance > MoveDistaneLimit) return;

        // Click
        Clicked.Invoke();
    }

    private void EndTouch(InputAction.CallbackContext context)
    {
        if (StateController.Instance.State == State.Lobby) return;

        endTouch = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
        //Debug.Log("End Touch at: " + endTouch);
        if(moveCoroutine!=null)StopCoroutine(moveCoroutine);
        ExecuteTouch();
    }

    private void StartTouch(InputAction.CallbackContext context)
    {
        if (StateController.Instance.State == State.Lobby) return;

        startTouch = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
        if (startTouch == Vector2.zero) return;

        cameraController.SetStartMovePosition(startTouch);
        moveCoroutine = StartCoroutine(MoveScreen());
        //Debug.Log("Start Touch at: " + startTouch);
    }

    private IEnumerator MoveScreen()
    {
        distance = 0f;
        lastTouchPoint = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
        while (true)
        {            
            Vector2 currentTouchPoint = Inputs.Controls.Touch.TouchPosition.ReadValue<Vector2>();
            distance += Vector2.Distance(currentTouchPoint,lastTouchPoint);
            cameraController.TouchMoveCamera(currentTouchPoint);
            lastTouchPoint = currentTouchPoint;
            yield return new WaitForFixedUpdate();
        }
    }
}
