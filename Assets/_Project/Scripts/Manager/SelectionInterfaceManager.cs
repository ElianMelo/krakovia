using UnityEngine;

public class SelectionInterfaceManager : MonoBehaviour
{
    [Header("Generate")]
    public GameObject lobbyName;
    public GameObject playersAmount;
    public GameObject create;

    [Header("Search")]
    public GameObject theList;
    public GameObject refresh;

    public void ShowGenerate()
    {
        SwitchGenerate(true);
        SwitchSearch(false);
    }

    public void ShowSearch()
    {
        SwitchGenerate(false);
        SwitchSearch(true);
    }

    private void SwitchGenerate(bool newValue)
    {
        lobbyName.SetActive(newValue);
        playersAmount.SetActive(newValue);
        create.SetActive(newValue);
    }

    private void SwitchSearch(bool newValue)
    {
        theList.SetActive(newValue);
        refresh.SetActive(newValue);
    }
}
