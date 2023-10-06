using TMPro;
using UnityEngine;

public class WaitingInfoController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI infoText;

    public void SetInfoText(string info)
    {
        infoText.text = info;
    }
}
