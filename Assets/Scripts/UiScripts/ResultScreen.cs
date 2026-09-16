using Fusion;
using TMPro;
using UnityEngine;

public class ResultScreen : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI scoreText;

    public override void OnAwake()
    {
        base.OnAwake();
    }

    public void ShowResults()
    {
        NetworkPlayerData localPlayer = null;
        NetworkPlayerData opponentPlayer = null;

        foreach (NetworkPlayerData player in FindObjectsOfType<NetworkPlayerData>())
        {
            if (player.Object != null && player.Object.HasInputAuthority)
                localPlayer = player;
            else if (opponentPlayer == null)
                opponentPlayer = player;
        }

        if (localPlayer == null || opponentPlayer == null)
            return;

        string result = localPlayer.CoinCount == opponentPlayer.CoinCount
            ? "Draw"
            : localPlayer.CoinCount > opponentPlayer.CoinCount ? "You Win" : "You Lose";

        if (resultText != null)
            resultText.text = result;

        if (scoreText != null)
            scoreText.text = "You: " + localPlayer.CoinCount + "    Opponent: " + opponentPlayer.CoinCount;
    }
}
