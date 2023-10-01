using System;
using System.Collections;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobbyUI : MonoBehaviour
{
    [SerializeField] PlayerButton playerButtonPrefab;
    [SerializeField] GameObject playerButtonHolder;
    [SerializeField] GameObject startButton;
    [SerializeField] GameLobby gameLobby;
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] Blinker blinker;

    private string activeLobbyCode = string.Empty;
    private Lobby activeLobby;

    private PlayerButton[] playerButtons;
    private const int AmountOfPlayers = 2;

    private void Init()
    {
        CreatePlayerButtons();
        startButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameLobby.Polling += UpdateLobby;
    }
    
    private void OnDisable()
    {
        GameLobby.Polling -= UpdateLobby;
    }

    private void PollingUpdate(Lobby obj)
    {

        Debug.Log("Polling recieved, update players and text");
        UpdatePlayers();
    }

    private void TickLobby()
    {
        throw new NotImplementedException();
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
        if(playerButtons ==null) Init();

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
            playerButtons[i].SetText(activeLobby.Players[i].Data["PlayerName"].Value);
            //Debug.Log("Updating playername to: "+ activeLobby.Players[i].Data["PlayerName"].Value);
        }
    }

    public void UpdateLobby(Lobby lobby)
    {
        activeLobby = lobby;
        UpdateLobbyText();
        UpdatePlayers();

        blinker.StartBlink();
    }

    private void UpdateLobbyText()
    {
        lobbyNameText.text = activeLobby.Name;
        lobbyCodeText.text = "Join Code: "+activeLobby.LobbyCode;
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
