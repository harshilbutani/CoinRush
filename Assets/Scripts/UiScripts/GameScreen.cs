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

    void OnEnable()
    {
        if (jumpButton != null)
            jumpButton.onClick.AddListener(OnJumpButtonClicked);
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
        if (playerController != null)
        {
            playerController.Jump();
            return;
        }

        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach (PlayerController player in players)
        {
            NetworkObject networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null && networkObject.HasInputAuthority)
            {
                player.Jump();
                return;
            }
        }
    }

    public void UpdateTimer(float remainingSeconds)
    {
        if (timerText != null)
            timerText.text = "Remaining Time: " + Mathf.Ceil(remainingSeconds) + "s";
    }
}
