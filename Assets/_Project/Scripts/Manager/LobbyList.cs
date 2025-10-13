using System.Collections.Generic;
using UnityEngine;

public class LobbyList : MonoBehaviour
{
    public GameObject lobbyPrefab;
    private List<GameObject> lobbies = new();

    private GameObject currentLobby;

    public void Clear()
    {
        for (int i = 0; i < lobbies.Count; i++)
        {
            Destroy(lobbies[i]);
        }
        lobbies.Clear();
    }

    public void AddLobby(string lobbyName, string lobbyAmount, string lobbyId)
    {
        currentLobby = Instantiate(lobbyPrefab, transform);
        LobbyItem lobbyItem = currentLobby.GetComponent<LobbyItem>();
        lobbyItem.SetupLobby(lobbyName, lobbyAmount, lobbyId);
        lobbies.Add(currentLobby);
    }
}