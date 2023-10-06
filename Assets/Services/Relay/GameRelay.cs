using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class GameRelay : MonoBehaviour
{
    public class ConnectionType{public static string dtls = "dtls";public static string udp = "udp";}

    public static GameRelay Instance;
    public static Allocation JoinedRelay { get; set; }
    private string JoinCode { get; set;}

    private float updateTimer = 0;
    private float UpdateTime = 1.0f;
    private string ip;
    private int port;
    private byte[] connectionData;
    private Guid allocationID;

    private async void Start()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        // Check if signed in?
        //if (AuthenticationService.Instance.SessionTokenExists) return;

        // Initialize the connection to the Authentication Service + Sign In
        try {

            await UnityServices.InitializeAsync();

            AuthenticationService.Instance.SignedIn += () =>
            {
                Debug.Log("Signed In ID: " + AuthenticationService.Instance.PlayerId);
            };

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        } 
        catch (Exception e) 
        { 
            Debug.Log(e);
        }
    }

    public async Task<string> CreateRelayAsync()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: "+ JoinCode);

            RelayServerData relayServerData = new RelayServerData(allocation, ConnectionType.udp);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            // Show values in debug window
            UIController.Instance.SetConnectionInfo("Created allocation ["+ JoinCode + "] ID:"+allocation.AllocationId.ToString());            
            SetConnectionData(allocation.ServerEndpoints.First(conn => conn.ConnectionType == ConnectionType.udp),allocation.AllocationId, allocation.ConnectionData);            
            SetConnectionDataB("Created the Relay");
            NetworkCommunicator.Instance.UpdatePlayerNamesFromLobby();
            return JoinCode;
        }
        catch(RelayServiceException e)
        {
            SetConnectionDataB("Failed to connect to Relay");
            Debug.Log(e);
        }
        return null;
    }

    private void SetConnectionData(RelayServerEndpoint r, Guid a, byte[] c)
    {
        ip = r.Host;
        port = r.Port;
        allocationID = a;
        connectionData = c;
        UIController.Instance.SetConnectionInfo("Host IP: [" + ip + "] - port:" + port+" Code: "+ JoinCode + "\nAllocation: " + allocationID+ "\n");
    }
    
    private void SetConnectionDataB(string message)
    {
        UIController.Instance.SetConnectionInfoB(message);
    }

    public async void JoinRelayAsync(string joinCode)
    {
        try
        {
            JoinCode = joinCode;
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined with Join Code: "+joinCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, ConnectionType.udp);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            bool didStart = NetworkManager.Singleton.StartClient();

            SetConnectionDataB("Joined the Relay - signed in: ("+ AuthenticationService.Instance.IsSignedIn+") clientStarted: "+didStart);
            SetConnectionData(joinAllocation.ServerEndpoints.First(conn => conn.ConnectionType == ConnectionType.udp), joinAllocation.AllocationId, joinAllocation.ConnectionData);
            NetworkCommunicator.Instance.ResetScore();
            NetworkCommunicator.Instance.UpdatePlayerNamesFromLobby();
            // Set playernames from Lobby?

        }
        catch (RelayServiceException e)
        {
            SetConnectionDataB("Could not Join Relay");
            Debug.Log(e);
        }
    }

    // Disconnect method - Not implemented yet
    public void Disconnect(ulong clientId)
    {
        Debug.Log("Trying to disconnect Client");
        try
        {
            NetworkManager.Singleton.DisconnectClient(clientId);
        }
        catch (RelayServiceException e)
        {
            SetConnectionDataB("Could not Join Relay");
            Debug.Log(e);
        }
    }
}

public static class MyExtensions
{
    public static string AsString(this byte[] bytes)
    {
        StringBuilder asString = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
        {
            asString.Append(bytes[i].ToString());
        }
        return asString.ToString();
}
}