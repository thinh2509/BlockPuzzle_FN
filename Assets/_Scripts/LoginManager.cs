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

    // --- Helper classes for JSON serialization ---
    [System.Serializable]
    private class LoginRequestData { public string email; public string password; }

    [System.Serializable]
    private class LoginResponseData { public string token; }


    void Start()
    {
        loginButton.onClick.AddListener(() => StartCoroutine(LoginCoroutine()));
        goToRegisterButton.onClick.AddListener(GoToRegisterScene);
    }

    void GoToRegisterScene()
    {
        SceneManager.LoadScene("Register"); 
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
        LoginRequestData data = new LoginRequestData { email = email, password = password };
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
                // Deserialize the response to get the token
                string jsonResponse = request.downloadHandler.text;
                LoginResponseData responseData = JsonUtility.FromJson<LoginResponseData>(jsonResponse);

                if (responseData != null && !string.IsNullOrEmpty(responseData.token))
                {
                    // Save the token
                    AuthManager.Instance.SetToken(responseData.token);

                    feedbackText.text = "Login Successful!";
                    yield return new WaitForSeconds(1); // Wait a moment so user can see the message
                    SceneManager.LoadScene("MainMenu");
                }
                else
                {
                    feedbackText.text = "Error: Invalid token received.";
                }
            }
            else
            {
                // Start with the basic error from the request object
                string errorMessage = request.error;

                // If there's a response body, it might contain a more specific message
                if (request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    // For 401 Unauthorized, we provide a standard message.
                    if (request.responseCode == 401) {
                        errorMessage = "Invalid credentials.";
                    } else {
                        errorMessage = request.downloadHandler.text;
                    }
                }
                else if (string.IsNullOrEmpty(errorMessage))
                {
                    errorMessage = "An unknown error occurred.";
                }

                feedbackText.text = "Error: " + errorMessage;
            }
        }
    }
}
