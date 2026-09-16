using Fusion;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultScreen : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI resultText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [Header("Buttons")]
    [SerializeField] private Button homeButton;

    private void OnEnable()
    {
        if (homeButton != null)
            homeButton.onClick.AddListener(OnHomeButtonClicked);
    }

    private void OnDisable()
    {
        if (homeButton != null)
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
    }

    public override void OnAwake()
    {
        base.OnAwake();
    }

    private void OnHomeButtonClicked()
    {
        if (RoomManager.Instance != null)
        {
            RoomManager.Instance.LeaveRoom();
            return;
        }

        Debug.LogWarning("ResultScreen: RoomManager instance is not available.");
    }

    public void ShowOpponentDisconnectWin()
    {
        if (resultText != null)
            resultText.text = "You Win";

        if (scoreText != null)
            scoreText.text = "Opponent disconnected";
    }

    public void ShowResults()
    {
        NetworkPlayerData localPlayer = PlayerManager.Instance != null ? PlayerManager.Instance.MyPlayer : null;
        NetworkPlayerData opponentPlayer = PlayerManager.Instance != null ? PlayerManager.Instance.OpponentPlayer : null;

        if (localPlayer == null || opponentPlayer == null)
            return;

        string result;
        if (NetworkGameTimer.Instance != null && NetworkGameTimer.Instance.OpponentDisconnected)
        {
            result = "You Win";
        }
        else if (NetworkGameTimer.Instance != null && NetworkGameTimer.Instance.WinnerPlayer != PlayerRef.None)
        {
            result = localPlayer.Object.InputAuthority == NetworkGameTimer.Instance.WinnerPlayer
                ? "You Win"
                : "You Lose";
        }
        else
        {
            result = localPlayer.CoinCount == opponentPlayer.CoinCount
                ? "Draw"
                : localPlayer.CoinCount > opponentPlayer.CoinCount ? "You Win" : "You Lose";
        }

        if (resultText != null)
            resultText.text = result;

        if (scoreText != null)
        {
            scoreText.text = NetworkGameTimer.Instance != null && NetworkGameTimer.Instance.OpponentDisconnected
                ? "Opponent disconnected"
                : "You = " + localPlayer.CoinCount + "    " +
                  opponentPlayer.PlayerName + " = " + opponentPlayer.CoinCount;
        }
    }
}
