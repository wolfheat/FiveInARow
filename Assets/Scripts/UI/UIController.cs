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
    private const int InfoLinesForRPC = 20;
    [SerializeField] GameLobbyUI gameLobbyUI;
    [SerializeField] GameObject myTurnIndicator;
    [SerializeField] GameObject lobbies;
    [SerializeField] LobbyUI lobbyUI;
    [SerializeField] InfoController infoController;
    [SerializeField] WaitingInfoController waitingInfoController;   
    [SerializeField] GameLobby gameLobby;
    [SerializeField] MessagePopup generalMessagePopup;
    [SerializeField] TextMeshProUGUI score;
    [SerializeField] TextMeshProUGUI serverClientText;
    [SerializeField] TextMeshProUGUI recieveData;
    [SerializeField] TextMeshProUGUI recieveDataB;
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

        InitializeCorrectMenus();

    }

    private void InitializeCorrectMenus()
    {
        lobbyUI.gameObject.SetActive(true);
        lobbies.gameObject.SetActive(true);
        infoController.gameObject.SetActive(true);
        gameLobbyUI.gameObject.SetActive(false);
        waitingInfoController.gameObject.SetActive(false);
    }

    public void JoinGameLobby(Lobby lobby)
    {
        ShowJoinedLobby();
        gameLobbyUI.UpdateLobby(lobby);
    }

    public void ShowJoinedLobby()
    {
        StateController.Instance.State = State.Lobby;
        lobbyUI.gameObject.SetActive(false);
        gameLobbyUI.gameObject.SetActive(true);
        gameLobbyUI.SetToJoinbuttonState();
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
    
    public void AddRPCInfoB(string info)
    {
        recieveDataB.text = info;
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

    public void UpdateScoreText(int[] s, string[] playerNames)
    {
        SetConnectionInfoB("Updating Score Text: ");
        score.text = playerNames[0] + " " + s[0] +" - " + s[1] +" " +playerNames[1];
        SetConnectionInfoB("Updating Score Text to: "+score.text);
    }
    
    public void SetInGameTextAsServer(bool isServer = true)
    {
        serverClientImage.color = isServer?Color.red:Color.blue;
        serverClientText.text = isServer ? "Server" : "Client";
    }

    public void BackToLobby()
    {
        Debug.Log("Requesting Back to lobby");
        // Disconnect from server and show lobby

        //GameRelay.Instance.Disconnect();

        ShowJoinedLobby();
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

    public void HidePopupInfoControllerMessage()
    {
        Debug.Log("Hide waiting info");
        waitingInfoController.gameObject.SetActive(false);
    }

    public void ShowPopupInfoControllerMessage(string info)
    {
        Debug.Log("Show waiting info: "+info);
        waitingInfoController.gameObject.SetActive(true);
        waitingInfoController.SetInfoText(info);
    }

    public void ShowWaitingForAllPlayers(bool show = true)
    {
        StateController.Instance.State = State.Paused;
        infoController.WaitingForPlayers(show);
    }

    internal void MyTurnIndicator(bool setActive)
    {
        myTurnIndicator.SetActive(setActive);
    }
}
