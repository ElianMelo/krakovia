using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberWorldSpacePooler : MonoBehaviour
{
    public GameObject numberTemplatePrefab;
    public int amountToPool;

    public static NumberWorldSpacePooler Instance;
    private List<GameObject> poolNumbers = new();

    private float offsetRange = 0.5f;
    private NumberText currentNumberText;
    private float xRandomOffset;
    private float yRandomOffset;
    private float zRandomOffset;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        GameObject obj;
        for (int i = 0; i < amountToPool; i++)
        {
            obj = Instantiate(numberTemplatePrefab, transform);
            obj.SetActive(false);
            poolNumbers.Add(obj);
        }
    }

    public void ShowNumberInWorld(int number, Vector3 position, bool isCritical, float duration = 3f, Color color = new())
    {
        for (int i = 0; i < poolNumbers.Count; i++)
        {
            if (poolNumbers[i].activeSelf) continue;
            xRandomOffset = Random.Range(-offsetRange, offsetRange);
            yRandomOffset = Random.Range(-offsetRange, offsetRange);
            zRandomOffset = Random.Range(-offsetRange, offsetRange);
            poolNumbers[i].transform.position = position + new Vector3(xRandomOffset, yRandomOffset, zRandomOffset);
            poolNumbers[i].SetActive(true);
            currentNumberText = poolNumbers[i].GetComponent<NumberText>();
            currentNumberText.SetupForCritical(isCritical);
            currentNumberText.ChangeTextValue(number.ToString());
            currentNumberText.SmoothFadeOut(duration);
            StartCoroutine(HidePoolNumber(poolNumbers[i], duration));
            return;
        }
    }

    private IEnumerator HidePoolNumber(GameObject numberObject, float duration)
    {
        yield return new WaitForSeconds(duration);
        numberObject.SetActive(false);
    }
}
