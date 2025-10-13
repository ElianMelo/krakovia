using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyManager : MonoBehaviour
{
    public LobbyList lobbyList;
    public Button refreshList;

    public TMP_Text lobbyUIText;
    public TMP_Text maxPlayersUI;
    private bool canRefresh = true;

    public Button createLobby;

    public static LobbyManager Instance;

    private void Awake()
    {
        Instance = this;
        refreshList.onClick.AddListener(OnRefreshList);
        createLobby.onClick.AddListener(OnCreateLobby);
    }

    public void OnCreateLobby()
    {
        _ = CreateLobbyAsync();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    public void OnRefreshList()
    {
        if(canRefresh)
        {
            _ = QueryLobbies();
            canRefresh = false;
            refreshList.interactable = false;
            StartCoroutine(ResetCanRefresh());
        }
    }

    IEnumerator ResetCanRefresh()
    {
        yield return new WaitForSeconds(2f);
        refreshList.interactable = true;
        canRefresh = true;
    }

    IEnumerator HeartbeatLobbyCoroutine(string lobbyId, float waitTimeSeconds)
    {
        var delay = new WaitForSecondsRealtime(waitTimeSeconds);

        while (true)
        {
            LobbyService.Instance.SendHeartbeatPingAsync(lobbyId);
            yield return delay;
        }
    }

    public async Task JoinLobbyById(string lobbyId)
    {
        try
        {
            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            // Retrieve Relay Join Code from Lobby data
            string relayJoinCode = joinedLobby.Data["JoinCode"].Value;
            // Join allocation
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);
            // Configure transport
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
            // Start client
            NetworkManager.Singleton.StartClient();
            SystemManager.Instance.StartGame();
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async Task CreateLobbyAsync()
    {
        string lobbyName = lobbyUIText.text.Count() > 15 ? lobbyUIText.text.Substring(0, 14) + "." : lobbyUIText.text;
        int maxPlayers = 10;
        string raw = maxPlayersUI.GetParsedText();
        string numericOnly = Regex.Match(raw, @"\d+").Value; // Extracts first number it finds

        if (int.TryParse(numericOnly, out maxPlayers))
        {
            Debug.Log($"Parsed maxPlayers = {maxPlayers}");
        }
        else
        {
            maxPlayers = 10;
            Debug.Log($"Couldn't parse a number from: \"{raw}\"");
        }
        await CreateLobbyAndRelay(lobbyName, maxPlayers);
    }

    public async Task CreateLobbyAndRelay(string lobbyName, int maxPlayers)
    {
        // --- 1. Allocate Relay ---
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers);

        // --- 2. Apply Relay Data to Transport ---
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));

        // --- 3. Get Relay JoinCode ---
        string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

        // --- 4. Create Lobby and Store Relay Join Code ---
        CreateLobbyOptions options = new CreateLobbyOptions
        {
            IsPrivate = false,
            Data = new Dictionary<string, DataObject>
        {
            {
                "JoinCode", new DataObject(
                    visibility: DataObject.VisibilityOptions.Public,
                    value: relayJoinCode
                )
            }
        }
        };

        Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);

        Debug.Log($"Lobby created: {lobby.Id} | Relay Code: {relayJoinCode}");

        // --- 5. Start Hosting ---
        NetworkManager.Singleton.StartHost();
        SystemManager.Instance.StartGame();

        StartCoroutine(HeartbeatLobbyCoroutine(lobby.Id, 15));
    }

    private async Task QueryLobbies()
    {
        lobbyList.Clear();
        try
        {
            QueryLobbiesOptions options = new QueryLobbiesOptions();
            options.Count = 25;

            // Filter for open lobbies only
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0")
            };

            // Order by newest lobbies first
            options.Order = new List<QueryOrder>()
            {
                new QueryOrder(
                    asc: false,
                    field: QueryOrder.FieldOptions.Created)
            };

            QueryResponse lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (var theLobby in lobbies.Results)
            {
                lobbyList.AddLobby(theLobby.Name, theLobby.Players.Count + "/" + theLobby.MaxPlayers, theLobby.Id);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

}
