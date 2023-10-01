using System;
using TMPro;
using UnityEngine;

public class JoinPrivateGameController : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] GameLobby gameLobby;

    public string PlayerName { get; private set; }

    public void JoinPrivateGameCheck()
    {
        if (!ValidateNameLength()) return;

        string lobbyID = inputField.text;

        //Try join game
        Debug.Log("Trying to join Lobby with ID: "+lobbyID);
        gameLobby.JoinLobbyByLobbyCodeAsync(lobbyID);
    }
    
    public void RequestBackToLobby()
    {
        gameObject.SetActive(false);
    }

    public void Reset()
    {
        inputField.text = string.Empty;
    }

    private bool ValidateNameLength()
    {
        Debug.Log("Checking Length of string: "+inputField.text.Length);
        UIController.Instance.ShowPopup("Code needs 6 digits!");
        return (inputField.text.Length == 6);
    }
}
