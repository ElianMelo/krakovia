using UnityEngine;
using Unity.Services.Core;
using System;
using Unity.Services.Authentication;
using Unity.Services.Multiplayer;
using System.Threading.Tasks;
using Unity.Netcode;

public class SessionManager : MonoBehaviour
{
    public void Start()
    {
        InitiliazeSession();
    }

    public static SessionManager Instance;

    public void ConnectMainServer()
    {
        _ = QuerySessions();
    }

    public void CreateMainServer()
    {
        _ = StartSessionAsHost();
    }

    async void InitiliazeSession()
    {
        await Autenticate();

        string[] args = System.Environment.GetCommandLineArgs();

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "-dedicatedserver" && i + 1 < args.Length)
            {
                await StartSessionAsHost();
            }
        }
#if DEDICATED_SERVER
        await StartSessionAsHost();
#endif

        // await QuerySessions();
    }

    async Task Autenticate()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    async Task StartSessionAsHost()
    {
        // NetworkManager.Singleton.StartHost();
        var options = new SessionOptions
        {
            Name = "Main Server",
            MaxPlayers = 100
        }.WithRelayNetwork(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay
        var session = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {session.Id} created! Join code: {session.Code}");
    }

    async Task QuerySessions()
    {
        var queryOptions = new QuerySessionsOptions(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay
        QuerySessionsResults querySessionsResults = await MultiplayerService.Instance.QuerySessionsAsync(queryOptions);
        await MultiplayerService.Instance.JoinSessionByIdAsync(querySessionsResults.Sessions[0].Id);
        NetworkManager.Singleton.StartClient();
    }

}