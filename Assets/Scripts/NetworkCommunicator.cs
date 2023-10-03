using System;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkCommunicator : NetworkBehaviour
{

    public static NetworkCommunicator Instance;
    public static int PlayerIndex { set; get; }

    private void Start()
    {
        if (Instance == null) Destroy(Instance);
        Instance = this;
    }

    private void OnEnable()
    {
        Inputs.Controls.Main.L.performed += SetConnectedPlayersInfo;
        Inputs.Controls.Main.M.performed += SendServerMessage;
        Inputs.Controls.Main.N.performed += SendPlacement;
    }

    private void SetConnectedPlayersInfo(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Updating Connected players list");
        UIController.Instance.SetConnectedPlayers(GetConnectedPlayers());
    }
    
    private void SendPlacement(InputAction.CallbackContext callbackContext)
    {
        ServerRpcParams parameters = new ServerRpcParams() {Receive = new ServerRpcReceiveParams() { } };
        SendPlacementServerRpc(new Vector2Int(4,5));
    }
    
    private void SendServerMessage(InputAction.CallbackContext callbackContext)
    {
        // Check if this is a legal move then send it to all clients
        if (!PlacementIsLegal()) return;

        
        string serverMessage = PlayerIndex.ToString();
        Debug.Log("Send Message to Server ("+serverMessage+")");
        UIController.Instance.AddRPCInfo("Sending my ID to Server: " + serverMessage);
        SendMessageServerRpc(serverMessage);
    }

    private bool PlacementIsLegal()
    {
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SendPlacementServerRpc(Vector2Int pos, ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server recieved request: " + pos + " from: "+rpcParams.Receive.SenderClientId);
        UIController.Instance.AddRPCInfo("Recieved Position as Server: " + pos+" from: " + rpcParams.Receive.SenderClientId);
        SendPlacementClientRpc(new Vector3Int(pos.x,pos.y, (int)rpcParams.Receive.SenderClientId));
    }

    [ClientRpc]
    public void SendPlacementClientRpc(Vector3Int pos)
    {
        Debug.Log("Client recieved request: " + pos);
        UIController.Instance.AddRPCInfo("Recieved Position as Client: " + pos);
        UIController.Instance.AddRPCInfo("Position Sent By: " + pos.z);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SendMessageServerRpc(string message)
    {
        Debug.Log("Server recieved request: " + message);
        string randomPositionString = FindObjectOfType<GameController>().GetRandomPositionString();
        string newMessage = message + randomPositionString;
        UIController.Instance.AddRPCInfo("Recieved Message as Server: " + newMessage + " Players: " + GetConnectedPlayers());
        SendMessageClientRpc(newMessage);
    }

    [ClientRpc]
    public void SendMessageClientRpc(string message)
    {
        Debug.Log("Clients recieved message from server: " + message);
        int playerIndex = Char2Int(message[0]);
        Vector3Int pos = new Vector3Int(Char2Int(message[1]), Char2Int(message[2]),1);
        PlaceMarkerRequest(pos, playerIndex);
        string addition = IsHost ? (" Players: " + GetConnectedPlayers()) : "";
        UIController.Instance.AddRPCInfo("Recieved Message as Client: "+message + addition);
    }

    private string GetConnectedPlayers()
    {
        StringBuilder sb = new StringBuilder();
        foreach (var item in NetworkManager.ConnectedClients)
        {
            sb.Append(item.Key.ToString());
            sb.Append("|");
        }
        return sb.ToString();
    }

    private int Char2Int(char c)
    {
        char playerIndexChar = (char)c;
        return (int)char.GetNumericValue(playerIndexChar);
    }        

    private void PlaceMarkerRequest(Vector3Int pos, int playerIndex)
    {
        FindObjectOfType<GameController>().PlaceMarkerByPlayerIndex(pos,playerIndex);
    }
}
