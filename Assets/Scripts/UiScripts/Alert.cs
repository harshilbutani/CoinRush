using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum AlertButtonMode
{
    Ok = 0,
    YesNo = 1,
}

public enum AlertEnterDirection
{
    Left = 0,
    Right = 1,
    Up = 2,
}

public class Alert : UIBase
{
    public static Alert Instance { get; private set; }

    [Header("RectTransform")]
    [SerializeField] private RectTransform centerBoxTransform;

    [Header("Show Animation")]
    [SerializeField] private float enterAnimDuration = 0.35f;
    [SerializeField] private float enterOffset = 800f;
    [SerializeField] private Ease enterEase = Ease.OutBack;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI messageText;

    [Header("Buttons")]
    [SerializeField] private Button okButton;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    private Action onOk;
    private Action onYes;
    private Action onNo;
    private bool closeWithOk;
    private bool autoHide;
    private float autoHideSeconds;
    private AlertEnterDirection enterDirection = AlertEnterDirection.Left;
    private Coroutine autoHideRoutine;
    private Vector2 centerBoxRestPos;
    private bool restPosCached;
    private Tween enterTween;

    public override void OnAwake()
    {
        base.OnAwake();

        if (Instance != null && Instance != this)
        {
            gameObject.SetActive(false);
            return;
        }

        Instance = this;
        HideScreen();
    }

    private void OnEnable()
    {
        okButton?.onClick.AddListener(OnOkClicked);
        yesButton?.onClick.AddListener(OnYesClicked);
        noButton?.onClick.AddListener(OnNoClicked);
    }

    private void OnDisable()
    {
        okButton?.onClick.RemoveListener(OnOkClicked);
        yesButton?.onClick.RemoveListener(OnYesClicked);
        noButton?.onClick.RemoveListener(OnNoClicked);

        KillEnterAnim();
        StopAutoHide();
        ClearCallbacks();
    }

    public void Show(
        string message,
        AlertButtonMode buttonMode = AlertButtonMode.Ok,
        Action onOkCallback = null,
        Action onYesCallback = null,
        Action onNoCallback = null,
        bool closeWithOkButton = true,
        bool autoHideEnabled = false,
        float autoHideAfterSeconds = 0f,
        AlertEnterDirection? enterFrom = null)
    {
        Configure(message, buttonMode, onOkCallback, onYesCallback, onNoCallback,
            closeWithOkButton, autoHideEnabled, autoHideAfterSeconds, enterFrom);
        ShowScreen();
    }

    public void Configure(
        string message,
        AlertButtonMode buttonMode,
        Action onOkCallback,
        Action onYesCallback,
        Action onNoCallback,
        bool closeWithOkButton,
        bool autoHideEnabled,
        float autoHideAfterSeconds,
        AlertEnterDirection? enterFrom = null)
    {
        onOk = onOkCallback;
        onYes = onYesCallback;
        onNo = onNoCallback;
        closeWithOk = closeWithOkButton;
        autoHide = autoHideEnabled;
        autoHideSeconds = Mathf.Max(0f, autoHideAfterSeconds);
        enterDirection = enterFrom ?? AlertEnterDirection.Left;

        if (messageText != null)
            messageText.text = message ?? string.Empty;

        bool showOk = buttonMode == AlertButtonMode.Ok;
        bool showYesNo = buttonMode == AlertButtonMode.YesNo;

        okButton?.gameObject.SetActive(showOk && closeWithOk);
        yesButton?.gameObject.SetActive(showYesNo);
        noButton?.gameObject.SetActive(showYesNo);
    }

    public override void ShowScreen()
    {
        base.ShowScreen();
        PlayEnterAnimation();
        StopAutoHide();

        if (autoHide && autoHideSeconds > 0f)
            autoHideRoutine = StartCoroutine(AutoHideRoutine());
    }

    public override void HideScreen()
    {
        KillEnterAnim();
        RestoreCenterBoxPos();
        StopAutoHide();
        ClearCallbacks();
        base.HideScreen();
    }

    public void Close()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.HideAlert();
        else
            HideScreen();
    }

    private void PlayEnterAnimation()
    {
        if (centerBoxTransform == null)
            return;

        CacheRestPos();
        KillEnterAnim();

        Vector2 from = centerBoxRestPos;
        switch (enterDirection)
        {
            case AlertEnterDirection.Right:
                from.x += enterOffset;
                break;
            case AlertEnterDirection.Up:
                from.y += enterOffset;
                break;
            default:
                from.x -= enterOffset;
                break;
        }

        centerBoxTransform.anchoredPosition = from;
        enterTween = centerBoxTransform
            .DOAnchorPos(centerBoxRestPos, Mathf.Max(0.01f, enterAnimDuration))
            .SetEase(enterEase)
            .SetUpdate(true)
            .SetLink(centerBoxTransform.gameObject);
    }

    private void CacheRestPos()
    {
        if (restPosCached || centerBoxTransform == null)
            return;

        centerBoxRestPos = centerBoxTransform.anchoredPosition;
        restPosCached = true;
    }

    private void RestoreCenterBoxPos()
    {
        if (centerBoxTransform != null && restPosCached)
            centerBoxTransform.anchoredPosition = centerBoxRestPos;
    }

    private void KillEnterAnim()
    {
        if (enterTween != null && enterTween.IsActive())
            enterTween.Kill();

        enterTween = null;
        centerBoxTransform?.DOKill();
    }

    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSecondsRealtime(autoHideSeconds);
        autoHideRoutine = null;
        Close();
    }

    private void StopAutoHide()
    {
        if (autoHideRoutine == null)
            return;

        StopCoroutine(autoHideRoutine);
        autoHideRoutine = null;
    }

    private void ClearCallbacks()
    {
        onOk = null;
        onYes = null;
        onNo = null;
    }

    private void OnOkClicked()
    {
        Action callback = onOk;
        Close();
        callback?.Invoke();
    }

    private void OnYesClicked()
    {
        Action callback = onYes;
        Close();
        callback?.Invoke();
    }

    private void OnNoClicked()
    {
        Action callback = onNo;
        Close();
        callback?.Invoke();
    }
}