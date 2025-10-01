using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributeController : NetworkBehaviour
{
    [HideInInspector] public int currentExperience = 0;
    [HideInInspector] public int currentLevel = 1;
    [HideInInspector] public int maxExperience = 1000;

    private float criticalChance = 0;
    private float healthRegen = 0;
    private float cooldown = 0;
    private float damage = 0;
    private float speed = 0;

    public float CriticalChance => criticalChance / 100;
    public float HealthRegen => healthRegen;
    public float Cooldown => cooldown;
    public float Damage => damage;
    public float Speed => speed;

    public NetworkVariable<float> CurrentHP = new NetworkVariable<float>();
    public NetworkVariable<float> MaxHP = new NetworkVariable<float>();

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

    private void Start()
    {
        if (!IsOwner) return;
        StartCoroutine(LifeRegenCoroutine());
    }

    private IEnumerator LifeRegenCoroutine()
    {
        while (true) {
            // todo: change this if its consuming too much latency
            yield return new WaitForSeconds(1f);
            if(healthRegen != 0 && CurrentHP.Value != MaxHP.Value) SendLifeRegenerationRpc(healthRegen, NetworkObjectId);
        }
    }

    [Rpc(SendTo.Server)]
    private void SendLifeRegenerationRpc(float value, ulong playerNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var playerObj))
        {
            var player = playerObj.GetComponent<PlayerAttributeController>();
            if (player == null) return;
            
            if(CurrentHP.Value + value >= MaxHP.Value)
            {
                CurrentHP.Value = MaxHP.Value;
            } else
            {
                CurrentHP.Value += value;
            }
        }
    }

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
            case Attribute.Health: UpdateHealth(value); return;
            case Attribute.HealthRegen: healthRegen += value; return;
            case Attribute.Cooldown: cooldown += value; return;
            case Attribute.Damage: damage += value; return;
            case Attribute.Speed: speed += value; return;
            default: return;
        }
    }

    private void UpdateHealth(float value)
    {
        UpdateHealthRpc(NetworkObjectId, value);
    }

    [Rpc(SendTo.Server)]
    private void UpdateHealthRpc(ulong playerNetworkObjectId, float value)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out var playerObj))
        {
            var player = playerObj.GetComponent<PlayerAttributeController>();
            if (player != null)
            {
                MaxHP.Value += value;
                CurrentHP.Value = MaxHP.Value;
            }
        }
    }

    private void LevelUp()
    {
        foreach (var attributeData in levelUpData.attributeDatas)
        {
            UpdateAttributeData(attributeData.attribute, attributeData.flatAmount);
        }
    }

    public void OnHealthValueChanged(float previous, float current)
    {
        healthBarImage.fillAmount = current / MaxHP.Value;
        if(IsOwner)
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(CurrentHP.Value, MaxHP.Value);
    }

    public void ReceiveDamageEnemy(int damage, bool isCritical)
    {
        ReceiveDamageRpc(NetworkObjectId, damage, isCritical);
    }

    public void ReceivePlayerDamage(int damage, bool isCritical)
    {
        if (IsOwner) return;
        ReceiveDamageRpc(NetworkObjectId, damage, isCritical);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong targetNetworkObjectId, int damage, bool isCritical)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var playerObj))
        {
            var player = playerObj.GetComponent<PlayerAttributeController>();
            if (player != null)
            {
                player.CurrentHP.Value -= damage; 
                SendDamageClientRpc(player.transform.position, damage, isCritical);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(Vector3 contactPoint, float damage, bool isCritical)
    {
        NumberWorldSpacePooler.Instance.ShowNumberInWorld((int)damage, transform.position + new Vector3(0f, 1f, 0f), isCritical);
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
                playerAttributeController.CurrentHP.Value = playerAttributeController.MaxHP.Value;
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
