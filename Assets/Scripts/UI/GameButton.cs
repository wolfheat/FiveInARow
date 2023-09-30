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

    public void SetLobby(Lobby lob)
    {
        Lobby = lob;
    }
    
    public void RequestJoinGame()
    {
        FindObjectOfType<LobbyUI>().JoinGame(Lobby.LobbyCode);
    }
}
