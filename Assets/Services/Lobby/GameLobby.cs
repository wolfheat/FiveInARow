using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    [SerializeField] bool UseUpdatePolling = true;
    public Lobby JoinedLobby { get; private set; }
    public string RelayCode { get; private set; }

    public bool IsHost { get; private set; }

    private Lobby hostLobby;

    private const string RELAY_CODE = "RELAY_CODE";
    private const float HeartBeatTime = 15f;
    private const float PollingTime = 1.5f;
    private string playerName = "RandomName";
    private float heartBeatTimer = 0;
    private float pollingTimer = 0;

    public static Action<Lobby> Polling;
    public static Action GameStarted;
    public static Action<List<Lobby>> PollingGameList;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        // Check if signed in?

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
        };

        //Signs in and creates new Account
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        // Heart beat - helps the host keep the gamelobby from timing out
        HandleHeartbeat();

        // Polling - updates information for each player about the joined lobby OR available lobbies
        HandlePolling();
    }

    private void HandleHeartbeat()
    {
        if (hostLobby == null || StateController.Instance.State != State.Lobby) return;
        heartBeatTimer += Time.deltaTime;
        if(heartBeatTimer >= HeartBeatTime)
        {
            DoHeartBeat();
            heartBeatTimer -= HeartBeatTime;
        }
    }

    private void HandlePolling()
    {
        if (!UseUpdatePolling || StateController.Instance.State != State.Lobby) return;
        pollingTimer += Time.deltaTime;
        if(pollingTimer >= PollingTime)
        {
            pollingTimer -= PollingTime;
            // Player has joined a gamelobby
            if(JoinedLobby != null)
            {
                JoinedLobbyPolling();
                return;
            }
            // Player is in Main Lobby
            UpdateLobbiesListAsync();
        }
    }

    private async void JoinedLobbyPolling()
    {
        JoinedLobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
        
        Polling?.Invoke(JoinedLobby);

        RelayCode = JoinedLobby.Data[RELAY_CODE].Value;

        // Check if game is set to start
        if (RelayCode != "0")
        {
            Debug.Log("Relay code is set: "+RelayCode+" this is host: "+IsHost);
            if (!IsHost)
                ShowJoinButton();
            else
                JoinSetup(true);
        }
        else
            ShowJoinButton(false); // Currently this is set all the time (inefficient)
    }

    private void JoinSetup(bool isHost)
    {
        // Host Joins directly when joincode is set
        UIController.Instance.SetInGameTextAsServer(isHost);
        GameStarted?.Invoke();
        StateController.Instance.State = State.Paused;
        NetworkCommunicator.PlayerIndex = isHost?0:1;
    }

    public void JoinRelay()
    {
        // Only Clients Join by JoinCode
        if (RelayCode == "")
        {
            Debug.Log("Trying to join relay but it has no valid joincode");
            return;
        }
        JoinSetup(false);
        GameRelay.Instance.JoinRelayAsync(RelayCode);
    }
    
    private void ShowJoinButton(bool show = true)
    {
        GameLobbyUI.Instance.ShowJoinButton(show);
    }

    private async void DoHeartBeat()
    {
        Debug.Log("Doing Heart beat");
        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    }


    private async void UpdateLobbiesListAsync()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            PollingGameList?.Invoke(queryResponse.Results);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // JOINING
    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions{Player = GetPlayer()};
            JoinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            // If there is no free lobby to join create a new one
            CreateLobbyAsync();
        }
    }

    public async void JoinLobbyByLobbyCodeAsync(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions{Player = GetPlayer()};
            JoinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            UIController.Instance.ShowPopup("Invalid Code!");
            Debug.Log(e);
        }
    }
    
    public async void JoinLobbyByIdAsync(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions { Player = GetPlayer()};
            JoinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, joinLobbyByIdOptions);
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            UIController.Instance.ShowPopup("Invalid Code!");
            Debug.Log(e);
        }
    }

    // CREATING
    public async void CreatePrivateLobbyAsync()
    {
        try
        {
            string lobbyName = "Private Game " + UnityEngine.Random.Range(0, 1000);
            int maxPlayers = 2;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created private Lobby "+lobbyName+", with max players: "+maxPlayers+ " JoinCode: "+lobby.LobbyCode);

            hostLobby = lobby;
            JoinedLobby = lobby;
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    public async void CreateLobbyAsync()
    {
        try
        {
            string lobbyName = "Game "+ UnityEngine.Random.Range(0,1000);
            int maxPlayers = 2;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject> { 
                    {RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member,"0")} 
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers,createLobbyOptions);

            Debug.Log("Created Lobby "+lobbyName+", with max players: "+maxPlayers+" LobbyCode: "+ lobby.LobbyCode);
            // Auto Join This Game If you are the creator
            hostLobby = lobby;
            JoinedLobby = lobby; 
            IsHost = true;
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            UIController.Instance.ShowPopup("Could not create game.");
            Debug.Log(e);
        }
    }
    
    public void SetPlayerName(string name)
    {
        Debug.Log("Setting Player Name in gamelobby to: "+name);    
        playerName = name;
    }

    // Only Activate this if you want player to be able to set name when inside a lobby
    private async void UpdatePlayerNameAsync()
    {
        if (JoinedLobby == null) return;

        try
        {
            await LobbyService.Instance.UpdatePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId, new UpdatePlayerOptions
            {
                Data = new Dictionary<string, PlayerDataObject> {
                {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
            }
            });
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobbyAsync()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(JoinedLobby.Id, AuthenticationService.Instance.PlayerId);
            JoinedLobby = null;
            hostLobby = null;
            Debug.Log("Player left game lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Player was not in that lobby");
            Debug.Log(e);
        }
        finally
        {
            UIController.Instance.ShowMainLobby();
        }
    }

    private Player GetPlayer()
    {
        Debug.Log("Getting player name and it is: "+playerName);
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>{
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    }

    public async void StartGameAsync()
    {
        if (!IsLobbyHost()) return;
        try
        {
            Debug.Log("Host Starts Game");

            // Create Relay
            string relayCode = await GameRelay.Instance.CreateRelayAsync();

            Lobby lobby = await Lobbies.Instance.UpdateLobbyAsync(JoinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>() {
                    { RELAY_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayCode)} 
                }
            });
        }
        catch (LobbyServiceException e){
            Debug.Log(e);
        }
    }

    private bool IsLobbyHost()
    {
        Debug.Log("Checking if player is Host, first player name is: "+ JoinedLobby.Players[0].Data["PlayerName"].Value+" This player: "+playerName+" : "+(JoinedLobby.Players[0].Data["PlayerName"].Value == playerName));
        Debug.Log("Amount of players in Lobby: "+ JoinedLobby.Players.Count);
        return JoinedLobby.Players[0].Data["PlayerName"].Value == playerName;
    }
}
