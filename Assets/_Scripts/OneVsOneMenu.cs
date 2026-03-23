using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class OneVsOneMenu : MonoBehaviour
{
    public GameObject panel1vs1;
    public string baseUrl = "https://localhost:7051/api/room";
    public TMP_InputField roomCodeInput;
    public GameObject waitingRoomPanel;
    public TMP_Text roomCodeText;
    // Mở panel

    public void OpenPanel()
    {
        panel1vs1.SetActive(true);
    }

    // Đóng panel
    public void ClosePanel()
    {
        panel1vs1.SetActive(false);
    }

    // Quick Match
    public void QuickMatch()
    {
        StartCoroutine(QuickMatchCoroutine());
    }

    IEnumerator QuickMatchCoroutine()
    {
        string userId = PlayerPrefs.GetString("userId");

        WWWForm form = new WWWForm();
        form.AddField("userId", userId);

        UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/quickmatch", form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string roomId = www.downloadHandler.text;

            PlayerPrefs.SetString("roomId", roomId);

            StartCoroutine(CheckRoomReady(roomId));
        }
        else
        {
            Debug.LogError(www.error);
        }
    }
    // Create Room
  
    public void CreateRoom()
    {
        StartCoroutine(CreateRoomCoroutine());
    }

    IEnumerator CreateRoomCoroutine()
    {
        // string userId = PlayerPrefs.GetString("userId");
        string userId = "1";
        Debug.Log("userId gửi lên server: " + userId);

        WWWForm form = new WWWForm();
        form.AddField("userId", userId);

        UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/create", form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;

            WaitingRoomManager.RoomResponse room =
                JsonUtility.FromJson<WaitingRoomManager.RoomResponse>(json);

            Debug.Log("Room created, code: " + room.roomCode);

            RoomData.CurrentRoomCode = room.roomCode;

            PlayerPrefs.SetString("roomId", room.id);

            SceneManager.LoadScene("WaitingRoom");
        }
        else
        {
            Debug.LogError("Create Room Error: " + www.error);
        }
    }
    IEnumerator CheckRoomReady(string roomId)
    {
        while (true)
        {
            UnityWebRequest www = UnityWebRequest.Get(baseUrl + "/" + roomId);

            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;

                if (json.Contains("Full"))
                {
                    SceneManager.LoadScene("Solo");
                    yield break;
                }
            }

            yield return new WaitForSeconds(2f);
        }
    }

    // Join Room
    public void JoinRoom()
    {
        StartCoroutine(JoinRoomCoroutine());
    }
    IEnumerator JoinRoomCoroutine()
    {
        string userId = "2"; // player 2
        string roomCode = roomCodeInput.text;

        Debug.Log("RoomCode: " + roomCode);
        Debug.Log("UserId: " + userId);

        WWWForm form = new WWWForm();
        form.AddField("userId", userId);
        form.AddField("roomCode", roomCode);

        UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/join", form);

        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            string json = www.downloadHandler.text;

            WaitingRoomManager.RoomResponse room =
                JsonUtility.FromJson<WaitingRoomManager.RoomResponse>(json);

            Debug.Log("Joined room: " + room.roomCode);

            RoomData.CurrentRoomCode = room.roomCode;

            SceneManager.LoadScene("WaitingRoom");
        }
        else
        {
            Debug.LogError("Join Room Error: " + www.error);
            Debug.LogError("Server response: " + www.downloadHandler.text);
        }
    }

    //IEnumerator JoinRoomCoroutine()
    //{
    //    string userId = PlayerPrefs.GetString("userId");
    //    string roomCode = roomCodeInput.text;
    //     Debug.Log("RoomCode: " + roomCode);
    //    Debug.Log("UserId: " + userId);

    //    WWWForm form = new WWWForm();
    //    form.AddField("userId", userId);
    //    form.AddField("roomCode", roomCode);

    //    UnityWebRequest www = UnityWebRequest.Post(baseUrl + "/join", form);

    //    yield return www.SendWebRequest();

    //    if (www.result == UnityWebRequest.Result.Success)
    //    {

    //        string json = www.downloadHandler.text;

    //        WaitingRoomManager.RoomResponse room =
    //            JsonUtility.FromJson<WaitingRoomManager.RoomResponse>(json);

    //        Debug.Log("Joined room: " + room.roomCode);

    //        RoomData.CurrentRoomCode = room.roomCode;

    //        SceneManager.LoadScene("WaitingRoom");
    //    }
    //    else
    //    {
    //        Debug.LogError("Join Room Error: " + www.error);
    //        Debug.LogError("Server response: " + www.downloadHandler.text);
    //    }
    //}


}