using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyItem : MonoBehaviour
{
    public TMP_Text lobbyName;
    public TMP_Text lobbyAmount;
    public Button connectLobbyButton;
    public string lobbyId;

    public void SetupLobby(string lobbyName, string lobbyAmount, string lobbyId)
    {
        this.lobbyName.text = lobbyName;
        this.lobbyAmount.text = lobbyAmount;
        this.lobbyId = lobbyId;
    }

    void Start()
    {
        connectLobbyButton.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        _ = LobbyManager.Instance.JoinLobbyById(lobbyId);
    }
}