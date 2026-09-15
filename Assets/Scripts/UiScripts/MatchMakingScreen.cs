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

    [Header("Buttons")]

    [SerializeField] private Button backButton;

    private Coroutine timerCoroutine;
    private bool isSubscribed;
    private bool timerStarted;

    public override void OnAwake()
    {
        base.OnAwake();
    }

    private void OnEnable()
    {
        SubscribeToRoomManager();

        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
    }

    private void OnDisable()
    {
        if (isSubscribed && RoomManager.Instance != null)
        {
            RoomManager.Instance.OpponentPlayerNameReceived -= SetOpponentPlayerName;
            RoomManager.Instance.PlayersJoined -= DisableBackButton;
            isSubscribed = false;
        }

        if (backButton != null)
            backButton.onClick.RemoveListener(OnBackButtonClicked);
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        SubscribeToRoomManager();
        SetData();
        timerText.text = "";
        SetBackButtonState();

        if (RoomManager.Instance != null &&
            RoomManager.Instance.TryGetOpponentPlayerName(out string opponentName))
        {
            SetOpponentPlayerName(opponentName);
        }
    }

    private void SubscribeToRoomManager()
    {
        if (isSubscribed || RoomManager.Instance == null)
            return;

        RoomManager.Instance.OpponentPlayerNameReceived += SetOpponentPlayerName;
        RoomManager.Instance.PlayersJoined += DisableBackButton;
        isSubscribed = true;
    }

    public override void HideScreen()
    {
        base.HideScreen();

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        timerStarted = false;
    }

    private void SetBackButtonState()
    {
        if (backButton != null)
            backButton.gameObject.SetActive(RoomManager.Instance == null || !RoomManager.Instance.IsRoomFull);
    }

    private void DisableBackButton()
    {
        if (backButton != null)
            backButton.gameObject.SetActive(false);
    }

    private void OnBackButtonClicked()
    {
        RoomManager.Instance?.LeaveRoom();
    }

    private void SetOpponentPlayerName(string opponentName)
    {
        if (opponentPlayerNameText != null)
            opponentPlayerNameText.text = opponentName;
    }

    private void SetData()
    {
        roomcodeText.text = "Room Code : " + RoomManager.Instance.roomCode.ToString();
        myPlayerNameText.text = PlayerDataManager.Instance.PlayerName.ToString();
        opponentPlayerNameText.text = "Waiting for opponent...";
    }

    public void SetStatus(string status)
    {
        if (timerText != null)
            timerText.text = status;
    }

    public void StartTimer()
    {
        if (timerStarted)
            return;

        timerStarted = true;

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

            yield return null;
        }

        if (timerText != null)
            timerText.text = "Game starts in 0s";

        Debug.Log("<color=purple>Matchmaking time Over!</color>");
        UIManager.Instance.ShowNextScreen(ScreenNames.GameScreen);
        timerCoroutine = null;
    }
}
