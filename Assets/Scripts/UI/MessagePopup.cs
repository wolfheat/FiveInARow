using TMPro;
using UnityEngine;

public class MessagePopup : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI info;

    public void ShowMessage(string message)
    {
        gameObject.SetActive(true);
        info.text = message;
    }

    public void BackToLobby()
    {
        gameObject.SetActive(false);
    }
}
