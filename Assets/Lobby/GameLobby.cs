using System.Collections.Generic;
using System.Linq;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameLobby : MonoBehaviour
{

    private Lobby joinedLobby;
    private Lobby hostLobby;
    private float heartBeatTimer = 0;
    private const float HeartBeatTime = 15;
    private string playerName = "Johan";

    private async void Start()
    {
        playerName = "Johan" + Random.Range(0,100).ToString();

        await UnityServices.InitializeAsync();


        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed In " + AuthenticationService.Instance.PlayerId);
        };

        Inputs.Controls.Main.L.performed += CreateLobby;
        Inputs.Controls.Main.M.performed += ListLobbies;

        //Signs in and creates new Account
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private void Update()
    {
        HandleHeartbeat();
    }

    private void HandleHeartbeat()
    {
        if (hostLobby == null) return;
        heartBeatTimer += Time.deltaTime;
        if(heartBeatTimer>= HeartBeatTime)
        {
            DoHeartBeat();
            heartBeatTimer -= HeartBeatTime;
        }
    }

    private async void DoHeartBeat()
    {
        await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
    }

    private void CreateLobby(InputAction.CallbackContext context)
    {
        Debug.Log("Calling Create Lobby.");
        CreateLobbyAsync();
    }
    
    private void ListLobbies(InputAction.CallbackContext context)
    {
        Debug.Log("Calling Update Lobbies.");
        ListLobbiesAsync();
    }

    private async void ListLobbiesAsync()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();
            LobbyUI.Instance.UpdateLobbyButtons(queryResponse.Results.ToList());
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
            joinedLobby = lobby;
            UIController.Instance.JoinGameLobby(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
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
            joinedLobby = lobby;
            UIController.Instance.JoinGameLobby(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void CreatePrivateLobbyAsync()
    {
        try
        {
            string lobbyName = "Private Lobby";
            int maxPlayers = 2;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions
            {
                IsPrivate = true,
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            Debug.Log("Created private Lobby "+lobbyName+", with max players: "+maxPlayers+ " JoinCode: "+lobby.LobbyCode);

            hostLobby = lobby;
            joinedLobby = lobby;
            UIController.Instance.JoinGameLobby(joinedLobby);
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
            string lobbyName = "New Lobby ";
            int maxPlayers = 2;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions {
                IsPrivate = false,
                Player = GetPlayer()
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers,createLobbyOptions);

            Debug.Log("Created Lobby "+lobbyName+", with max players: "+maxPlayers);
            // Auto Join This Game If you are the creator
            hostLobby = lobby;
            joinedLobby = lobby;
            UIController.Instance.JoinGameLobby(joinedLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    
    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>{
                { "playerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName)}
            }
        };
    }

    public async void LeaveLobbyAsync()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);
            joinedLobby = null;
            UIController.Instance.ShowMainLobby();
            Debug.Log("Player left game lobby");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void UpdateLobby()
    {
        ListLobbiesAsync();
    }
}
