using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SystemManager : MonoBehaviour
{
    public TMP_InputField inputField;

    [SerializeField] private SavePoint currentSavePoint;

    public SavePoint CurrentSavePoint { get { return currentSavePoint; } set { currentSavePoint = value; } }

    public bool GameStarted { get; private set; }

    public static SystemManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SwapSavePoint(SavePoint savePoint)
    {
        currentSavePoint = savePoint;
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.LeftControl))
        {
            if(Cursor.lockState == CursorLockMode.None)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            } else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    private void StartGame()
    {
        GameStarted = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void StartHostLocal()
    {
        NetworkManager.Singleton.StartHost();
        StartGame();
    }

    public void StartClientLocal()
    {
        NetworkManager.Singleton.StartClient();
        StartGame();
    }

    public void StartHost()
    {
        using var _ = StartHostWithRelay(6);
        StartGame();
    }

    public void StartClient()
    {
        _ = StartClientWithRelay("asd");
        StartGame();
    }

    /// <summary>
    /// Creates a relay server allocation and start a host
    /// </summary>
    /// <param name="maxConnections">The maximum amount of clients that can connect to the relay</param>
    /// <returns>The join code</returns>
    public async Task<string> StartHostWithRelay(int maxConnections = 21)
    {
        //Initialize the Unity Services engine
        await UnityServices.InitializeAsync();
        //Always authenticate your users beforehand
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Request allocation and join code
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        inputField.text = joinCode;
        // Configure transport
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(allocation, "dtls"));
        // Start host
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }

    /// <summary>
    /// Join a Relay server based on the JoinCode received from the Host or Server
    /// </summary>
    /// <param name="joinCode">The join code generated on the host or server</param>
    /// <returns>True if the connection was successful</returns>
    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        joinCode = inputField.text;
        //Initialize the Unity Services engine
        await UnityServices.InitializeAsync();
        //Always authenticate your users beforehand
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            //If not already logged, log the user in
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // Join allocation
        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        // Configure transport
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(AllocationUtils.ToRelayServerData(joinAllocation, "dtls"));
        // Start client
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }
}
