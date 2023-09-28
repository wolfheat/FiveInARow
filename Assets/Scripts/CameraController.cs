using System;
using System.Collections;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Camera cam;
    [SerializeField] TileMapController tileMapController;

    [Range(3,5)]
    [SerializeField] float zoom;
    private const float zoomMin=2;
    private const float zoomMax=4;
    private Vector2 startTouchPosition;
    private Vector3 startCameraPosition;

    private void Start()
    {
        zoom = cam.orthographicSize;
    }

    public void SetPosition(Vector3 camPos)
    {
        transform.position = camPos;
    }
    public void SetPositionLerped(Vector3 camPos)
    {
        StartCoroutine(LerpToPosition(camPos));
    }

    public void SetStartMovePosition(Vector2 startTouch)
    {
        startTouchPosition = startTouch;
        startCameraPosition = transform.position;
    }
    public void MoveCamera(Vector2 vector2)
    {        
        transform.position += (Vector3)vector2;

        // Clamp to game area
        ClampToGameArea();
    }

    private void ClampToGameArea()
    {
        float Xpos = Mathf.Clamp(transform.position.x,0,tileMapController.TileMap.size.x * tileMapController.CellSize);
        float Ypos = Mathf.Clamp(transform.position.y,0,tileMapController.TileMap.size.y * tileMapController.CellSize);

        transform.position = new Vector3(Xpos,Ypos,transform.position.z);
    }

    public void TouchMoveCamera(Vector2 vector2)
    {        
        Vector2 cameraChange = startTouchPosition - vector2;
        Vector2 cameraChangeScaled = cameraChange/180;

        Vector3 worldMove = cam.ScreenToWorldPoint(cameraChange);

        //Debug.Log("Touch pos " + vector2);
        //Debug.Log("Camera change: "+cameraChange);
        //Debug.Log("Moving Camera to "+worldMove);
        transform.position = startCameraPosition + (Vector3)cameraChangeScaled;

        ClampToGameArea();
        //Debug.Log("New Camera Position " + transform.position);
        //Debug.Log("-------------------------------------");
    }

    public void ZoomIn()
    {
        zoom = Mathf.Clamp(zoom-1,zoomMin,zoomMax);
        UpdateCamera();
    }

    public void ZoomOut()
    {
        zoom = Mathf.Clamp(zoom+1, zoomMin, zoomMax);
        UpdateCamera();
    }
    public void UpdateCamera()
    {
        cam.orthographicSize = zoom;
    }

    private IEnumerator LerpToPosition(Vector3 camPos, float time = 5f)
    {
        float lerpTimer = 0;
        Vector3 startPos = transform.position;
        while (Vector3.Distance(transform.position,camPos)>0.1f) 
        {
            lerpTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, camPos, lerpTimer/time);
            yield return null;
        }
        transform.position = camPos;
        ClampToGameArea();
    }

}
