using UnityEngine;

public class Inputs : MonoBehaviour
{
    public static Controls Controls { get; set;} 

    private void OnEnable()
    {
        Controls = new Controls();
        Controls.Enable();
    }
}
