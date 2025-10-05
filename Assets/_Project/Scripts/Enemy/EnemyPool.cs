using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public enum EnemyType
{
    Dog,
    Mummy,
    Yeti,
    EnemyA,
    EnemyB,
    EnemyC,
}

[System.Serializable]
public class PooledEnemyData
{
    public EnemyType type;
    public GameObject prefab;
    public int initialPoolSize = 10;
}

public class EnemyPool : MonoBehaviour
{
    public List<PooledEnemyData> enemyTypes = new List<PooledEnemyData>();

    private Dictionary<EnemyType, Queue<NetworkObject>> poolDictionary = new Dictionary<EnemyType, Queue<NetworkObject>>();

    void Awake()
    {
        foreach (var type in enemyTypes)
        {
            var queue = new Queue<NetworkObject>();
            for (int i = 0; i < type.initialPoolSize; i++)
            {
                var obj = Instantiate(type.prefab).GetComponent<NetworkObject>();
                obj.gameObject.SetActive(false);
                queue.Enqueue(obj);
            }
            poolDictionary[type.type] = queue;
        }
    }

    public NetworkObject GetFromPool(EnemyType type)
    {
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"No pool found for enemy type: {type}");
            return null;
        }

        var queue = poolDictionary[type];
        NetworkObject obj;

        if (queue.Count > 0)
        {
            obj = queue.Dequeue();
            obj.gameObject.SetActive(true);
        }
        else
        {
            var typeData = enemyTypes.Find(t => t.type == type);
            if (typeData != null)
            {
                obj = Instantiate(typeData.prefab).GetComponent<NetworkObject>();
            }
            else
            {
                Debug.LogError($"Could not find prefab for enemy type: {type}");
                return null;
            }
        }

        return obj;
    }

    public void ReturnToPool(NetworkObject obj, EnemyType type)
    {
        obj.gameObject.SetActive(false);
        if (!poolDictionary.ContainsKey(type))
        {
            Debug.LogError($"No pool to return object with type: {type}");
            Destroy(obj.gameObject);
            return;
        }

        poolDictionary[type].Enqueue(obj);
    }
}
