using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    [Header("Player Prefab")]
    public GameObject playerPrefab;

    [Header("Game Starts Timer Seconds")]
    public float gameStartTimerSeconds;


    public override void Awake()
    {
        base.Awake();
    }
}
