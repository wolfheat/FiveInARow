using System;
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
        Inputs.Controls.Main.L.performed += SendClientMessage;
        Inputs.Controls.Main.M.performed += SendServerMessage;
    }

    private void SendClientMessage(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Trying to send a client message");
        SendMessageClientRpc("Sending This client message to all clients");
    }
    
    private void SendServerMessage(InputAction.CallbackContext callbackContext)
    {
        // Check if this is a legal move then send it to all clients
        if (!PlacementIsLegal()) return;

        
        string serverMessage = PlayerIndex.ToString();
        Debug.Log("Send Message to Server ("+serverMessage+")");
        SendMessageServerRpc(serverMessage);
    }

    private bool PlacementIsLegal()
    {
        return true;
    }

    [ServerRpc(RequireOwnership = false)]
    //public void SendMessageServerRpc(ServerRpcParams = new ServerRpcParams(), string message)
    public void SendMessageServerRpc(string message)
    {
        Debug.Log("Server recieved request: " + message);
        //FindObjectOfType<GameController>().PlaceMarker(new Vector3Int(4,4,1),Tiletype.O);
        string randomPositionString = FindObjectOfType<GameController>().GetRandomPositionString();
        SendMessageClientRpc(message+randomPositionString);
    }

    [ClientRpc]
    public void SendMessageClientRpc(string message)
    {
        Debug.Log("Clients recieved message from server: " + message);
        int playerIndex = Char2Int(message[0]);
        Vector3Int pos = new Vector3Int(Char2Int(message[1]), Char2Int(message[2]),1);
        PlaceMarkerRequest(pos, playerIndex);
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
