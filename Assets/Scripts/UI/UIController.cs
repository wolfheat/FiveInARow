using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameLobbyUI gameLobbyUI;
    [SerializeField] LobbyUI lobbyUI;
    [SerializeField] GameLobby gameLobby;
    [SerializeField] MessagePopup generalMessagePopup;

    public static UIController Instance { get; private set; }
    
    void Start()
    {
        if (Instance != null)
        {
            Destroy(Instance.gameObject);
        }
        Instance = this;
    }

    public void JoinGameLobby(Lobby lobby)
    {
        // Only creator sets this lobbycode how to handle this

        lobbyUI.gameObject.SetActive(false);
        gameLobbyUI.gameObject.SetActive(true);
        gameLobbyUI.UpdateLobby(lobby);
    }

    public void ShowMainLobby()
    {
        gameLobbyUI.gameObject.SetActive(false);
        lobbyUI.gameObject.SetActive(true);
    }

    public void ShowPopup(string message)
    {
        generalMessagePopup.ShowMessage(message);
    }
    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif

    }
}
