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
