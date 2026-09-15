using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RoomManager : Singleton<RoomManager>, INetworkRunnerCallbacks
{
    [SerializeField] private NetworkRunner _runner;
    [SerializeField] private NetworkSceneManagerDefault _sceneManager;

    [Header("Room Settings")]
    [SerializeField] private int maxPlayers = 2;
    public string roomCode;

    public override void Awake()
    {
        base.Awake();

        if (_runner == null)
        {
            _runner = GetComponent<NetworkRunner>();
        }

        if (_sceneManager == null)
        {
            _sceneManager = GetComponent<NetworkSceneManagerDefault>();
        }

        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
    }

    public async void CreateRoom(string roomCode)
    {
        await StartRunner(GameMode.Host, roomCode);
    }

    public async void JoinRoom(string roomCode)
    {
        await StartRunner(GameMode.Client, roomCode);
    }

    private async Task StartRunner(GameMode mode, string roomCode)
    {
        UIManager.Instance.GetScreen<MatchMakingScreen>()?.SetStatus("Connecting...");

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomCode,
            PlayerCount = maxPlayers,
            Scene = scene,
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        if (result.Ok)
        {
            Debug.Log($"Success! Mode: {mode}, Room: {roomCode}");
            if (mode == GameMode.Client)
            {
                UIManager.Instance.loader.HideScreen();
                UIManager.Instance.ShowNextScreen(ScreenNames.MatchMakingScreen);
                if (_runner.ActivePlayers.Count() == maxPlayers)
                    UIManager.Instance.GetScreen<MatchMakingScreen>()?.StartTimer();
            }
        }
        else
        {
            Debug.LogError($"Failed to start: {result.ShutdownReason}");
            if (mode == GameMode.Client)
            {
                UIManager.Instance.loader.HideScreen();
                UIManager.Instance.ShowNextScreen(ScreenNames.HomeScreen);
            }
        }
    }

    // ---- The callbacks you actually care about ----
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        UIManager.Instance.GetScreen<MatchMakingScreen>()?.SetStatus("Waiting for opponent...");

        if (runner.ActivePlayers.Count() == maxPlayers)
        {
            Debug.Log($"<color=green>Room is full! Starting game for room: {runner.SessionInfo.Name}</color>");
            if (UIManager.Instance.currentScreen == ScreenNames.MatchMakingScreen)
                UIManager.Instance.GetScreen<MatchMakingScreen>()?.StartTimer();
        }

    }

    private Vector3 GetSpawnPosition(PlayerRef player)
    {
        Vector3 pos = player.PlayerId == 0 ? new Vector3(-3, 1, 0) : new Vector3(3, 1, 0);
        Debug.Log($"Spawning PlayerId {player.PlayerId} at {pos}");
        return pos;
    }


    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        UIManager.Instance.GetScreen<MatchMakingScreen>()?.SetStatus("Waiting for opponent...");
        Debug.Log("Player left: " + player);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Shutdown: " + shutdownReason);
    }

    public string GenerateRoomCode(int length = 6)
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[length];
        for (int i = 0; i < length; i++)
            code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        this.roomCode = new string(code);
        Debug.Log($"<color=green>Generated Room Code: {this.roomCode}</color>");
        return this.roomCode;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
        UIManager.Instance.GetScreen<MatchMakingScreen>()?.SetStatus("Connected. Waiting for opponent...");
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        UIManager.Instance.GetScreen<MatchMakingScreen>()?.SetStatus("Room not found");
        Debug.LogError($"Could not connect to room: {reason}");
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}