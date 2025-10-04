using System.Collections;
using TMPro;
using UnityEngine;

public class NumberText : MonoBehaviour
{
    [SerializeField] private Color baseColor;
    [SerializeField] private Color criticalColor;
    [SerializeField] private float baseSize;
    [SerializeField] private float criticalSize;

    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void SetupForCritical(bool isCritical)
    {
        if(isCritical)
        {
            ChangeTextColor(criticalColor);
            ChangeTextSize(criticalSize);
        } else
        {
            ChangeTextColor(baseColor);
            ChangeTextSize(baseSize);
        }
    }

    public void ChangeTextValue(string text)
    {
        textMesh.text = text;
    }

    public void ResetTranform()
    {
        textMesh.transform.localScale = Vector3.one;
        textMesh.alpha = 1f;
    }

    public void SmoothFadeOut(float duration)
    {
        ResetTranform();
        StartCoroutine(SmoothFadeOutCoroutine(duration));
    }

    private IEnumerator SmoothFadeOutCoroutine(float duration)
    {
        float currentDuration = 0;
        while(currentDuration < duration)
        {
            currentDuration += Time.deltaTime;
            float calculatedScale = 1f - (currentDuration / duration);
            textMesh.transform.localScale = new Vector3(calculatedScale, calculatedScale, calculatedScale);
            textMesh.alpha = 1f - (currentDuration / duration);
            yield return null;
        }
    }

    public void ChangeTextColor(Color color)
    {
        textMesh.color = color;
    }

    public void ChangeTextSize(float size)
    {
        textMesh.fontSize = size;
    }
}
