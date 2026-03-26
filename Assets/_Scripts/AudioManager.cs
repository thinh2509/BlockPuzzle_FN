
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ApplySettings();
        SettingsManager.Instance.OnSettingsChanged += ApplySettings;
    }

    private void ApplySettings()
    {
        bool musicOn = SettingsManager.Instance.IsMusicOn;

        if (musicSource == null)
        {
            return;
        }

        if (musicSource != null)
        {
            musicSource.mute = !musicOn;

            if (musicOn)
            {
                if (!musicSource.isPlaying)
                {
                    musicSource.Play();
                }
            }
            else
            {
                musicSource.Stop();
            }
        }
        
        if (sfxSource != null)
        {
            sfxSource.mute = !SettingsManager.Instance.IsSoundOn;
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (SettingsManager.Instance.IsSoundOn)
        {
            sfxSource.PlayOneShot(clip);
        }
    }


}