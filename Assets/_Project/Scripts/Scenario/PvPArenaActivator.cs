using Unity.Netcode;
using UnityEngine;

public class PvPArenaActivator : NetworkBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if(other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            player.isPvPActive.Value = true;
        }                       
    }
}
