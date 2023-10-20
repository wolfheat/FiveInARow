using UnityEngine;

public enum State { Paused, Playing, Lobby}

public class StateController : MonoBehaviour
{
    public static StateController Instance;

    public State State { get; set; } = State.Lobby;

    private void Start()
    {
        if(Instance != null) Destroy(Instance.gameObject);
        Instance = this;    
    }

}
