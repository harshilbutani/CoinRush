using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ProfileScreen : UIBase
{
    [Header("Input Field")]
    [SerializeField] private TMP_InputField playerNameInput;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI errorText;

    [Header("Buttons")]
    [SerializeField] private Button okButton;

    public override void ShowScreen()
    {
        base.ShowScreen();

        if (playerNameInput != null)
        {
            playerNameInput.text = string.Empty;
        }

        errorText.text = "";
    }

    private void OnEnable()
    {
        if (okButton != null)
            okButton.onClick.AddListener(OnOkButtonClicked);
    }

    private void OnDisable()
    {
        if (okButton != null)
            okButton.onClick.RemoveListener(OnOkButtonClicked);
    }

    private void OnOkButtonClicked()
    {
        if (playerNameInput == null || string.IsNullOrWhiteSpace(playerNameInput.text))
        {
            errorText.text = "Please enter a player name.";
            return;
        }

        if (PlayerDataManager.Instance == null)
            return;

        PlayerDataManager.Instance.SavePlayerName(playerNameInput.text);
        UIManager.Instance.ShowNextScreen(ScreenNames.HomeScreen);
    }

}
