using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI gameText;

    public Lobby Lobby { get; private set; }

    public void SetText(string lobbyName)
    {
        gameText.text = lobbyName;
    }

    public void SetLobby(Lobby lobby)
    {
        Lobby = lobby;
        SetText(lobby.Name);
        Debug.Log("Setting LobbyId: "+Lobby.Id);
    }
    
    public void RequestJoinGame()
    {
        Debug.Log("Trying to join Game with Id: " + Lobby.Id);
        FindObjectOfType<LobbyUI>().JoinGameId(Lobby.Id);
    }
}
