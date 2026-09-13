using UnityEngine;
using UnityEngine.UI;

public class GameScreen : MonoBehaviour
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

    void OnJumpButtonClicked()
    {
        if (playerController != null)
            playerController.Jump();
    }
}
