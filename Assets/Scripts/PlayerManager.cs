using UnityEngine;

public class PlayerManager : Singleton<PlayerManager>
{
    public NetworkPlayerData MyPlayer { get; private set; }
    public NetworkPlayerData OpponentPlayer { get; private set; }

    public override void Awake()
    {
        base.Awake();
    }

    public void RegisterPlayer(NetworkPlayerData player)
    {
        if (player == null)
            return;

        if (player.Object != null && player.Object.HasInputAuthority)
        {
            MyPlayer = player;
            return;
        }

        if (MyPlayer != player && OpponentPlayer != player)
            OpponentPlayer = player;
    }

    public void UnregisterPlayer(NetworkPlayerData player)
    {
        if (player == null)
            return;

        if (MyPlayer == player)
            MyPlayer = null;

        if (OpponentPlayer == player)
            OpponentPlayer = null;
    }
}
