using Unity.Netcode;
using UnityEngine;

public class PlayerAttributeController : NetworkBehaviour
{
    private int playerHP = 10;
    private int maxHP = 10;

    [Header("Fairy")]
    public int fairyMaxHP = 10;
    public int fairyMouseLeftSkillDamage = 10;

    public void ReceiveDamage()
    {
        ReceiveDamageRpc(OwnerClientId);
    }

    [Rpc(SendTo.Server)]
    private void ReceiveDamageRpc(ulong targetOwnerClientId)
    {
        var rpcParams = new RpcParams
        {
            Send = new RpcSendParams
            {
                Target = NetworkManager.Singleton.RpcTarget.Single(targetOwnerClientId, RpcTargetUse.Persistent)
            }
        };
        SendDamageClientRpc(rpcParams);
    }

    [Rpc(SendTo.SpecifiedInParams)]
    private void SendDamageClientRpc(RpcParams rpcParams = default)
    {
        Debug.Log("Damage Received!");
        playerHP -= 1;
        InterfaceManager.Instance.UpdatePlayerHP(playerHP, maxHP);
    }
}
