using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkCommunicator : NetworkBehaviour
{

    public static NetworkCommunicator Instance;
    public static int PlayerIndex { set; get; }
    public static bool IsMyTurn { get; private set; }



    private bool[] acceptedRematch = new bool[2];
    private bool restartNotificationBeingDelivered;
    private bool waitForPlayersToJoin = true;

    private void Start()
    {
        if (Instance == null) Destroy(Instance);
        Instance = this;
    }

    private void Update()
    {
        // Only Server checks for rematch when state is paused
        if (!IsServer) return;


        if (AllPlayersWantRematch())
        {
            waitForPlayersToJoin = true;
        }

        // Wait for all players
        if (waitForPlayersToJoin)
        {
            if (NetworkManager.ConnectedClients.Count >= 2)
            {
                Debug.Log("At least two players have joined");
                waitForPlayersToJoin = false;
                ResetRematchSize();

                // Use random starter
                int randomstarter = Random.Range(0, NetworkManager.ConnectedClients.Count);
                SendRematchConfirmationClientRpc(randomstarter);
            }
        }
                
    }

    private void ResetMatchCounter()
    {
        ResetRematchSize();
        restartNotificationBeingDelivered = false;
    }

    private bool AllPlayersWantRematch()
    {
        foreach (bool value in acceptedRematch) 
        if(!value) return false;
        return true;
    }

    private void OnEnable()
    {
        Inputs.Controls.Main.L.performed += SetConnectedPlayersInfo;
        Inputs.Controls.Main.N.performed += SendRandomPlacement;
    }

    public void ResetRematchSize()
    {
        int size = NetworkManager.ConnectedClients.Count;   
        UIController.Instance.AddRPCInfo("Setting Rematch array to size: " + size+ " restartNotice=false");
        Debug.Log("Setting Rematch size "+size);
        acceptedRematch = new bool[size];
    }

    private void SetConnectedPlayersInfo(InputAction.CallbackContext callbackContext)
    {
        Debug.Log("Updating Connected players list");
        UIController.Instance.SetConnectedPlayers(GetConnectedPlayers());
    }
    
    private void SendRandomPlacement(InputAction.CallbackContext callbackContext)
    {
        if (StateController.Instance.State != State.Playing) return;
        if (!IsMyTurn)
        {
            Debug.Log("Not Your turn");
            return;
        }
        Vector2Int randomplacement = GameController.Instance.GetRandomPosition();

        UIController.Instance.AddRPCInfo("Sending Position for validation: " + randomplacement);
        // Show Info SetWaitingInfoController
        UIController.Instance.ShowPopupInfoControllerMessage("Sending " + randomplacement + " placement for validation");
        // Use this to send a random placement

        SendPlacementServerRpc(randomplacement);
    }
    
    [ServerRpc(RequireOwnership = false)]
    public void SendPlacementServerRpc(Vector2Int pos, ServerRpcParams rpcParams = default)
    {
        Debug.Log("Server recieved request: " + pos + " from: "+rpcParams.Receive.SenderClientId);
        
        bool placementAccepted = ServerGameController.Instance.HandlePlacementInput(pos,(int)rpcParams.Receive.SenderClientId);

        if (!placementAccepted)
        {
            UIController.Instance.AddRPCInfo("Position placement, not accepted: " + pos+" from: " + rpcParams.Receive.SenderClientId);
            ClientRpcParams clientRpcParams = new ClientRpcParams { 
                Send = new ClientRpcSendParams { 
                    TargetClientIds = new List<ulong>{ rpcParams.Receive.SenderClientId }
                } 
            };

            SendPlacementRejectionClientRpc(clientRpcParams);
            return;
        }

        UIController.Instance.AddRPCInfo("Position placement, accepted: " + pos+" from: " + rpcParams.Receive.SenderClientId);
        SendPlacementClientRpc(new Vector3Int(pos.x,pos.y, (int)rpcParams.Receive.SenderClientId));
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyWantRematchServerRpc(ServerRpcParams rpcParams = default)
    {
        int clientID = (int)rpcParams.Receive.SenderClientId;
        if (acceptedRematch.Length > clientID)
            acceptedRematch[clientID] = true;

        UIController.Instance.AddRPCInfo("Server Recieved Request request from: " + clientID);
    }

    [ServerRpc(RequireOwnership = true)]
    public void SendWinConditionServerRpc(int winner)
    {
        Debug.Log("Server recieved winner from host: " + winner);
    
        UIController.Instance.AddRPCInfo("Server recieved winner: " + winner);

        SendWinConditionClientRpc(winner);
    }

    [ClientRpc]
    public void SendRematchConfirmationClientRpc(int starter)
    {
        Debug.Log("Client recieved rematch start notice: ");    
        UIController.Instance.AddRPCInfo("Client recieved rematch start notice");
        UIController.Instance.ShowWaitingForAllPlayers(false);
        GameController.Instance.ResetGame();
        if (IsServer)
        {
            UIController.Instance.AddRPCInfo("Server resets match counter");
            ResetMatchCounter();
        }
        HandleRecievedGameStarter(starter);
    }


    [ClientRpc]
    public void SendWinConditionClientRpc(int winner)
    {
        Debug.Log("Client recieved winner: " + winner);
    
        UIController.Instance.AddRPCInfo("Client Recieved Winner: " + winner);
        GameController.Instance.ShowWinner(winner);
    }

    [ClientRpc]
    public void SendPlacementClientRpc(Vector3Int pos)
    {
        Debug.Log("Client recieved request: " + pos);
        UIController.Instance.AddRPCInfo("Recieved Position as Client: " + pos);
        
        bool gameComplete = GameController.Instance.PlaceMarkerByPlayerIndex((Vector2Int)pos,pos.z);

        HandleRecievedValidPlacement(pos.z);
        
        // Only Host reports win?
        if (gameComplete && IsServer)
            SendWinConditionServerRpc(pos.z);
    }

    [ClientRpc]
    public void SendPlacementRejectionClientRpc(ClientRpcParams clientRecieveParams)
    {
        int rejectedID = (int)clientRecieveParams.Send.TargetClientIds[0];
        Debug.Log("Client recieved rejection for clientID: " + rejectedID);

        UIController.Instance.AddRPCInfo("Recieved Position Rejection: " + rejectedID);
                
        HandleRecievedInvalidPlacement();
    }

    [ClientRpc]
    public void SendStarterClientRpc(int starter)
    {
        Debug.Log("Client recieved starter: " + starter);
        UIController.Instance.AddRPCInfo("Client recieved starter: " + starter);
        
        HandleRecievedGameStarter(starter);
    }

    private void HandleRecievedInvalidPlacement()
    {
        // Input rejected
        UIController.Instance.HidePopupInfoControllerMessage();
    }

    private void HandleRecievedValidPlacement(int senderID)
    {
        // Its the player that did not place last markers turn
        int playerTurn = senderID == 0 ? 1 : 0;

        // Recieving a placement that I sent to the server
        SetMyTurn(PlayerIndex == playerTurn?true:false);
        
        UIController.Instance.HidePopupInfoControllerMessage();

        // Show Info SetWaitingInfoController
        UIController.Instance.AddRPCInfo(playerTurn + " Turn. Now its my turn: " + (PlayerIndex == playerTurn));

    }
    
    private void HandleRecievedGameStarter(int starter)
    {
        SetMyTurn(PlayerIndex == starter?true:false);
        
        // Show Info SetWaitingInfoController
        UIController.Instance.AddRPCInfo(PlayerIndex==starter?"You go first!":"Opponent goes first!");
    }

    private void SetMyTurn(bool myTurn)
    {
        IsMyTurn = myTurn;
        UIController.Instance.MyTurnIndicator(IsMyTurn);
    }

    /*
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
}*/

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
    
    public int GetConnectedPlayersAmount()
    {
        return NetworkManager.ConnectedClients.Count;
    }

    private int Char2Int(char c)
    {
        char playerIndexChar = (char)c;
        return (int)char.GetNumericValue(playerIndexChar);
    }        

}
