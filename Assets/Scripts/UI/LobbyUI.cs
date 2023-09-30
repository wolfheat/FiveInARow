using System;
using System.Collections.Generic;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] GameLobby gameLobby;
    [SerializeField] GameButton lobbyButtonPrefab;
    [SerializeField] GameObject gameHolder;
    [SerializeField] ChangeNameController changeNameController;
    [SerializeField] TextMeshProUGUI playerName;

    private GameButton[] gameButtons;

    private const int AmountOfGameButtons = 10;
    public static LobbyUI Instance { get; private set; }

    private void Start()
    {
        if (Instance != null) Destroy(Instance.gameObject);
        Instance = this;

        CreateAllGameButtons();
    }

    public void CreateGame(bool isPrivate)
    {
        Debug.Log("Request Create Game private = "+isPrivate);
        gameLobby.CreateLobbyAsync();
    }

    public void JoinAnyGame()
    {
        Debug.Log("Request Join Any Game");
        gameLobby.QuickJoinLobby();
    }
    
    public void JoinPrivateGame()
    {
        Debug.Log("Request Join Private Game");
        // Show Screen for putting in ID
        //gameLobby.JoinPrivateLobbyAsync();
    }

    public void JoinGame(String LobbyCode)
    {
        Debug.Log("Request Join Game: "+ LobbyCode);
        gameLobby.JoinLobbyByLobbyCodeAsync(LobbyCode);
    }
    
    public void ChangeNameClicked()
    {
        changeNameController.gameObject.SetActive(true);
    }
    
    public void UpdatePlayerName(string newName)
    {
        playerName.text = newName;
        gameLobby.SetPlayerName(newName);
    }


    private void CreateAllGameButtons()
    {
        gameButtons = new GameButton[AmountOfGameButtons];
        for (int i = 0; i< AmountOfGameButtons; i++)
        {
            GameButton gameButton = Instantiate(lobbyButtonPrefab, gameHolder.transform);
            gameButtons[i] = gameButton;    
            gameButton.gameObject.SetActive(false);
        }
        Debug.Log("Created All GameButtons, all are hidden");
    }

    public void UpdateLobbyButtons(List<Lobby> lobbies)
    {
        Debug.Log("Updating Lobby UI with lobbies: "+lobbies.Count);

        for (int i = 0; i < AmountOfGameButtons; i++)
        {
            if (i >= lobbies.Count)
            {
                gameButtons[i].gameObject.SetActive(false);
                continue;
            }
            // Update Button
            gameButtons[i].gameObject.SetActive(true);
            gameButtons[i].SetText(lobbies[i].Name);
            gameButtons[i].SetLobby(lobbies[i]);
        }
    }

}
