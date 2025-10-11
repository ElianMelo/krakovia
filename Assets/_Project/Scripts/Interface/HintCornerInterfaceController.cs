using UnityEngine;

public class HintCornerInterfaceController : MonoBehaviour
{
    public GameObject hiddenList;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Tab))
        {
            hiddenList.SetActive(!hiddenList.activeInHierarchy);
        }
    }
}
