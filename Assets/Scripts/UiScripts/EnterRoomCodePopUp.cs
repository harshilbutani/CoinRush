using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class EnterRoomCodePopUp : UIBase
{
    [Header("Input Field")]
    [SerializeField] private TMP_InputField roomCodeInputField;

    [Header("Buttons")]
    [SerializeField] private Button joinButton;
    [SerializeField] private Button backButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI errorText;


    public override void OnAwake()
    {
        base.OnAwake();
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        errorText.text = "";
    }

    public override void HideScreen()
    {
        base.HideScreen();
        errorText.text = "";
    }

    void OnEnable()
    {
        if (joinButton != null)
            joinButton.onClick.AddListener(OnclickJoinButton);

        if (backButton != null)
            backButton.onClick.AddListener(OnclickBackButton);
    }

    void OnDisable()
    {
        if (joinButton != null)
            joinButton.onClick.RemoveListener(OnclickJoinButton);

        if (backButton != null)
            backButton.onClick.RemoveListener(OnclickBackButton);
    }

    private void OnclickJoinButton()
    {
        if (string.IsNullOrEmpty(roomCodeInputField.text))
        {
            errorText.text = "Please enter a room code.";
            return;
        }

        string roomCode = roomCodeInputField.text.Trim().ToUpperInvariant();
        RoomManager.Instance.roomCode = roomCode;
        UIManager.Instance.HidePopUp();
        UIManager.Instance.loader.ShowScreen();
        RoomManager.Instance.JoinRoom(roomCode);
    }

    private void OnclickBackButton()
    {
        UIManager.Instance.HidePopUp();
    }
}
