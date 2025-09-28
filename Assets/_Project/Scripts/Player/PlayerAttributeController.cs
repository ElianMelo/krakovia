using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAttributeController : NetworkBehaviour
{
    private int currentExperience = 0;
    private int currentLevel = 1;
    private int maxExperience = 100;

    public NetworkVariable<int> CurrentHP = new NetworkVariable<int>();

    public GameObject healthBar;
    public Image healthBarImage;

    [Header("Experience")]
    public int experienceIncreaseAmount = 30;

    [Header("Fairy")]
    public int fairyMaxHP = 10;
    public int fairyMouseLeftSkillDamage = 10;
    public int fairyMouseRightSkillDamage = 10;
    public int fairyQSkillDamage = 10;
    public int fairyFSkillDamage = 10;

    [Header("Skeleton")]
    public int skeletonMaxHP = 10;
    public int skeletonMouseLeftSkillDamage = 10;
    public int skeletonMouseRightSkillDamage = 10;
    public int skeletonQSkillDamage = 10;
    public int skeletonFSkillDamage = 10;

    [Header("Horse")]
    public int horseMaxHP = 10;
    public int horseMouseLeftSkillDamage = 10;
    public int horseMouseRightSkillDamage = 10;
    public int horseQSkillDamage = 10;
    public int horseFSkillDamage = 10;

    public void SwitchHealthBar(bool target)
    {
        healthBar.SetActive(target);
    }

    public void OnHealthValueChanged(int previous, int current)
    {
        NumberWorldSpacePooler.Instance.ShowNumberInWorld(current - previous, transform.position + new Vector3(0f, 1f, 0f));
        healthBarImage.fillAmount = (float) current / skeletonMaxHP;
        if(IsOwner)
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(CurrentHP.Value, skeletonMaxHP);
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

        // CurrentHP
        //SendDamageClientRpc(damage, rpcParams);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SendDamageClientRpc(int damage, RpcParams rpcParams = default)
    {
        //currentHP -= damage;
        NumberWorldSpacePooler.Instance.ShowNumberInWorld(damage, transform.position + new Vector3(0f,1f,0f));
        InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(CurrentHP.Value, skeletonMaxHP);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.K) && IsServer)
        {
            //CurrentHP.Value = skeletonMaxHP;
            //InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(CurrentHP.Value, skeletonMaxHP);
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
                playerAttributeController.CurrentHP.Value = skeletonMaxHP;
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
        Debug.Log("Receive EXP!");
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
