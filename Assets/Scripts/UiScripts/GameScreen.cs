using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : UIBase
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private NetworkGameTimer networkGameTimer;

    [Header("Buttons")]
    [SerializeField] private Button jumpButton;

    [Header("Text")]
    [SerializeField] private TMPro.TextMeshProUGUI timerText;

    [Header("Joystick")]
    public Joystick joystick;

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

        networkGameTimer?.SetLocalPlayerReady();
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
