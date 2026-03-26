
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle soundToggle;
    [SerializeField] private Toggle vibrationToggle;

    private void OnEnable()
    {
        LoadUI();
    }

    private void LoadUI()
    {
        musicToggle.SetIsOnWithoutNotify(SettingsManager.Instance.IsMusicOn);
        soundToggle.SetIsOnWithoutNotify(SettingsManager.Instance.IsSoundOn);
    }

    public void OnMusicToggle(bool isOn)
    {
        SettingsManager.Instance.SetMusic(isOn);
    }

    public void OnSoundToggle(bool value)
    {
        SettingsManager.Instance.SetSound(value);
    }


    [SerializeField] private MainMenuManager mainMenuManager;

    public void Close()
    {
        if (mainMenuManager != null)
        {
            mainMenuManager.HideSettingsPopup();
        }
    }
}