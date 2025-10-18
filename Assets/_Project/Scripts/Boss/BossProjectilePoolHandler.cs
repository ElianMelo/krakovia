using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class BossProjectilePoolHandler : INetworkPrefabInstanceHandler
{
    private readonly GameObject prefab;
    private readonly Queue<NetworkObject> pool;

    public BossProjectilePoolHandler(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        pool = new Queue<NetworkObject>();

        for (int i = 0; i < initialSize; i++)
        {
            var obj = Object.Instantiate(prefab).GetComponent<NetworkObject>();
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public NetworkObject Instantiate(ulong ownerClientId, Vector3 position, Quaternion rotation)
    {
        NetworkObject obj;
        if (pool.Count > 0)
        {
            obj = pool.Dequeue();
            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("SHOLD NOT INSTANTIATE");
            obj = Object.Instantiate(prefab, position, rotation).GetComponent<NetworkObject>();
        }

        return obj;
    }

    public void Destroy(NetworkObject networkObject)
    {
        networkObject.gameObject.SetActive(false);
        pool.Enqueue(networkObject);
    }
}
