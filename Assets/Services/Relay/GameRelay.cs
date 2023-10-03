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
    public static GameRelay Instance;
    public static Allocation JoinedRelay { get; set; }

    private float updateTimer = 0;
    private float UpdateTime = 1.0f;
    private string JoinCode { get; set;}
    private string ip;
    private int port;
    private byte[] connectionData;
    private System.Guid allocationID;


    private async void Start()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;

        try { 
            // Check if signed in?
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

    private void Update()
    {
        if (JoinedRelay != null)
        {
            updateTimer-= Time.deltaTime;
            if(updateTimer <= 0)
            {
                updateTimer += UpdateTime;
                // Update Here
            }

        }
    }

    public async Task<string> CreateRelayAsync()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("Join Code: "+ JoinCode);

            RelayServerData relayServerData = new RelayServerData(allocation,"dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            UIController.Instance.SetConnectionInfo("Created allocation ["+ JoinCode + "] ID:"+allocation.AllocationId.ToString());
            
            SetConnectionData(allocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls"),allocation.AllocationId, allocation.ConnectionData);
            
            SetConnectionDataB("Created the Relay");

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

        Debug.Log("Connection Data (my): " + c.AsString());
        Debug.Log("Connection Data (Bitconverter): " + BitConverter.ToString(c));

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

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();
            if(AuthenticationService.Instance.IsSignedIn)
            SetConnectionDataB("Joined the Relay - signed in: ("+ AuthenticationService.Instance.IsSignedIn+")");
            SetConnectionData(joinAllocation.ServerEndpoints.First(conn => conn.ConnectionType == "dtls"), joinAllocation.AllocationId, joinAllocation.ConnectionData);

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