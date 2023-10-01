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

        string lobbyCode = inputField.text;
        //Try join game
        Debug.Log("Trying to join Lobby with ID: "+lobbyCode);
        gameLobby.JoinLobbyByLobbyCodeAsync(lobbyCode);
        gameObject.SetActive(false);
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
        if(inputField.text.Length != 6)
            UIController.Instance.ShowPopup("Code needs 6 digits!");
        return (inputField.text.Length == 6);
    }
}
