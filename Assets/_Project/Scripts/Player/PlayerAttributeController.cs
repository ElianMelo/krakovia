using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributeController : NetworkBehaviour
{
    [HideInInspector] public int currentExperience = 0;
    [HideInInspector] public int currentLevel = 1;
    [HideInInspector] public int maxExperience = 100;

    private float criticalChance = 0;
    private float health = 0;
    private float healthRegen = 0;
    private float cooldown = 0;
    private float damage = 0;
    private float speed = 0;

    public float CriticalChance => criticalChance;
    public float Health => health;
    public float HealthRegen => healthRegen;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float Speed => speed;

    public NetworkVariable<int> CurrentHP = new NetworkVariable<int>();

    public GameObject healthBar;
    public Image healthBarImage;

    [Header("Level Up Data")]
    public ClassLevelUPDataSO fairyData;
    public ClassLevelUPDataSO skeletonData;
    public ClassLevelUPDataSO horseData;

    private LevelUpData levelUpData;

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

    public void SetupClassLevelUp(PlayerClass playerClass)
    {
        ClassLevelUPDataSO classLevelUPDataSO = GetDataBasedOnClass(playerClass);
        levelUpData = classLevelUPDataSO.levelUpData;
        foreach (var attributeData in classLevelUPDataSO.initialLevelData.attributeDatas)
        {
            UpdateAttributeData(attributeData.attribute, attributeData.flatAmount);
        }
    }

    private ClassLevelUPDataSO GetDataBasedOnClass(PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Fairy: return fairyData;
            case PlayerClass.Skeleton: return skeletonData;
            case PlayerClass.Horse: return horseData;
            default: return skeletonData;
        }
    }

    private void UpdateAttributeData(Attribute attribute, float value)
    {
        switch (attribute)
        {
            case Attribute.CriticalChance: criticalChance += value; return;
            case Attribute.Health: health += value; return;
            case Attribute.HealthRegen: healthRegen += value; return;
            case Attribute.Cooldown: cooldown += value; return;
            case Attribute.Damage: damage += value; return;
            case Attribute.Speed: speed += value; return;
            default: return;
        }
    }

    private void LevelUp()
    {
        foreach (var attributeData in levelUpData.attributeDatas)
        {
            UpdateAttributeData(attributeData.attribute, attributeData.flatAmount);
        }
    }

    public void OnHealthValueChanged(int previous, int current)
    {
        NumberWorldSpacePooler.Instance.ShowNumberInWorld(current - previous, transform.position + new Vector3(0f, 1f, 0f));
        healthBarImage.fillAmount = (float) current / health;
        if(IsOwner)
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(CurrentHP.Value, (int) health);
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
                playerAttributeController.CurrentHP.Value = (int) health;
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
            LevelUp();
        }
        InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerExperience(currentExperience, maxExperience);
    }
}
