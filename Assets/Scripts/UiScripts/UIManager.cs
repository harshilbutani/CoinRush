using UnityEngine;
using System.Collections.Generic;



public enum ScreenNames
{
    Splash,
    Profile,
    HomeScreen,
    GameScreen,
    MatchMakingScreen

}

public enum PopUpNames
{
    enterRoomCode,
}

[System.Serializable]
public class ScreenType
{
    public ScreenNames screenName;
    public UIBase screenBase;
}

[System.Serializable]
public class PopUpType
{
    public PopUpNames popUpName;
    public UIBase popUpBase;
}

public class UIManager : Singleton<UIManager>
{
    #region PUBLIC_VARS
    [Header("Screen Canvas")]
    public List<ScreenType> screenTypes;
    [SerializeField] public LoaderScreen loader;
    public ScreenNames currentScreen;
    public ScreenNames previousScreen;
    public ScreenNames InitScreen;

    [Header("PopUp Canvas")]
    public List<PopUpType> popUpTypes;
    public PopUpNames currentPopUpScreen;
    [HideInInspector] public bool isPopUpOn;
    #endregion

    #region PRIVATE_VARS
    #endregion

    #region UNITY_CALLBACKS
    public override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        InitializeScreen();
    }
    #endregion

    #region STATIC_FUNCTIONS
    #endregion

    #region PUBLIC_FUNCTIONS
    public void ShowNextScreen(ScreenNames screen)
    {
        screenTypes.Find(a => a.screenName == currentScreen).screenBase.HideScreen();
        previousScreen = currentScreen;
        currentScreen = screen;
        screenTypes.Find(a => a.screenName == currentScreen).screenBase.ShowScreen();
    }

    public void ShowPopUp(PopUpNames popUpScreen)
    {
        currentPopUpScreen = popUpScreen;
        popUpTypes.Find(a => a.popUpName == popUpScreen).popUpBase.ShowScreen();
        isPopUpOn = true;
    }

    public void HidePopUp()
    {
        if (isPopUpOn)
        {
            popUpTypes.Find(a => a.popUpName == currentPopUpScreen).popUpBase.HideScreen();
            isPopUpOn = false;
        }
    }

    public T GetScreen<T>() where T : UIBase
    {

        ScreenType requestedScreenType = screenTypes.Find(st => st.screenBase.GetType().Name == typeof(T).Name);

        return requestedScreenType?.screenBase as T;
    }

    #endregion

    #region PRIVATE_FUNCTIONS
    private void InitializeScreen()
    {
        foreach (var item in screenTypes)
        {
            if (item.screenName == InitScreen)
            {
                item.screenBase.ShowScreen();
                currentScreen = item.screenName;
            }
            else
            {
                item.screenBase.HideScreen();
            }
        }
    }
    #endregion

    #region CO-ROUTINES
    #endregion

    #region EVENT_HANDLERS
    #endregion

    #region UI_CALLBACKS
    #endregion
}
