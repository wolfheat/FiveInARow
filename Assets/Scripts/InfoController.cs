using System;
using TMPro;
using UnityEngine;

public class InfoController : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject waitingForPlayersObject;
    [SerializeField] TextMeshProUGUI winnertext;

    public void ShowPanel(bool show=true)
    {
        panel.SetActive(show);
    }

    internal void SetWinner(string name)
    {
        winnertext.text = name + " Won";
    }
    public void RequestLobby()
    {
        Debug.Log("Request Lobby");
        UIController.Instance.AddRPCInfo("Request Lobby");

    }
    public void RequestRematch()
    {
        Debug.Log("Request Rematch");
        UIController.Instance.AddRPCInfo("Request Rematch");

    }

    public void WaitingForPlayers(bool SetActive)
    {
        waitingForPlayersObject.SetActive(SetActive);
    }
}
