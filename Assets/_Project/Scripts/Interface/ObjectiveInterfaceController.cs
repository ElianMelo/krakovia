using UnityEngine;
using TMPro;

public class ObjectiveInterfaceController : MonoBehaviour
{
    public TMP_Text headerTextMesh;
    public TMP_Text contentTextMesh;

    [Header("Objectives to Level 5")]
    public string headerObjectiveToLevelFive;
    [TextArea] public string contentObjectiveToLevelFive;
    [Header("Objectives to Level 10")]
    public string headerObjectiveToLevelTen;
    [TextArea] public string contentObjectiveToLevelTen;
    [Header("Objectives to Level 15")]
    public string headerObjectiveToLevelFifteen;
    [TextArea] public string contentObjectiveToLevelFifteen;
    [Header("Objectives to Level 20")]
    public string headerObjectiveToLevelTwenty;
    [TextArea] public string contentObjectiveToLevelTwenty;
    [Header("Objectives after level 20")]
    public string headerObjectiveToLevelTwentyPos;
    [TextArea] public string contentObjectiveToLevelTwentyPos;

    void Start()
    {
        headerTextMesh.text = headerObjectiveToLevelFive;
        contentTextMesh.text = contentObjectiveToLevelFive;
    }

    public void ChangeObjectiveToLevelTen()
    {
        headerTextMesh.text = headerObjectiveToLevelTen;
        contentTextMesh.text = contentObjectiveToLevelTen;
    }

    public void ChangeObjectiveToLevelFifteen()
    {
        headerTextMesh.text = headerObjectiveToLevelFifteen;
        contentTextMesh.text = contentObjectiveToLevelFifteen;
    }

    public void ChangeObjectiveToLevelTwenty()
    {
        headerTextMesh.text = headerObjectiveToLevelTwenty;
        contentTextMesh.text = contentObjectiveToLevelTwenty;
    }

    public void ChangeObjectiveToLevelTwentyPos()
    {
        headerTextMesh.text = headerObjectiveToLevelTwentyPos;
        contentTextMesh.text = contentObjectiveToLevelTwentyPos;
    }
}
