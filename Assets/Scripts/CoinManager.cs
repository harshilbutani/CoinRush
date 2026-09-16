using Fusion;
using UnityEngine;

public class CoinManager : NetworkBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private NetworkObject coinPrefab;
    [SerializeField] private Transform[] coinSpawnPoints;

    [Networked]
    private int ActiveSpawnIndex { get; set; } = -1;

    private NetworkObject activeCoin;
    private int lastSpawnIndex = -1;

    public override void Spawned()
    {
        Instance = this;
    }

    public void StartCoinRound()
    {
        if (!Object.HasStateAuthority || activeCoin != null)
            return;

        SpawnCoin();
    }

    public void CollectCoin(NetworkPlayerData playerData)
    {
        if (!Object.HasStateAuthority || activeCoin == null || playerData == null)
            return;

        playerData.AddCoin();
        NetworkObject collectedCoin = activeCoin;
        activeCoin = null;
        Runner.Despawn(collectedCoin);
        SpawnCoin();
    }

    public void StopCoinRound()
    {
        if (!Object.HasStateAuthority || activeCoin == null)
            return;

        Runner.Despawn(activeCoin);
        activeCoin = null;
    }

    private void SpawnCoin()
    {
        if (!Object.HasStateAuthority)
            return;

        if (coinPrefab == null)
        {
            Debug.LogError("CoinManager: Coin prefab is not assigned.");
            return;
        }

        if (coinSpawnPoints == null || coinSpawnPoints.Length == 0)
        {
            Debug.LogError("CoinManager: No coin spawn points are assigned.");
            return;
        }

        int spawnIndex = GetNextSpawnIndex();
        ActiveSpawnIndex = spawnIndex;
        Transform spawnPoint = coinSpawnPoints[spawnIndex];
        activeCoin = Runner.Spawn(coinPrefab, spawnPoint.position, spawnPoint.rotation);
        lastSpawnIndex = spawnIndex;
    }

    private int GetNextSpawnIndex()
    {
        if (coinSpawnPoints.Length == 1)
            return 0;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, coinSpawnPoints.Length);
        }
        while (randomIndex == lastSpawnIndex);

        return randomIndex;
    }
}
