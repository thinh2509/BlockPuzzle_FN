using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginChecker : MonoBehaviour
{
    void Start()
    {
        // Ensure AuthManager exists
        if (AuthManager.Instance == null)
        {
            Debug.LogError("AuthManager not found in the scene. Make sure it's in your initial scene.");
            // Force back to login because we can't verify status
            SceneManager.LoadScene("Login");
            return;
        }

        // Check if the user is logged in
        if (!AuthManager.Instance.IsLoggedIn)
        {
            // If not, redirect to the Login scene
            SceneManager.LoadScene("Login");
        }
    }
}
