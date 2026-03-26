using UnityEngine;
using System;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance;

    public bool IsMusicOn { get; private set; }
    public bool IsSoundOn { get; private set; }

    public event Action OnSettingsChanged;

    private const string MUSIC_KEY = "MUSIC";
    private const string SOUND_KEY = "SOUND";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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