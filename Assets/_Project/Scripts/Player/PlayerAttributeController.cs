using Unity.Netcode;
using UnityEngine;

public class PlayerAttributeController : NetworkBehaviour
{
    private int currentHP = 50;
    private int maxHP = 50;
    private int currentExperience = 0;
    private int currentLevel = 1;
    private int maxExperience = 100;

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

    public void ReceiveDamage(int damage)
    {
        if (!IsOwner) return;
        ReceiveDamageRpc(OwnerClientId, damage);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong targetOwnerClientId, int damage)
    {
        var rpcParams = new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Single(targetOwnerClientId, RpcTargetUse.Persistent)
            }
        };
        SendDamageClientRpc(damage, rpcParams);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendDamageClientRpc(int damage, RpcParams rpcParams = default)
    {
        currentHP -= damage;
        NumberWorldSpacePooler.Instance.ShowNumberInWorld(damage, transform.position + new Vector3(0f,1f,0f));
        InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(currentHP, maxHP);
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.K))
        {
            currentHP = maxHP;
            InterfaceManager.Instance.PlayerInterfaceController.UpdatePlayerHp(currentHP, maxHP);
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
