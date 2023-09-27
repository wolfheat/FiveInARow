using System;
using TMPro;
using UnityEngine;

public class InfoController : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] TextMeshProUGUI winnertext;

    public void ShowPanel(bool show=true)
    {
        panel.SetActive(show);
    }

    internal void SetWinner(string name)
    {
        winnertext.text = name + " Won";
    }
}
