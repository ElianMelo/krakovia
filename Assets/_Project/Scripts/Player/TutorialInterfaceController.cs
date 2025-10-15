using System.Collections.Generic;
using UnityEngine;

public class TutorialInterfaceController : MonoBehaviour
{
    public List<GameObject> pages = new();
    public GameObject visuals;

    private int currentPage = 0;

    public void NextPage()
    {
        SwitchCurrentPage(false);
        currentPage += 1;
        if(currentPage >= pages.Count)
            currentPage = 0;
        SwitchCurrentPage(true);
    }

    public void PreviousPage()
    {
        SwitchCurrentPage(false);
        currentPage -= 1;
        if (currentPage < 0)
            currentPage = pages.Count-1;
        SwitchCurrentPage(true);
    }

    private void SwitchCurrentPage(bool target)
    {
        pages[currentPage].SetActive(target);
    }

    public void Hide()
    {
        visuals.SetActive(false);
    }

    public void Show()
    {
        visuals.SetActive(true);
    }
}
