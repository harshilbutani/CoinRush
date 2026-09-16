using System.Collections;
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
    public const int RoomCodeLength = 6;

    public event Action<string> OpponentPlayerNameReceived;
    public event Action PlayersJoined;
    public string opponentPlayerName { get; private set; }
    public bool playersReady { get; private set; }
    public bool IsRoomFull { get; private set; }

    [SerializeField] private NetworkSceneManagerDefault _sceneManager;
    private NetworkRunner _runner;

    [Header("Room Settings")]
    [SerializeField] private int maxPlayers = 2;
    public string roomCode;
    private int startRequestId;
    private bool isLeavingRoom;
    private Coroutine opponentNameCoroutine;

    public override void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        DonotDestroyOnLoad = true;
        base.Awake();

        if (_sceneManager == null)
        {
            _sceneManager = GetComponent<NetworkSceneManagerDefault>();
            if (_sceneManager == null)
                _sceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>();
        }
    }

    public async void CreateRoom(string roomCode)
    {
        isLeavingRoom = false;
        await StartRunner(GameMode.Host, roomCode);
    }

    public async void JoinRoom(string roomCode)
    {
        isLeavingRoom = false;
        await StartRunner(GameMode.Client, roomCode);
    }

    private async Task StartRunner(GameMode mode, string roomCode)
    {
        int requestId = ++startRequestId;

        if (this == null)
            return;

        if (_sceneManager == null)
            return;

        await DestroyCurrentRunner();

        if (this == null || isLeavingRoom || requestId != startRequestId)
            return;

        _runner = CreateRunner();

        var scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex);
        var sceneInfo = new NetworkSceneInfo();
        if (scene.IsValid) sceneInfo.AddSceneRef(scene, LoadSceneMode.Additive);

        var result = await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = roomCode,
            PlayerCount = maxPlayers,
            Scene = scene,
            SceneManager = _sceneManager
        });

        if (this == null || isLeavingRoom || requestId != startRequestId)
            return;

        if (result.Ok)
        {
            Debug.Log($"Success! Mode: {mode}, Room: {roomCode}");

            if (mode == GameMode.Host)
            {
                UIManager.Instance.loader.HideScreen();
                UIManager.Instance.ShowNextScreen(ScreenNames.MatchMakingScreen);
            }
        }
        else
        {
            Debug.LogError($"Failed to start: {result.ShutdownReason}");
            UIManager.Instance.loader.HideScreen();
            UIManager.Instance.ShowNextScreen(ScreenNames.HomeScreen);
        }
    }

    private NetworkRunner CreateRunner()
    {
        GameObject runnerObject = new GameObject("NetworkRunner");

        NetworkRunner runner = runnerObject.AddComponent<NetworkRunner>();
        runnerObject.hideFlags = HideFlags.DontSave;
        runner.ProvideInput = true;
        runner.AddCallbacks(this);
        return runner;
    }

    private async Task DestroyCurrentRunner()
    {
        if (_runner == null)
            return;

        NetworkRunner runner = _runner;
        _runner = null;

        if (runner.IsRunning)
            await runner.Shutdown(destroyGameObject: false);

        if (runner != null)
            Destroy(runner.gameObject);
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
            SpawnPlayer(runner, player);

        if (player == runner.LocalPlayer && UIManager.Instance.currentScreen != ScreenNames.MatchMakingScreen)
        {
            UIManager.Instance.loader.HideScreen();
            UIManager.Instance.ShowNextScreen(ScreenNames.MatchMakingScreen);
            TryStartMatchTimer();
        }
        if (runner.ActivePlayers.Count() == maxPlayers)
        {
            if (IsRoomFull)
                return;

            IsRoomFull = true;
            PlayersJoined?.Invoke();
            Debug.Log($"<color=green>Room is full! Starting game for room: {runner.SessionInfo.Name}</color>");

            StartWaitingForOpponentName();

            if (runner.IsServer)
                SendPlayersReady(runner);
        }

    }

    private void SendPlayersReady(NetworkRunner runner)
    {
        NetworkObject playerObject = runner.GetPlayerObject(runner.LocalPlayer);
        NetworkPlayerData playerData = playerObject != null
            ? playerObject.GetComponent<NetworkPlayerData>()
            : null;

        if (playerData != null)
            playerData.SendPlayersReady();
    }

    public void ReceivePlayersReady()
    {
        playersReady = true;
        TryStartMatchTimer();
    }

    public async void LeaveRoom()
    {
        isLeavingRoom = true;
        startRequestId++;

        UIManager.Instance?.loader?.ShowScreen();

        if (opponentNameCoroutine != null)
        {
            StopCoroutine(opponentNameCoroutine);
            opponentNameCoroutine = null;
        }

        await DestroyCurrentRunner();

        IsRoomFull = false;
        playersReady = false;
        opponentPlayerName = string.Empty;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(
            SceneManager.GetActiveScene().buildIndex,
            LoadSceneMode.Single);

        while (!loadOperation.isDone)
            await Task.Yield();
    }

    private void TryStartMatchTimer()
    {
        if (playersReady && UIManager.Instance.currentScreen == ScreenNames.MatchMakingScreen)
            UIManager.Instance.GetScreen<MatchMakingScreen>()?.StartTimer();
    }

    private void SpawnPlayer(NetworkRunner runner, PlayerRef player)
    {
        if (GameManager.Instance == null || GameManager.Instance.playerPrefab == null)
        {
            Debug.LogError("Player prefab is not assigned on GameManager.");
            return;
        }

        NetworkObject playerObject = runner.Spawn(
            GameManager.Instance.playerPrefab,
            Vector3.zero,
            Quaternion.identity,
            player);

        NetworkPlayerData playerData = playerObject.GetComponent<NetworkPlayerData>();
        playerData?.SetSpawnPointIndex(GetSpawnIndex(runner, player));
        runner.SetPlayerObject(player, playerObject);
    }

    private int GetSpawnIndex(NetworkRunner runner, PlayerRef player)
    {
        List<PlayerRef> activePlayers = runner.ActivePlayers
            .OrderBy(activePlayer => activePlayer.PlayerId)
            .ToList();

        return Mathf.Max(0, activePlayers.IndexOf(player));
    }

    public bool TryGetOpponentPlayerName(out string playerName)
    {
        playerName = opponentPlayerName;

        if (!string.IsNullOrEmpty(playerName))
            return true;

        if (_runner == null || !_runner.IsRunning)
            return false;

        foreach (PlayerRef player in _runner.ActivePlayers)
        {
            if (player == _runner.LocalPlayer)
                continue;

            NetworkObject playerObject = _runner.GetPlayerObject(player);
            NetworkPlayerData playerData = playerObject != null
                ? playerObject.GetComponent<NetworkPlayerData>()
                : null;

            if (playerData == null || playerData.PlayerName.ToString().Length == 0)
                continue;

            playerName = playerData.PlayerName.ToString();
            opponentPlayerName = playerName;
            return true;
        }

        return false;
    }

    private void StartWaitingForOpponentName()
    {
        if (opponentNameCoroutine != null)
            StopCoroutine(opponentNameCoroutine);

        opponentNameCoroutine = StartCoroutine(WaitForOpponentName());
    }

    private IEnumerator WaitForOpponentName()
    {
        while (string.IsNullOrEmpty(opponentPlayerName))
        {
            if (TryGetOpponentPlayerName(out string playerName))
            {
                opponentPlayerName = playerName;
                OpponentPlayerNameReceived?.Invoke(playerName);
                Debug.Log("Opponent name synchronized: " + playerName);
                opponentNameCoroutine = null;
                yield break;
            }

            yield return null;
        }

        opponentNameCoroutine = null;
    }

    public void ReceivePlayerName(PlayerRef player, string playerName)
    {
        if (_runner != null && player == _runner.LocalPlayer)
            return;

        opponentPlayerName = playerName;
        OpponentPlayerNameReceived?.Invoke(playerName);
    }


    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log("Player left: " + player);
    }

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
    {
        Debug.Log("Shutdown: " + shutdownReason);
    }

    public string GenerateRoomCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        var code = new char[RoomCodeLength];
        for (int i = 0; i < RoomCodeLength; i++)
            code[i] = chars[UnityEngine.Random.Range(0, chars.Length)];
        this.roomCode = new string(code);
        Debug.Log($"<color=green>Generated Room Code: {this.roomCode}</color>");
        return this.roomCode;
    }

    public void OnConnectedToServer(NetworkRunner runner)
    {
    }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason)
    {
        UIManager.Instance.loader.HideScreen();
        UIManager.Instance.ShowNextScreen(ScreenNames.HomeScreen);
        Debug.LogError($"Could not connect to room: {reason}");
    }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        PlayerController localPlayer = PlayerController.LocalPlayer;

        if (localPlayer == null)
        {
            PlayerController[] players = FindObjectsOfType<PlayerController>();
            foreach (PlayerController player in players)
            {
                if (player.HasInputAuthority)
                {
                    localPlayer = player;
                    break;
                }
            }
        }

        if (localPlayer != null)
            input.Set(localPlayer.ReadInput());
    }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}