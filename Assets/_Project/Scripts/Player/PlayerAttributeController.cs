using FIMSpace.FProceduralAnimation;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributeController : NetworkBehaviour
{
    public int currentExperience = 0;
    public int currentLevel = 1;
    public int maxExperience = 1000;

    private PlayerMovementController playerMovementController; 
    private PlayerClassController playerClassController;

    private float criticalChance = 0;
    private float healthRegen = 0;
    private float cooldown = 0;
    private float damage = 0;
    private float speed = 0;

    private const int MaxLevel = 20;

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
    [Header("Skill Damage Percentage")]
    public float fairyMouseLeftSkillDamagePercentage = 10;
    public float fairyMouseRightSkillDamagePercentage = 10;
    public float fairyQSkillDamagePercentage = 10;
    public float fairyFSkillDamagePercentage = 10;
    [Header("Sill Cooldown Percentage")]
    public float fairyMouseLeftSkillCooldownSeconds = 10;
    public float fairyMouseRightSkillCooldownSeconds = 10;
    public float fairyQSkillCooldownSeconds = 10;
    public float fairyFSkillCooldownSeconds = 10;

    [Header("Skeleton")]
    [Header("Skill Damage Percentage")]
    public float skeletonMouseLeftSkillDamagePercentage = 10;
    public float skeletonMouseRightSkillDamagePercentage = 10;
    public float skeletonQSkillDamagePercentage = 10;
    public float skeletonFSkillDamagePercentage = 10;
    [Header("Skill Cooldown Percentage")]
    public float skeletonMouseLeftSkillCooldownSeconds = 10;
    public float skeletonMouseRightSkillCooldownSeconds = 10;
    public float skeletonQSkillCooldownSeconds = 10;
    public float skeletonFSkillCooldownSeconds = 10;

    [Header("Horse")]
    [Header("Skill Damage Percentage")]
    public float horseMouseLeftSkillDamagePercentage = 10;
    public float horseMouseRightSkillDamagePercentage = 10;
    public float horseQSkillDamagePercentage = 10;
    public float horseFSkillDamagePercentage = 10;
    [Header("Skill Cooldown Percentage")]
    public float horseMouseLeftSkillCooldownSeconds = 10;
    public float horseMouseRightSkillCooldownSeconds = 10;
    public float horseQSkillCooldownSeconds = 10;
    public float horseFSkillCooldownSeconds = 10;

    public float GetSkillCalculatedCooldown(PlayerClass playerClass, SkillCommand skillCommand)
    {
        return GetSkillCooldownBasedOnClassAndCommands(playerClass, skillCommand) * (1 - (cooldown / 100));
    }

    private float GetSkillCooldownBasedOnClassAndCommands(PlayerClass playerClass, SkillCommand skillCommand)
    {
        switch (playerClass)
        {
            case PlayerClass.Fairy:
                switch (skillCommand)
                {
                    case SkillCommand.MouseLeft: return fairyMouseLeftSkillCooldownSeconds;
                    case SkillCommand.MouseRight: return fairyMouseRightSkillCooldownSeconds;
                    case SkillCommand.Q: return fairyQSkillCooldownSeconds;
                    case SkillCommand.F: return fairyFSkillCooldownSeconds;
                }
                break;
            case PlayerClass.Skeleton:
                switch (skillCommand)
                {
                    case SkillCommand.MouseLeft: return skeletonMouseLeftSkillCooldownSeconds;
                    case SkillCommand.MouseRight: return skeletonMouseRightSkillCooldownSeconds;
                    case SkillCommand.Q: return skeletonQSkillCooldownSeconds;
                    case SkillCommand.F: return skeletonFSkillCooldownSeconds;
                }
                break;
            case PlayerClass.Horse:
                switch (skillCommand)
                {
                    case SkillCommand.MouseLeft: return horseMouseLeftSkillCooldownSeconds;
                    case SkillCommand.MouseRight: return horseMouseRightSkillCooldownSeconds;
                    case SkillCommand.Q: return horseQSkillCooldownSeconds;
                    case SkillCommand.F: return horseFSkillCooldownSeconds;
                }
                break;
            default:
                break;
        }
        return 1f;
    }

    public float GetMultiplierBasedOnSkill(SkillCommand skillCommand, PlayerClass playerClass)
    {
        switch (playerClass)
        {
            case PlayerClass.Fairy:
                switch (skillCommand)
                {
                    case SkillCommand.MouseLeft: return fairyMouseLeftSkillDamagePercentage;
                    case SkillCommand.MouseRight: return fairyMouseRightSkillDamagePercentage;
                    case SkillCommand.Q: return fairyQSkillDamagePercentage;
                    case SkillCommand.F: return fairyFSkillDamagePercentage;
                    default: return fairyMouseLeftSkillDamagePercentage;
                }
            case PlayerClass.Skeleton:
                switch (skillCommand)
                {
                    case SkillCommand.MouseLeft: return skeletonMouseLeftSkillDamagePercentage;
                    case SkillCommand.MouseRight: return skeletonMouseRightSkillDamagePercentage;
                    case SkillCommand.Q: return skeletonQSkillDamagePercentage;
                    case SkillCommand.F: return skeletonFSkillDamagePercentage;
                    default: return skeletonMouseLeftSkillDamagePercentage;
                }
            case PlayerClass.Horse:
                switch (skillCommand)
                {
                    case SkillCommand.MouseLeft: return horseMouseLeftSkillDamagePercentage;
                    case SkillCommand.MouseRight: return horseMouseRightSkillDamagePercentage;
                    case SkillCommand.Q: return horseQSkillDamagePercentage;
                    case SkillCommand.F: return horseFSkillDamagePercentage;
                    default: return horseMouseLeftSkillDamagePercentage;
                }
            default: return 100;
        }
    }

    private void Start()
    {
        if (!IsOwner) return;
        playerMovementController = GetComponent<PlayerMovementController>();
        playerClassController = GetComponent<PlayerClassController>();
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
            case Attribute.Speed: UpdateSpeed(value); return;
            default: return;
        }
    }

    private void UpdateSpeed(float value)
    {
        speed += value;
        if(playerMovementController == null) playerMovementController = GetComponent<PlayerMovementController>();
        playerMovementController.SetupSpeed(speed);
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
        GetComponent<PlayerAttackController>().CheckUnlockSkill(currentLevel);
        if(currentLevel == 5)
        {
            InterfaceManager.Instance.ObjectiveInterfaceController.ChangeObjectiveToLevelTen();
        } else if(currentLevel == 10)
        {
            InterfaceManager.Instance.ObjectiveInterfaceController.ChangeObjectiveToLevelFifteen();
        } else if (currentLevel == 15)
        {
            InterfaceManager.Instance.ObjectiveInterfaceController.ChangeObjectiveToLevelTwenty();
        } else if (currentLevel == 20)
        {
            InterfaceManager.Instance.ObjectiveInterfaceController.ChangeObjectiveToLevelTwentyPos();
        }
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
        ReceiveDamageRpc(NetworkObjectId, damage, isCritical, true);
    }

    public void ReceivePlayerDamage(int damage, bool isCritical)
    {
        if (IsOwner) return;
        ReceiveDamageRpc(NetworkObjectId, damage, isCritical, false);
    }

    public void ReceivePlayerDamageFairy(int damage, bool isCritical)
    {
        ReceiveDamageRpc(NetworkObjectId, damage, isCritical, false);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong targetNetworkObjectId, int damage, bool isCritical, bool isEnemy)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var playerObj))
        {
            PlayerAttributeController player = playerObj.GetComponent<PlayerAttributeController>();
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (player != null)
            {
                if (!isEnemy && !playerController.isPvPActive.Value) return;
                player.CurrentHP.Value -= damage;
                if(player.CurrentHP.Value - damage <= 0)
                {
                    player.CurrentHP.Value = player.MaxHP.Value;
                    playerController.isPvPActive.Value = false;
                    var rpcParams = new RpcParams
                    {
                        Send = new RpcSendParams
                        {
                            Target = NetworkManager.Singleton.RpcTarget.Single(player.OwnerClientId, RpcTargetUse.Persistent)
                        }
                    };
                    DeathRpc(rpcParams);
                }
                SendDamageClientRpc(player.transform.position, damage, isCritical, player.CurrentHP.Value - damage <= 0);
            }
        }
    }

    [Rpc(SendTo.Everyone)]
    private void SendDamageClientRpc(Vector3 contactPoint, float damage, bool isCritical, bool isDead)
    {
        PlayerEffectPooler.Instance.ShowHitEffect(transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, 3f);
        if (isDead)
        {
            PlayerEffectPooler.Instance.ShowDeathEffect(transform.position + new Vector3(0f, 1f, 0f), Quaternion.identity, 3f);
        }
        NumberWorldSpacePooler.Instance.ShowNumberInWorld((int)damage, transform.position + new Vector3(0f, 1f, 0f), isCritical);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void DeathRpc(RpcParams rpcParams = default)
    {
        StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        playerClassController.GetRagdollByClass().Settings.AnimatingMode = RagdollHandler.EAnimatingMode.Sleep;
        // yield return new WaitForSeconds(3f);
        transform.position = SystemManager.Instance.CurrentSavePoint.GetRandomPositionAround();
        playerClassController.GetRagdollByClass().Settings.AnimatingMode = RagdollHandler.EAnimatingMode.Standing;
        yield return null;
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
        if (currentLevel == MaxLevel) return;
        currentExperience += experience;

        while (currentExperience > maxExperience)
        {
            currentExperience -= maxExperience;
            currentLevel += 1;
            maxExperience += experienceIncreaseAmount;
            PlayerEffectPooler.Instance.ShowLevelUpEffect(transform.position, Quaternion.identity, 3f);
            LevelUp();
            if (MaxLevelCheck()) return;
        }
        InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerLevel(currentLevel);
        InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerExperience(currentExperience, maxExperience);
    }

    private bool MaxLevelCheck()
    {
        bool isMaxLevel = currentLevel == MaxLevel;
        if (isMaxLevel) {
            GetComponent<PlayerController>().isMaxLevel.Value = true;
            RequestMaxLevelRpc(NetworkObjectId);
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerLevel(MaxLevel);
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerExperience(0, maxExperience);
        }
        return isMaxLevel;
    }

    [Rpc(SendTo.Server)]
    private void RequestMaxLevelRpc(ulong targetNetworkObjectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(targetNetworkObjectId, out var playerObj))
        {
            PlayerController playerController = playerObj.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isMaxLevel.Value = true;
            }
        }
    }
}
