using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Player Spawn Points")]
    public Transform[] playerSpawnPoints;

    [Header("Game Starts Timer Seconds")]
    public float gameStartTimerSeconds;

    [Header("Game Duration")]
    public float gameTimerSeconds = 60f;


    public override void Awake()
    {
        base.Awake();
    }
}
