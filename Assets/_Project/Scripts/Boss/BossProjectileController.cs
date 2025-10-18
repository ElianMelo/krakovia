using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public class BossProjectileData
{
    public bool hasXRandomRange;
    public float xRange;
    public bool hasYRandomRange;
    public float yRange;
    public bool hasZRandomRange;
    public float zRange;
    public float damage;
    public Transform soucePosition;
    public float duration;
    public ProjectileType projectileType;
    public int amount;
}

public class BossProjectileController : MonoBehaviour
{
    public BossProjectileData atk1Data;
    public BossProjectileData atk2Data;
    public BossProjectileData atk3Data;

    public void PerformAttackOne()
    {
        PerformAttack(atk1Data);
    }

    public void PerformAttackTwo()
    {
        PerformAttack(atk2Data);
    }

    public void PerformAttackThree()
    {
        PerformAttack(atk3Data);
    }

    private void PerformAttack(BossProjectileData currentAtkData)
    {
        for (int i = 0; i < currentAtkData.amount; i++)
        {
            var xOffset = 0f; var yOffset = 0f; var zOffset = 0f;
            if (currentAtkData.hasXRandomRange)
                xOffset = UnityEngine.Random.Range(-currentAtkData.xRange, currentAtkData.xRange);
            if (currentAtkData.hasYRandomRange)
                yOffset = UnityEngine.Random.Range(-currentAtkData.yRange, currentAtkData.yRange);
            if (currentAtkData.hasZRandomRange)
                zOffset = UnityEngine.Random.Range(-currentAtkData.zRange, currentAtkData.zRange);
            Vector3 offset = new Vector3(xOffset, yOffset, zOffset);
            NetworkObject currentObject = BossProjectilePool.Instance.SpawnProjectile(currentAtkData.projectileType, currentAtkData.soucePosition.position + offset, currentAtkData.soucePosition.rotation);
            BossProjectile currentProjectile = currentObject.GetComponent<BossProjectile>();
            currentProjectile.SetupData(currentAtkData);
            StartCoroutine(DespawnProjectile(currentObject, currentAtkData.duration));
        }
    }

    private IEnumerator DespawnProjectile(NetworkObject currentProjectile, float duration)
    {
        yield return new WaitForSeconds(duration);
        BossProjectilePool.Instance.DespawnProjectile(currentProjectile);
    }
}
