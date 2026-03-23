using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;
    public AudioClip clickSound;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayClick()
    {
        if (SettingsManager.Instance.IsSoundOn)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}