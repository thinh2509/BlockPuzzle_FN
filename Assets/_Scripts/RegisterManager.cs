using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using TMPro;
using System.Collections;
using System.Text;

public class RegisterManager : MonoBehaviour
{
    [Header("Register UI")]
    public TMP_InputField emailRegInput;
    public TMP_InputField passwordRegInput;
    public TMP_InputField nameRegInput;
    public TMP_InputField addressRegInput;
    public TMP_InputField dobRegInput;
    public TMP_Dropdown genderRegDropdown;
    public Button registerButton;
    public Button goToLoginButton;
    public TextMeshProUGUI feedbackText;

    private const string ApiBaseUrl = "https://localhost:7051/api/Auth";

    [System.Serializable]
    private class RegisterData { public string email; public string password; public string name; public string address; public string dateOfBirth; public string gender; }

    void Start()
    {
        registerButton.onClick.AddListener(() => StartCoroutine(RegisterCoroutine()));
        goToLoginButton.onClick.AddListener(GoToLoginScene);
    }

    void GoToLoginScene()
    {
        SceneManager.LoadScene("Login");
    }

    private IEnumerator RegisterCoroutine()
    {
        feedbackText.text = "Registering...";
        
        // Get gender from Dropdown
        string selectedGender = genderRegDropdown.options[genderRegDropdown.value].text;

        RegisterData data = new RegisterData
        {
            email = emailRegInput.text,
            password = passwordRegInput.text,
            name = nameRegInput.text,
            address = addressRegInput.text,
            dateOfBirth = dobRegInput.text,
            gender = selectedGender
        };

        if (string.IsNullOrEmpty(data.email) || !data.email.Contains("@") || string.IsNullOrEmpty(data.password) || string.IsNullOrEmpty(data.name))
        {
            feedbackText.text = "Email, Password, and Name are required.";
            yield break;
        }

        string jsonBody = JsonUtility.ToJson(data);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest($"{ApiBaseUrl}/register", "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.certificateHandler = new BypassCertificateHandler();

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                feedbackText.text = "Registration successful! Please return to login.";
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
