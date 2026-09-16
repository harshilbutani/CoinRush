using Fusion;
using UnityEngine;

public class CoinManager : NetworkBehaviour
{
    public static CoinManager Instance { get; private set; }

    [SerializeField] private Coin sceneCoin;
    [SerializeField] private Transform[] coinSpawnPoints;

    [Networked]
    private int ActiveSpawnIndex { get; set; } = -1;

    [Networked]
    private NetworkBool CoinVisible { get; set; }

    private int lastSpawnIndex = -1;

    public override void Spawned()
    {
        Instance = this;
        ApplyCoinState();
    }

    public override void Render()
    {
        ApplyCoinState();
    }

    public void StartCoinRound()
    {
        if (!Object.HasStateAuthority || CoinVisible)
            return;

        SpawnCoinAtRandomIndex();
    }

    public void CollectCoin(NetworkPlayerData playerData)
    {
        if (!Object.HasStateAuthority || !CoinVisible || playerData == null)
            return;

        playerData.AddCoin();
        SpawnCoinAtRandomIndex();
    }

    public void StopCoinRound()
    {
        if (!Object.HasStateAuthority)
            return;

        CoinVisible = false;
    }

    private void SpawnCoinAtRandomIndex()
    {
        if (!Object.HasStateAuthority)
            return;

        if (coinSpawnPoints == null || coinSpawnPoints.Length == 0)
        {
            Debug.LogError("CoinManager: No coin spawn points are assigned.");
            return;
        }

        ActiveSpawnIndex = GetNextSpawnIndex();
        lastSpawnIndex = ActiveSpawnIndex;
        CoinVisible = true;
        ApplyCoinState();
    }

    private void ApplyCoinState()
    {
        if (sceneCoin == null || coinSpawnPoints == null || coinSpawnPoints.Length == 0)
            return;

        bool validIndex = ActiveSpawnIndex >= 0 && ActiveSpawnIndex < coinSpawnPoints.Length;
        if (validIndex)
        {
            sceneCoin.transform.position = coinSpawnPoints[ActiveSpawnIndex].position;
            sceneCoin.transform.rotation = coinSpawnPoints[ActiveSpawnIndex].rotation;
        }

        sceneCoin.SetVisible(CoinVisible && validIndex);
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
