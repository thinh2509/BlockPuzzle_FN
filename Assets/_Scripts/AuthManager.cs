using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    public string AuthToken { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(AuthToken);

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetToken(string token)
    {
        AuthToken = token;
        // You could also save the token to PlayerPrefs here for persistence across game sessions
        // PlayerPrefs.SetString("AuthToken", token);
    }

    public void ClearToken()
    {
        AuthToken = null;
        // PlayerPrefs.DeleteKey("AuthToken");
    }
}
