using System;
using UnityEngine;


public class ServerGameController : MonoBehaviour
{
    public static ServerGameController Instance { get; private set; }

    public int PlayerTurnID { get; set; }

    private void Start()
    {
        if (Instance != null) Destroy(gameObject);
        Instance = this;
    }

    public bool HandlePlacementInput(Vector2Int pos,int playerID)
    {
        if (playerID != PlayerTurnID) return false;

        Debug.Log("Server recieved placement request. Position: "+pos+" from: "+playerID);
        UIController.Instance.AddRPCInfo("Server recieved placement: " + pos + " from: " + playerID);
        bool isValid = StateController.Instance.State == State.Playing && GameController.Instance.ValidatePlacement(pos,playerID);
        return isValid;
    }

    public void NextPlayer()
    {
        PlayerTurnID = (PlayerTurnID + 1)%2;
    }
}