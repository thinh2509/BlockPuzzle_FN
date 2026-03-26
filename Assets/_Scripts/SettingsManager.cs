using UnityEngine;
using System;

public class SettingsManager : MonoBehaviour
{
    private static SettingsManager _instance;
    public static SettingsManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SettingsManager>();
            }
            return _instance;
        }
    }

    public bool IsMusicOn { get; private set; }
    public bool IsSoundOn { get; private set; }

    public event Action OnSettingsChanged;

    private const string MUSIC_KEY = "MUSIC";
    private const string SOUND_KEY = "SOUND";

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    public void SetMusic(bool value)
    {
        IsMusicOn = value;

        PlayerPrefs.SetInt(MUSIC_KEY, value ? 1 : 0); 
        PlayerPrefs.Save();

        OnSettingsChanged?.Invoke();
    }

    public void SetSound(bool value)
    {
        IsSoundOn = value;
        PlayerPrefs.SetInt(SOUND_KEY, value ? 1 : 0);
    }

    private void LoadSettings()
    {
        IsMusicOn = PlayerPrefs.GetInt("MUSIC", 1) == 1;
        IsSoundOn = PlayerPrefs.GetInt("SOUND", 1) == 1;
    }

}