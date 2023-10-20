using System;
using System.Collections;
using TMPro;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.VisualScripting;
using UnityEngine;

public class GameLobbyUI : MonoBehaviour
{
    [SerializeField] PlayerButton playerButtonPrefab;
    [SerializeField] GameObject playerButtonHolder;
    [SerializeField] GameObject startButton;
    [SerializeField] GameObject joinButton;
    [SerializeField] GameLobby gameLobby;
    [SerializeField] TextMeshProUGUI lobbyCodeText;
    [SerializeField] TextMeshProUGUI lobbyNameText;
    [SerializeField] Blinker blinker;

    private string activeLobbyCode = string.Empty;
    private Lobby activeLobby;

    private PlayerButton[] playerButtons;
    private const int AmountOfPlayers = 2;

    public static GameLobbyUI Instance { get; private set; }

    private void Start()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    

    private void Init()
    {
        CreatePlayerButtons();
        startButton.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        HideStartJoinByDefault();
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
        
    }

    private bool IsLobbyHost()
    {
        return true;
    }

    public void UpdatePlayers()
    {
        if(playerButtons == null) Init();

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

        ShowStartButtonIfHostAndFull();
    }

    private void ShowStartButtonIfHostAndFull()
    {
        if (JoinedLobbyIsFull() && gameLobby.IsHost)
            startButton.gameObject.SetActive(true);
        else 
            startButton.gameObject.SetActive(false);
    }
    
    public void HideStartJoinByDefault()
    {        
        startButton.gameObject.SetActive(false);
        joinButton.gameObject.SetActive(false);
    }

    private bool JoinedLobbyIsFull()
    {
        return AmountOfPlayers == gameLobby.JoinedLobby?.Players?.Count;
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
        gameLobby.StartGameAsync();
    }

    public void RequestReturnToLobby()
    {
        Debug.Log("Return to Lobby Pressed");
        gameLobby.LeaveLobbyAsync();
    }

    public void RequestJoinGame()
    {
        Debug.Log("Client request to join game");
        gameLobby.JoinRelay();
    }
    public void ShowJoinButton(bool show = true)
    {
        joinButton.gameObject.SetActive(show);
    }

    public void SetToJoinbuttonState()
    {
        Debug.Log("Setting Lobby to join button state");

    }
}
