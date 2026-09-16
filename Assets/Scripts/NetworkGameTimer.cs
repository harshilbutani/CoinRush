using Fusion;
using UnityEngine;

public class NetworkGameTimer : NetworkBehaviour
{
    [Networked] public TickTimer GameTimer { get; set; }
    [Networked] public NetworkBool IsGameEnded { get; set; }
    [Networked] public NetworkBool Player0Ready { get; set; }
    [Networked] public NetworkBool Player1Ready { get; set; }
    [Networked] public NetworkBool TimerStarted { get; set; }
    [Networked] public PlayerRef WinnerPlayer { get; set; }
    [Networked] public NetworkBool OpponentDisconnected { get; set; }

    public static NetworkGameTimer Instance { get; private set; }
    private bool resultShown;
    private PlayerRef firstReadyPlayer;

    public override void Spawned()
    {
        Instance = this;
    }

    public void SetLocalPlayerReady()
    {
        if (!Object || !Object.IsValid)
        {
            Debug.LogWarning("NetworkGameTimer is not spawned, so Ready RPC was not sent.");
            return;
        }

        Debug.Log("Sending Ready RPC for Player " + Runner.LocalPlayer.PlayerId);
        RPC_SetPlayerReady(Runner.LocalPlayer);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    private void RPC_SetPlayerReady(PlayerRef player)
    {
        if (firstReadyPlayer == PlayerRef.None)
        {
            firstReadyPlayer = player;
            Player0Ready = true;
        }
        else if (player != firstReadyPlayer)
        {
            Player1Ready = true;
        }

        Debug.Log("Ready received from Player " + player.PlayerId + ". Player0Ready: " + Player0Ready + ", Player1Ready: " + Player1Ready);
    }

    public override void FixedUpdateNetwork()
    {
        if (!Object.HasStateAuthority)
            return;

        if (!TimerStarted && Player0Ready && Player1Ready)
        {
            GameTimer = TickTimer.CreateFromSeconds(Runner, GameManager.Instance.gameTimerSeconds);
            TimerStarted = true;
            CoinManager.Instance?.StartCoinRound();
            Debug.Log("<color=green>Network game timer started for " + GameManager.Instance.gameTimerSeconds + " seconds.</color>");
        }

        if (TimerStarted && !IsGameEnded && GameTimer.Expired(Runner))
        {
            IsGameEnded = true;
            WinnerPlayer = FindWinnerByScore();
            CoinManager.Instance?.StopCoinRound();
            Debug.Log("<color=yellow>Network game timer expired.</color>");
        }
    }

    public override void Render()
    {
        if (IsGameEnded)
        {
            if (!resultShown)
                ShowResultScreen();
            return;
        }

        if (!TimerStarted)
            return;

        float remaining = GameTimer.RemainingTime(Runner) ?? 0f;
        UIManager.Instance.GetScreen<GameScreen>()?.UpdateTimer(remaining);

    }

    private void ShowResultScreen()
    {
        resultShown = true;
        Debug.Log("<color=red>Game ended. Opening ResultScreen.</color>");
        UIManager.Instance.GetScreen<GameScreen>()?.UpdateTimer(0f);
        UIManager.Instance.ShowNextScreen(ScreenNames.ResultScreen);
        UIManager.Instance.GetScreen<ResultScreen>()?.ShowResults();
    }

    public void HandlePlayerLeft(PlayerRef leftPlayer)
    {
        if (!Object.HasStateAuthority || IsGameEnded)
            return;

        foreach (PlayerRef activePlayer in Runner.ActivePlayers)
        {
            if (activePlayer == leftPlayer)
                continue;

            WinnerPlayer = activePlayer;
            OpponentDisconnected = true;
            IsGameEnded = true;
            CoinManager.Instance?.StopCoinRound();
            Debug.Log($"Player {leftPlayer.PlayerId} left. Player {activePlayer.PlayerId} wins.");
            return;
        }
    }

    private PlayerRef FindWinnerByScore()
    {
        PlayerRef winner = PlayerRef.None;
        int highestScore = -1;

        foreach (PlayerRef player in Runner.ActivePlayers)
        {
            NetworkObject playerObject = Runner.GetPlayerObject(player);
            NetworkPlayerData playerData = playerObject?.GetComponent<NetworkPlayerData>();
            if (playerData == null || playerData.CoinCount <= highestScore)
                continue;

            highestScore = playerData.CoinCount;
            winner = player;
        }

        return winner;
    }
}