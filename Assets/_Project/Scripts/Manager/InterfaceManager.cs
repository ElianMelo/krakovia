using TMPro;
using Unity.Netcode;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance;

    public TMP_Dropdown dropdown;

    public PlayerInterfaceController _playerInterfaceController;

    public PlayerInterfaceController PlayerInterfaceController
    {
        get => _playerInterfaceController;
        set
        {
            _playerInterfaceController = value;
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public PlayerClass GetSelectedClass()
    {
        return (PlayerClass) dropdown.value;
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
