using Fusion;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{
    [Networked]
    public NetworkString<_32> PlayerName { get; private set; }

    [Networked]
    public Vector2 NetworkPosition { get; private set; }

    private string lastNotifiedPlayerName;

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
        if (!Object.HasStateAuthority)
            transform.position = NetworkPosition;

        GetComponent<PlayerController>()?.UpdateNameLabel();
    }

    public void CapturePosition()
    {
        if (Object.HasStateAuthority)
            NetworkPosition = transform.position;
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