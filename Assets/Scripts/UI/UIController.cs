using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private const int InfoLinesForRPC = 8;
    [SerializeField] GameLobbyUI gameLobbyUI;
    [SerializeField] LobbyUI lobbyUI;
    [SerializeField] InfoController infoController;
    [SerializeField] GameLobby gameLobby;
    [SerializeField] MessagePopup generalMessagePopup;
    [SerializeField] TextMeshProUGUI serverClientText;
    [SerializeField] TextMeshProUGUI recieveData;
    [SerializeField] TextMeshProUGUI connectedPlayers;
    [SerializeField] Image serverClientImage;
    [SerializeField] TextMeshProUGUI connectionInfo;
    [SerializeField] TextMeshProUGUI connectionInfoB;

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
    
    public void HideAllLobbies()
    {
        gameLobbyUI.gameObject.SetActive(false);
        lobbyUI.gameObject.SetActive(false);
    }
    
    public void SetConnectionInfoB(string info)
    {
        connectionInfoB.text = "Info: "+info;
    }
    
    public void SetConnectionInfo(string info)
    {
        connectionInfo.text = "Info: "+info;
    }
    private int recieveID = 0;

    private Queue<string> recieveInfoArray = new Queue<string>(InfoLinesForRPC);

    public void AddRPCInfo(string info)
    {
        recieveID++;
        string newInfo = recieveID + ": " + info+"\n";
        if(recieveInfoArray.Count == InfoLinesForRPC) recieveInfoArray.Dequeue();
        recieveInfoArray.Enqueue(newInfo);
        SetRecieveDataFromArray();
    }

    public void SetConnectedPlayers(string c)
    {
        connectedPlayers.text = c;
    }
    
    private void SetRecieveDataFromArray()
    {
        StringBuilder finalRecieveDataString = new StringBuilder();
        foreach (var item in recieveInfoArray)
        {
            finalRecieveDataString.Append(item.ToString());
        }
        recieveData.text = finalRecieveDataString.ToString();
    }

    public void SetAsServer(bool isServer = true)
    {
        serverClientImage.color = isServer?Color.red:Color.blue;
        serverClientText.text = isServer ? "Server" : "Client";
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

    public void ShowWaitingForAllPlayers(bool show = true)
    {
        infoController.WaitingForPlayers(show);
    }
}
