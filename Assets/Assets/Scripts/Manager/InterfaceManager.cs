using Unity.Netcode;
using UnityEngine;

public class InterfaceManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            NetworkManager.Singleton.StartHost();
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}
