using Unity.Services.Lobbies.Models;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] GameLobbyUI gameLobbyUI;
    [SerializeField] LobbyUI lobbyUI;
    [SerializeField] GameLobby gameLobby;

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
        gameLobbyUI.SetLobby(lobby);
        gameLobbyUI.UpdatePlayers();
    }

    public void ShowMainLobby()
    {
        gameLobbyUI.gameObject.SetActive(false);

        lobbyUI.gameObject.SetActive(true);
        gameLobby.UpdateLobby();
    }
}
