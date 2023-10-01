using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEditor.Search;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    [SerializeField] bool UseUpdatePolling = true;
    public Lobby JoinedLobby { get; private set; }
    private Lobby hostLobby;
    private float heartBeatTimer = 0;
    private float pollingTimer = 0;
    private const float HeartBeatTime = 15f;
    private const float PollingTime = 1.5f;
    private string playerName = "RandomName";
    public static Action<Lobby> Polling;
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
        //Debug.Log("Handle Heart beat: Hostlobby: "+hostLobby+" heartbeattimer: "+heartBeatTimer);
        if (hostLobby == null) return;
        heartBeatTimer += Time.deltaTime;
        if(heartBeatTimer>= HeartBeatTime)
        {
            DoHeartBeat();
            heartBeatTimer -= HeartBeatTime;
        }
    }
    private void HandleUpdatePolling()
    {
        if (!UseUpdatePolling) return;

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
                Player = GetPlayer()
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

}
