using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public bool isFirstTime = true;
    public string playerName = string.Empty;
}

public class PlayerDataManager : Singleton<PlayerDataManager>
{

    [SerializeField] private PlayerData playerData = new PlayerData();

    public bool IsFirstTime => playerData.isFirstTime;
    public string PlayerName => playerData.playerName;

    public override void Awake()
    {
        base.Awake();
        Load();
    }

    public void SavePlayerName(string playerName)
    {
        playerData.playerName = playerName.Trim();
        playerData.isFirstTime = false;

        PlayerPrefs.SetString("PlayerData.Name", playerData.playerName);
        PlayerPrefs.SetInt("PlayerData.FirstTime", 0);
        PlayerPrefs.Save();
    }

    private void Load()
    {
        playerData.playerName = PlayerPrefs.GetString("PlayerData.Name", string.Empty);
        playerData.isFirstTime = PlayerPrefs.GetInt("PlayerData.FirstTime", 1) == 1;
    }

}
