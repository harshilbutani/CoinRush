using Fusion;
using UnityEngine;
using UnityEngine.UI;

public class GameScreen : UIBase
{
    [SerializeField] private PlayerController playerController;

    [Header("Buttons")]
    [SerializeField] private Button jumpButton;

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
}
