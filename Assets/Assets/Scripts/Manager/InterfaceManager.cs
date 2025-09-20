using Unity.Netcode;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    public static InterfaceManager Instance;

    public PlayerInterfaceController playerInterfaceController;
    private void Awake()
    {
        Instance = this;
    }

    public void UpdatePlayerHP(int currentValue, int maxValue)
    {
        playerInterfaceController.UpdatePlayerHp(currentValue, maxValue);
    }

    public void UpdatePlayerSkillFirstCooldown(float time)
    {
        playerInterfaceController.UpdatePlayerSkillFirstCooldown(time);
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
