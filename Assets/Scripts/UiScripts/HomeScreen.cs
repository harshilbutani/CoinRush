using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : UIBase
{
    [Header("Buttons")]
    [SerializeField] private Button startButton;

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

    void OnEnable()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
    }

    void OnDisable()
    {
        if (startButton != null)
            startButton.onClick.RemoveListener(OnStartButtonClicked);
    }

    void OnStartButtonClicked()
    {
        UIManager.Instance.ShowNextScreen(ScreenNames.GameScreen);
    }


}
