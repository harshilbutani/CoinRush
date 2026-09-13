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
        playerController.Jump();
    }
}
