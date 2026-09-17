using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SplashScreen : UIBase
{
    [SerializeField] private float completionSeconds = 3f;

    [Header("Slider")]
    [SerializeField] private Slider progressSlider;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI gameVersionText;

    public override void OnAwake()
    {
        base.OnAwake();

        progressSlider.value = 0f;
        SetGameVersion();
    }

    public override void ShowScreen()
    {
        base.ShowScreen();

        progressSlider.gameObject.SetActive(true);
        progressSlider.value = 0f;

        StartCoroutine(LoadingRoutine());
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }

    private IEnumerator LoadingRoutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < completionSeconds)
        {
            elapsedTime += Time.deltaTime;

            progressSlider.value = Mathf.Clamp01(
                elapsedTime / completionSeconds
            );

            yield return null;
        }

        progressSlider.value = 1f;

        Debug.Log("<color=yellow>Loading Complete!</color>");

        ScreenNames nextScreen = PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsFirstTime
            ? ScreenNames.Profile
            : ScreenNames.HomeScreen;

        UIManager.Instance.ShowNextScreen(nextScreen);
    }

    private void SetGameVersion()
    {
        if (gameVersionText != null)
            gameVersionText.text = "Version " + Application.version;
    }
}