using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobbyUI : MonoBehaviour
{
    [SerializeField] PlayerButton playerButtonPrefab;
    [SerializeField] GameObject playerButtonHolder;
    [SerializeField] GameObject startButton;
    [SerializeField] GameLobby gameLobby;

    private string activeLobbyCode = string.Empty;
    private Lobby activeLobby;

    private PlayerButton[] playerButtons;
    private const int AmountOfPlayers = 2;

    private void Start()
    {
        CreatePlayerButtons();
        startButton.gameObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private void CreatePlayerButtons()
    {
        playerButtons = new PlayerButton[AmountOfPlayers];
        for (int i = 0; i < AmountOfPlayers; i++)
        {
            PlayerButton playerButton= Instantiate(playerButtonPrefab, playerButtonHolder.transform);
            playerButtons[i] = playerButton;
            playerButton.gameObject.SetActive(false);
        }
        Debug.Log("Created All PlayerButton, all are hidden amount: "+AmountOfPlayers);
    }

    public void UpdatePlayers()
    {
        Debug.Log("Updating Players in Game Lobby.");
        Debug.Log("lobbyCode: "+activeLobbyCode);
        Debug.Log("Amount of players: "+ activeLobby?.Players.Count);
        
        if (activeLobby == null) return;

        int actualPlayersAmount = activeLobby.Players.Count;

        for (int i = 0; i < AmountOfPlayers; i++)
        {
            if (i >= actualPlayersAmount)
            {
                playerButtons[i].gameObject.SetActive(false);
                continue;
            }
            // Update Button
            playerButtons[i].gameObject.SetActive(true);
            playerButtons[i].SetText(activeLobby.Players[i].Data["playerName"].Value);
            //playerButtons[i].SetText(activeLobby.Players[i].playerName);
        }
    }

    public void SetLobby(Lobby lobby)
    {
        activeLobby = lobby;
        activeLobbyCode = lobby.LobbyCode;
        startButton.gameObject.SetActive(true);
    }
    
    public void RequestStartGame()
    {
        Debug.Log("Start Game Pressed");
    }
    
    public void RequestReturnToLobby()
    {
        Debug.Log("Return to Lobby Pressed");
        gameLobby.LeaveLobbyAsync();
    }
}
