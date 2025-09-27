using TMPro;
using UnityEngine;

public class NumberText : MonoBehaviour
{
    private TextMeshProUGUI textMesh;

    private void Awake()
    {
        textMesh = GetComponentInChildren<TextMeshProUGUI>();
    }

    public void ChangeTextValue(string text)
    {
        textMesh.text = text;
    }

    public void ChangeTextColor(Color color)
    {
        textMesh.color = color;
    }
}
