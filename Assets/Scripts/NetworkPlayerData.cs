using Fusion;

public class NetworkPlayerData : NetworkBehaviour
{
    [Networked]
    public NetworkString<_32> PlayerName { get; private set; }

    private string lastNotifiedPlayerName;

    public override void Spawned()
    {
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
        if (Object.HasInputAuthority || string.IsNullOrEmpty(PlayerName.ToString()))
            return;

        string currentPlayerName = PlayerName.ToString();
        if (currentPlayerName == lastNotifiedPlayerName)
            return;

        lastNotifiedPlayerName = currentPlayerName;
        RoomManager.Instance?.ReceivePlayerName(Object.InputAuthority, currentPlayerName);
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