using Fusion;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class GameScreen : UIBase
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private NetworkGameTimer networkGameTimer;

    [Header("Buttons")]
    [SerializeField] private Button jumpButton;

    [Header("Text")]
    [SerializeField] private TMPro.TextMeshProUGUI timerText;
    [SerializeField] private TMPro.TextMeshProUGUI ownScoreText;
    [SerializeField] private TMPro.TextMeshProUGUI opponentScoreText;

    [Header("Joystick")]
    public Joystick joystick;

    private NetworkPlayerData localPlayerData;
    private NetworkPlayerData opponentPlayerData;
    private int displayedLocalScore = -1;
    private int displayedOpponentScore = -1;
    private string displayedLocalName;
    private string displayedOpponentName;

    void OnEnable()
    {
        if (jumpButton != null)
        {
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
            Debug.Log("[GameScreen] Jump button listener registered.");
        }
        else
        {
            Debug.LogError("[GameScreen] Jump button reference is missing.");
        }
    }

    void OnDisable()
    {
        if (jumpButton != null)
            jumpButton.onClick.RemoveListener(OnJumpButtonClicked);
    }

    public override void OnAwake()
    {
        base.OnAwake();
    }

    public override void ShowScreen()
    {
        base.ShowScreen();

        if (playerController == null)
        {
            playerController = PlayerController.LocalPlayer;

            if (playerController == null)
            {
                PlayerController[] players = FindObjectsOfType<PlayerController>();
                foreach (PlayerController player in players)
                {
                    if (player.HasInputAuthority)
                    {
                        playerController = player;
                        break;
                    }
                }
            }
        }

        if (networkGameTimer == null)
            networkGameTimer = FindObjectOfType<NetworkGameTimer>();

        CacheScorePlayers();
        displayedLocalScore = 0;
        displayedOpponentScore = 0;
        displayedLocalName = null;
        displayedOpponentName = null;
        UpdateScoreText();
        networkGameTimer?.SetLocalPlayerReady();
    }

    public void UpdateScoreText()
    {
        if (canvas == null || !canvas.enabled)
            return;

        int localScore = localPlayerData != null ? localPlayerData.CoinCount : 0;
        int opponentScore = opponentPlayerData != null ? opponentPlayerData.CoinCount : 0;
        string localName = localPlayerData != null && !string.IsNullOrEmpty(localPlayerData.PlayerName.ToString())
            ? localPlayerData.PlayerName.ToString()
            : "Me";
        string opponentName = opponentPlayerData != null && !string.IsNullOrEmpty(opponentPlayerData.PlayerName.ToString())
            ? opponentPlayerData.PlayerName.ToString()
            : "Opponent";

        if (localScore == displayedLocalScore &&
            opponentScore == displayedOpponentScore &&
            localName == displayedLocalName &&
            opponentName == displayedOpponentName)
            return;

        displayedLocalScore = localScore;
        displayedOpponentScore = opponentScore;
        displayedLocalName = localName;
        displayedOpponentName = opponentName;

        if (ownScoreText != null)
        {
            ownScoreText.text = localName + ": " + localScore;
        }

        if (opponentScoreText != null)
            opponentScoreText.text = opponentName + ": " + opponentScore;
    }

    private void CacheScorePlayers()
    {
        localPlayerData = null;
        opponentPlayerData = null;

        foreach (NetworkPlayerData player in FindObjectsOfType<NetworkPlayerData>())
        {
            if (player.Object != null && player.Object.HasInputAuthority)
                localPlayerData = player;
            else if (opponentPlayerData == null)
                opponentPlayerData = player;
        }
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }

    void OnJumpButtonClicked()
    {
        Debug.Log("[GameScreen] Jump button clicked.");

        if (playerController == null)
        {
            Debug.LogWarning("[GameScreen] Jump failed: local PlayerController was not found.");
            return;
        }

        Debug.Log($"[GameScreen] Sending jump request to {playerController.name}.");
        playerController.RequestJump();
    }

    public void UpdateTimer(float remainingSeconds)
    {
        if (timerText != null)
            timerText.text = "Remaining Time: " + Mathf.Ceil(remainingSeconds) + "s";
    }
}
