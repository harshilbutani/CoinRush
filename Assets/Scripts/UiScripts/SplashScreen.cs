using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SplashScreen : UIBase
{
    [SerializeField] private float completionSeconds = 3f;

    [Header("Slider")]
    [SerializeField] private Slider progressSlider;

    public override void OnAwake()
    {
        base.OnAwake();

        progressSlider.value = 0f;
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

        UIManager.Instance.ShowNextScreen(ScreenNames.HomeScreen);
    }
}