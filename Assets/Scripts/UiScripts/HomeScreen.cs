using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeScreen : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI playerNameText;

    public override void OnAwake()
    {
        base.OnAwake();
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        SetPlayerNameText();
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }

    void OnEnable()
    {
        if (createRoomButton != null)
            createRoomButton.onClick.AddListener(OnCreateRoomButtonClicked);

        if (joinRoomButton != null)
            joinRoomButton.onClick.AddListener(OnJoinRoomButtonClicked);
    }

    void OnDisable()
    {
        if (createRoomButton != null)
            createRoomButton.onClick.RemoveListener(OnCreateRoomButtonClicked);

        if (joinRoomButton != null)
            joinRoomButton.onClick.RemoveListener(OnJoinRoomButtonClicked);
    }

    public void OnCreateRoomButtonClicked()
    {
        RoomManager.Instance.GenerateRoomCode();
        RoomManager.Instance.CreateRoom(RoomManager.Instance.roomCode);
        UIManager.Instance.ShowNextScreen(ScreenNames.MatchMakingScreen);
    }

    public void OnJoinRoomButtonClicked()
    {
        UIManager.Instance.ShowPopUp(PopUpNames.enterRoomCode);
    }

    private void SetPlayerNameText()
    {
        if (playerNameText == null || PlayerDataManager.Instance == null || string.IsNullOrEmpty(PlayerDataManager.Instance.PlayerName))
        {
            playerNameText.text = "";
            return;
        }
        playerNameText.text = "Name : " + PlayerDataManager.Instance.PlayerName.ToString();
    }
}

