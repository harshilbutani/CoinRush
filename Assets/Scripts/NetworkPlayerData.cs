using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{
    [Networked]
    public NetworkString<_32> PlayerName { get; private set; }

    [Networked]
    public Vector2 NetworkPosition { get; private set; }

    [Networked]
    public int SpawnPointIndex { get; private set; } = -1;

    [Networked]
    public int CoinCount { get; private set; }

    [Header("Coin Display")]
    [SerializeField] private Transform coinDisplayParent;
    [SerializeField] private GameObject coinIconPrefab;

    private string lastNotifiedPlayerName;
    private readonly List<GameObject> coinIcons = new List<GameObject>();
    private int displayedCoinCount = -1;
    private int lastAppliedSpawnPointIndex = -1;
    private int lastScoreSentToUi = -1;
    private string lastNameSentToUi;

    public override void Spawned()
    {
        if (Object.HasStateAuthority)
            NetworkPosition = transform.position;

        GetComponent<PlayerController>()?.RegisterAsLocalPlayer();

        if (!Object.HasInputAuthority || PlayerDataManager.Instance == null)
            return;

        if (Object.HasStateAuthority)
        {
            PlayerName = PlayerDataManager.Instance.PlayerName;
            RPC_BroadcastPlayerName();
        }
        else
            RPC_SetPlayerName(PlayerDataManager.Instance.PlayerName);
    }

    public override void FixedUpdateNetwork()
    {
        PlayerController playerController = GetComponent<PlayerController>();

        if (Object.HasStateAuthority && playerController != null && GetInput<PlayerInputData>(out PlayerInputData input))
        {
            playerController.ApplyInput(input);
            NetworkPosition = transform.position;
        }

        if (Object.HasInputAuthority || string.IsNullOrEmpty(PlayerName.ToString()))
            return;

        string currentPlayerName = PlayerName.ToString();
        if (currentPlayerName == lastNotifiedPlayerName)
            return;

        lastNotifiedPlayerName = currentPlayerName;
        RoomManager.Instance?.ReceivePlayerName(Object.InputAuthority, currentPlayerName);
    }

    public override void Render()
    {
        if (!Object.HasStateAuthority && SpawnPointIndex != lastAppliedSpawnPointIndex)
        {
            ApplyLocalSpawnPoint();
            lastAppliedSpawnPointIndex = SpawnPointIndex;
        }
        else if (!Object.HasStateAuthority)
        {
            transform.position = NetworkPosition;
        }

        GetComponent<PlayerController>()?.UpdateNameLabel();
        UpdateCoinDisplay();

        string currentName = PlayerName.ToString();
        if (lastScoreSentToUi != CoinCount || lastNameSentToUi != currentName)
        {
            lastScoreSentToUi = CoinCount;
            lastNameSentToUi = currentName;
            UIManager.Instance.GetScreen<GameScreen>()?.UpdateScoreText();
        }
    }

    private void ApplyLocalSpawnPoint()
    {
        if (GameManager.Instance == null ||
            GameManager.Instance.playerSpawnPoints == null ||
            SpawnPointIndex < 0 ||
            SpawnPointIndex >= GameManager.Instance.playerSpawnPoints.Length)
            return;

        Transform spawnPoint = GameManager.Instance.playerSpawnPoints[SpawnPointIndex];
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
    }

    public void CapturePosition()
    {
        if (Object.HasStateAuthority)
            NetworkPosition = transform.position;
    }

    public void SetSpawnPointIndex(int spawnPointIndex)
    {
        if (!Object.HasStateAuthority)
            return;

        SpawnPointIndex = spawnPointIndex;

        if (GameManager.Instance != null &&
            GameManager.Instance.playerSpawnPoints != null &&
            spawnPointIndex >= 0 &&
            spawnPointIndex < GameManager.Instance.playerSpawnPoints.Length)
        {
            Transform spawnPoint = GameManager.Instance.playerSpawnPoints[spawnPointIndex];
            transform.position = spawnPoint.position;
            transform.rotation = spawnPoint.rotation;
            NetworkPosition = transform.position;
        }
    }

    public void AddCoin()
    {
        if (Object.HasStateAuthority)
            CoinCount++;
    }

    private void UpdateCoinDisplay()
    {
        if (coinDisplayParent == null || coinIconPrefab == null || displayedCoinCount == CoinCount)
            return;

        while (coinIcons.Count < CoinCount)
        {
            GameObject coinIcon = Instantiate(coinIconPrefab, coinDisplayParent);
            coinIcon.transform.localScale = Vector3.one;
            coinIcons.Add(coinIcon);
        }

        while (coinIcons.Count > CoinCount)
        {
            int lastIndex = coinIcons.Count - 1;
            Destroy(coinIcons[lastIndex]);
            coinIcons.RemoveAt(lastIndex);
        }

        displayedCoinCount = CoinCount;
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerName(string playerName)
    {
        PlayerName = playerName;
        RPC_BroadcastPlayerName();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_BroadcastPlayerName()
    {
        NotifyPlayerName();
    }

    private void NotifyPlayerName()
    {
        if (Object.HasInputAuthority || string.IsNullOrEmpty(PlayerName.ToString()))
            return;

        lastNotifiedPlayerName = PlayerName.ToString();
        RoomManager.Instance?.ReceivePlayerName(Object.InputAuthority, lastNotifiedPlayerName);
    }

    public void SendPlayersReady()
    {
        RPC_PlayersReady();
    }

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    private void RPC_PlayersReady()
    {
        RoomManager.Instance?.ReceivePlayersReady();
    }
}