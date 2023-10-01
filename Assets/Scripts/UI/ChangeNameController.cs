using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChangeNameController : MonoBehaviour
{
    [SerializeField] TMP_InputField inputField;
    [SerializeField] UIController uIController;
    [SerializeField] LobbyUI lobbyUI;

    public string PlayerName { get; private set; }

    public void SetName(string Name)
    {
        PlayerName = Name;
        inputField.text = PlayerName;
    }
    
    private void ValidateName()
    {
        if (inputField.text.Length == 0)
            inputField.text = "Johan";
    }

    // Unity Event
    public void ChangeName()
    {
        ValidateName();
        Debug.Log("Change Name to "+inputField.text);        

        PlayerName = inputField.text;
        lobbyUI.UpdatePlayerName(PlayerName);
        gameObject.SetActive(false);
    }

}
