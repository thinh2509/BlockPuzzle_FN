using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class LoginManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public Button loginButton;
    public Button goToRegisterButton;
    public TextMeshProUGUI feedbackText;

    private const string ApiBaseUrl = "https://localhost:7051/api/Auth";

    [System.Serializable]
    private class LoginData { public string email; public string password; }

    void Start()
    {
        loginButton.onClick.AddListener(() => StartCoroutine(LoginCoroutine()));
        goToRegisterButton.onClick.AddListener(GoToRegisterScene);
    }

    void GoToRegisterScene()
    {
        SceneManager.LoadScene("Register"); // Load the new Register scene
    }

    private IEnumerator LoginCoroutine()
    {
        string email = emailInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            feedbackText.text = "Invalid email format.";
            yield break;
        }
        if (string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Password cannot be empty.";
            yield break;
        }

        feedbackText.text = "Logging in...";
        LoginData data = new LoginData { email = email, password = password };
        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest($"{ApiBaseUrl}/login", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = new BypassCertificateHandler();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                feedbackText.text = "Login Successful!";
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                string errorMessage = request.downloadHandler.text;
                if (string.IsNullOrEmpty(errorMessage)) errorMessage = request.error;
                feedbackText.text = "Error: " + errorMessage;
            }
        }
    }
}