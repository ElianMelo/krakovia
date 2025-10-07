using TMPro;
using Unity.Netcode;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance;

    public TMP_InputField playerNameInputField;
    public TMP_Dropdown classSelectionDropdown;

    public PlayerInterfaceController _playerInterfaceController;
    public PvPInterfaceController _pvPInterfaceController;

    public PlayerInterfaceController PlayerInterfaceController
    {
        get => _playerInterfaceController;
        set
        {
            _playerInterfaceController = value;
        }
    }

    public PvPInterfaceController PvPInterfaceController => _pvPInterfaceController;

    private void Awake()
    {
        Instance = this;
    }

    public PlayerClass GetSelectedClass()
    {
        return (PlayerClass) classSelectionDropdown.value;
    }

    public string GetPlayerName()
    {
        if(playerNameInputField.text.Length > 15)
        {
            return playerNameInputField.text.Substring(0, 14) + ".";
        } else
        {
            return playerNameInputField.text;
        }
    }

    public void UpdatePlayerSkillFirstCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillFirstCooldown(time);
    }

    public void UpdatePlayerSkillSecondCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillSecondCooldown(time);
    }

    public void UpdatePlayerSkillThirdCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillThirdCooldown(time);
    }

    public void UpdatePlayerSkillForthCooldown(float time)
    {
        PlayerInterfaceController.UpdatePlayerSkillForthCooldown(time);
    }

    public void UpdateDashSkillCooldown(float time)
    {
        PlayerInterfaceController.UpdateDashSkillCooldown(time);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
