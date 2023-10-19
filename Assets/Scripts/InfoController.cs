using System;
using TMPro;
using UnityEngine;

public class InfoController : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] GameObject waitingForPlayersObject;
    [SerializeField] GameObject rematchButton;
    [SerializeField] GameObject cancelRematchButton;
    [SerializeField] TextMeshProUGUI winnertext;
    [SerializeField] TextMeshProUGUI rematchText;

    private void Start()
    {
        ShowPanel(false);
    }

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
        rematchButton.gameObject.SetActive(false);
        cancelRematchButton.gameObject.SetActive(true);
        rematchText.text = "Rematch sent";
        Debug.Log("Request Rematch");
        UIController.Instance.AddRPCInfo("Request Rematch");
        NetworkCommunicator.Instance.NotifyWantRematchServerRpc();
    }
    
    public void CancelRequestRematch()
    {
        Debug.Log("Request Rematch Canceled");
        rematchButton.gameObject.SetActive(true);
        cancelRematchButton.gameObject.SetActive(false);
        rematchText.text = "Rematch";

        UIController.Instance.AddRPCInfo("Request Rematch Canceled");
        NetworkCommunicator.Instance.NotifyNoLongerWantRematchServerRpc();
    }

    public void WaitingForPlayers(bool SetActive)
    {
        waitingForPlayersObject.SetActive(SetActive);
    }
}
