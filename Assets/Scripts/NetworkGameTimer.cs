using Fusion;
using UnityEngine;

public class NetworkGameTimer : NetworkBehaviour
{
    [Networked] public TickTimer GameTimer { get; set; }
    [Networked] public NetworkBool IsGameEnded { get; set; }
    [Networked] public NetworkBool Player0Ready { get; set; }
    [Networked] public NetworkBool Player1Ready { get; set; }
    [Networked] public NetworkBool TimerStarted { get; set; }

    private bool resultShown;
    private PlayerRef firstReadyPlayer;

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
            Debug.Log("<color=green>Network game timer started for " + GameManager.Instance.gameTimerSeconds + " seconds.</color>");
        }

        if (TimerStarted && !IsGameEnded && GameTimer.Expired(Runner))
        {
            IsGameEnded = true;
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
    }
}