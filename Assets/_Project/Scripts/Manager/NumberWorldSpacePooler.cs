using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NumberWorldSpacePooler : MonoBehaviour
{
    public GameObject numberTemplatePrefab;
    public int amountToPool;

    public static NumberWorldSpacePooler Instance;
    private List<GameObject> poolNumbers = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < amountToPool; i++)
        {
            GameObject obj = Instantiate(numberTemplatePrefab, transform);
            obj.SetActive(false);
            poolNumbers.Add(obj);
        }
    }

    public void ShowNumberInWorld(int number, Vector3 position, float duration = 3f, Color color = new())
    {
        for (int i = 0; i < poolNumbers.Count; i++)
        {
            if (poolNumbers[i].activeSelf) continue;
            poolNumbers[i].transform.position = position;
            poolNumbers[i].SetActive(true);
            poolNumbers[i].GetComponent<NumberText>().ChangeTextValue(number.ToString());
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
