using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributeController : NetworkBehaviour
{
    [HideInInspector] public int currentExperience = 0;
    [HideInInspector] public int currentLevel = 1;
    [HideInInspector] public int maxExperience = 100;
    [HideInInspector] public int criticalChance;
    [HideInInspector] public int health;
    [HideInInspector] public int healthRegen;
    [HideInInspector] public int cooldown;
    [HideInInspector] public int damage;
    [HideInInspector] public int speed;

    public NetworkVariable<int> CurrentHP = new NetworkVariable<int>();

    public GameObject healthBar;
    public Image healthBarImage;

    [Header("Level Up Data")]
    public ClassLevelUPDataSO fairyData;
    public ClassLevelUPDataSO skeletonData;
    public ClassLevelUPDataSO horseData;

    [Header("Experience")]
    public int experienceIncreaseAmount = 30;

    [Header("Fairy")]
    public int fairyMouseLeftSkillPercentage = 10;
    public int fairyMouseRightSkillPercentage = 10;
    public int fairyQSkillPercentage = 10;
    public int fairyFSkillPercentage = 10;

    [Header("Skeleton")]
    public int skeletonMouseLeftSkillPercentage = 10;
    public int skeletonMouseRightSkillPercentage = 10;
    public int skeletonQSkillPercentage = 10;
    public int skeletonFSkillPercentage = 10;

    [Header("Horse")]
    public int horseMouseLeftSkillPercentage = 10;
    public int horseMouseRightSkillPercentage = 10;
    public int horseQSkillPercentage = 10;
    public int horseFSkillPercentage = 10;

    public void SwitchHealthBar(bool target)
    {
        healthBar.SetActive(target);
    }

    public void OnHealthValueChanged(int previous, int current)
    {
        NumberWorldSpacePooler.Instance.ShowNumberInWorld(current - previous, transform.position + new Vector3(0f, 1f, 0f));
        healthBarImage.fillAmount = (float) current / health;
        if(IsOwner)
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(CurrentHP.Value, health);
    }

    public void ReceiveDamage(int damage)
    {
        if (IsOwner) return;
        ReceiveDamageRpc(NetworkObjectId, damage);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong targetNetworkObjectId, int damage)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var playerObj))
        {
            var player = playerObj.GetComponent<PlayerAttributeController>();
            if (player != null)
            {
                player.CurrentHP.Value -= damage;
            }
        }
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.K) && IsServer)
        {
            HealEverybody();
        }
    }

    private void HealEverybody()
    {
        foreach (var item in NetworkManager.Singleton.SpawnManager.SpawnedObjects)
        {
            PlayerAttributeController playerAttributeController = item.Value.GetComponent<PlayerAttributeController>();
            if (playerAttributeController != null)
            {
                playerAttributeController.CurrentHP.Value = health;
            }
        }
    }

    public void ReceiveExp(ulong targetOwnerClientId, int experience)
    {
        var rpcParams = new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Single(targetOwnerClientId, RpcTargetUse.Persistent)
            }
        };
        SendExpClientRpc(experience, rpcParams);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendExpClientRpc(int experience, RpcParams rpcParams = default)
    {
        currentExperience += experience;
        if (currentExperience >= maxExperience)
        {
            currentExperience = maxExperience - currentExperience;
            currentLevel += 1;
            maxExperience = maxExperience + experienceIncreaseAmount;
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerLevel(currentLevel);
        }
        InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerExperience(currentExperience, maxExperience);
    }
}
