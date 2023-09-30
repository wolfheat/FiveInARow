using TMPro;
using UnityEngine;

public class PlayerButton : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerText;

    public void SetText(string lobbyName)
    {
        playerText.text = lobbyName;
    }
}
