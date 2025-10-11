using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerEffectPooler : MonoBehaviour
{
    [SerializeField] private GameObject onHitEffect;
    [SerializeField] private float hitEffectPool;
    [SerializeField] private GameObject onDeathEffect;
    [SerializeField] private float deathEffectPool;

    private List<GameObject> onHitEffects = new();
    private List<GameObject> onDeathEffects = new();

    public static PlayerEffectPooler Instance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        GameObject instance;
        for (int i = 0; i < hitEffectPool; i++)
        {
            instance = Instantiate(onHitEffect, transform);
            instance.SetActive(false);
            onHitEffects.Add(instance);
        }
        for (int i = 0; i < deathEffectPool; i++)
        {
            instance = Instantiate(onDeathEffect, transform);
            instance.SetActive(false);
            onDeathEffects.Add(instance);
        }
    }

    public void ShowHitEffect(Vector3 position, Quaternion rotation, float duration = 3f)
    {
        for (int i = 0; i < onHitEffects.Count; i++)
        {
            if (onHitEffects[i].activeSelf) continue;
            onHitEffects[i].SetActive(true);
            onHitEffects[i].transform.position = position;
            onHitEffects[i].transform.rotation = rotation;
            StartCoroutine(HideEffect(onHitEffects[i], duration));
            return;
        }
    }

    public void ShowDeathEffect(Vector3 position, Quaternion rotation, float duration = 3f)
    {
        for (int i = 0; i < onDeathEffects.Count; i++)
        {
            if (onDeathEffects[i].activeSelf) continue;
            onDeathEffects[i].SetActive(true);
            onDeathEffects[i].transform.position = position;
            onDeathEffects[i].transform.rotation = rotation;
            StartCoroutine(HideEffect(onDeathEffects[i], duration));
            return;
        }
    }

    private IEnumerator HideEffect(GameObject effect, float duration)
    {
        yield return new WaitForSeconds(duration);
        effect.SetActive(false);
    }
}
