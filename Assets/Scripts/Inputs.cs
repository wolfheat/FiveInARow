using UnityEngine;

public class Inputs : MonoBehaviour
{
    public static Controls Controls { get; set;} 

    private void Awake()
    {
        Controls = new Controls();
        Controls.Enable();
    }
}
