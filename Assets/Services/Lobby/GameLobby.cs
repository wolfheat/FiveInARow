using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    [SerializeField] bool UseUpdatePolling = true;
    public Lobby JoinedLobby { get; private set; }
    public bool IsHost { get; private set; }
    private Lobby hostLobby;
    private float heartBeatTimer = 0;
    private float pollingTimer = 0;
    private const float HeartBeatTime = 15f;
    private const float PollingTime = 1.5f;
    private string playerName = "RandomName";
    private string RELAY_CODE = "RELAY_CODE";
    public static Action<Lobby> Polling;
    public static Action GameStarted;
    public static Action<List<Lobby>> PollingGameList;

    private async void Start()
    {
        await UnityServices.InitializeAsync();


        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
        };

        //Signs in and creates new Account
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleHeartbeat();
        HandleUpdatePolling();
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
    private void HandleUpdatePolling()
    {
        if (!UseUpdatePolling || StateController.Instance.State != State.Lobby) return;

        pollingTimer += Time.deltaTime;
        //Debug.Log("Polling timer: "+pollingTimer);
        if(pollingTimer >= PollingTime)
        {
            pollingTimer -= PollingTime;
            if(JoinedLobby != null)
            {
                DoUpdatePolling();
                return;
            }
            UpdateLobbiesListAsync();
        }
    }

    private async void DoUpdatePolling()
    {
        Lobby lobby = await LobbyService.Instance.GetLobbyAsync(JoinedLobby.Id);
        JoinedLobby = lobby;
        Polling?.Invoke(lobby);

        string relayCode = JoinedLobby.Data[RELAY_CODE].Value;

        // Check if game is set to start
        if (relayCode != "0")
        {
            // Game has a relay code and this user is not Host (Host joins automatically)
            NetworkCommunicator.PlayerIndex = 0;
            if (!IsLobbyHost())
            {
                GameRelay.Instance.JoinRelayAsync(relayCode);
                UIController.Instance.SetAsServer(false);
                NetworkCommunicator.PlayerIndex = 1;
            }
            else
                UIController.Instance.SetAsServer();

            //JoinedLobby = null;

            // Leave Lobby? Keep Lobby?

            GameStarted?.Invoke();
        }
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

    public async void QuickJoinLobby()
    {
        try
        {
            QuickJoinLobbyOptions quickJoinLobbyOptions = new QuickJoinLobbyOptions{
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync(quickJoinLobbyOptions);
            JoinedLobby = lobby;
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
            // If there is no free lobby to join create a new one
            CreateLobbyAsync();
        }
    }

    public async void JoinLobbyByLobbyIdAsync(string id)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(id, joinLobbyByIdOptions);
            JoinedLobby = lobby;
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            //if(e.Message.)
            UIController.Instance.ShowPopup("Invalid Code!");
            Debug.Log(e);
        }

    }
    public async void JoinLobbyByLobbyCodeAsync(string lobbyCode)
    {
        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            JoinedLobby = lobby;
            UIController.Instance.JoinGameLobby(JoinedLobby);
        }
        catch (LobbyServiceException e)
        {
            //if(e.Message.)
            UIController.Instance.ShowPopup("Invalid Code!");
            Debug.Log(e);
        }
    }

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
