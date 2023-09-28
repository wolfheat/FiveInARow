using UnityEngine;

public class LineRendererController : MonoBehaviour
{
    [SerializeField] LineRenderer winLine;

    void Start()
    {
        DefineLineRenderer();    
    }

    private void DefineLineRenderer()
    {
        // Show Line
        winLine.startColor = Color.red;
        winLine.endColor = Color.red;
        winLine.startWidth = 0.05f;
        winLine.endWidth = 0.05f;
    }   

    public void SetPositions(Vector3[] linePositions)
    {
        winLine.SetPositions(linePositions);
    }
}
