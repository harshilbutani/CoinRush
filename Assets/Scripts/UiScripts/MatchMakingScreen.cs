using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MatchMakingScreen : UIBase
{
    [Header("Text")]
    [SerializeField] private TextMeshProUGUI roomcodeText;
    [SerializeField] private TextMeshProUGUI myPlayerNameText;
    [SerializeField] private TextMeshProUGUI opponentPlayerNameText;
    [SerializeField] private TextMeshProUGUI timerText;

    private Coroutine timerCoroutine;

    public override void OnAwake()
    {
        base.OnAwake();
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        SetData();
        timerText.text = "";
    }

    public override void HideScreen()
    {
        base.HideScreen();

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private void SetData()
    {
        roomcodeText.text = "Room Code : " + RoomManager.Instance.roomCode.ToString();
        myPlayerNameText.text = PlayerDataManager.Instance.PlayerName.ToString();
        opponentPlayerNameText.text = "Waiting for opponent...";
        timerText.text = "Connecting...";
    }

    public void SetStatus(string status)
    {
        if (timerText != null)
            timerText.text = status;
    }

    public void StartTimer()
    {
        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);

        timerCoroutine = StartCoroutine(MatchTimerRoutine());
    }

    private IEnumerator MatchTimerRoutine()
    {
        float timer = GameManager.Instance.gameStartTimerSeconds;

        while (timer > 0f)
        {
            timer -= Time.deltaTime;

            if (timerText != null)
                timerText.text = "Game starts in " + Mathf.Ceil(timer) + "s";

            // Debug.Log("Game starts in " + Mathf.Ceil(timer) + "s");
            yield return null;
        }

        if (timerText != null)
            timerText.text = "Game starts in 0s";

        Debug.Log("<color=purple>Matchmaking time Over!</color>");
        UIManager.Instance.ShowNextScreen(ScreenNames.GameScreen);
        timerCoroutine = null;
    }
}
