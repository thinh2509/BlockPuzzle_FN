using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Collections;

public class WaitingRoomManager : MonoBehaviour
{
    public TMP_Text roomCodeText;
    public GameObject avatarP2;
    public TMP_Text txtP2;

    string baseUrl = "https://localhost:7051/api/room";
    [System.Serializable]
    public class RoomResponse
    {
        public string id;
        public string roomCode;
        public string player1Id;
        public string player2Id;
        public int player1Score;
        public int player2Score;
        public string status;
    }

    void Start()
    {
        roomCodeText.text = "ROOM: " + RoomData.CurrentRoomCode;

        StartCoroutine(CheckRoomReady());
    }

    IEnumerator CheckRoomReady()
    {
        while (true)
        {
            string url = baseUrl + "/" + RoomData.CurrentRoomCode;
            Debug.Log("Calling URL: " + url);

            UnityWebRequest www = UnityWebRequest.Get(url);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                string json = www.downloadHandler.text;

                RoomResponse room =
                    JsonUtility.FromJson<RoomResponse>(json);

                // Nếu có player2
                if (!string.IsNullOrEmpty(room.player2Id))
                {
                    avatarP2.GetComponent<UnityEngine.UI.Image>().color = Color.white;
                    txtP2.text = "Player 2 Joined!";
                }

                // Nếu phòng đã full hoặc status khác Waiting
                if (room.status == "Full" || room.status == "Playing")
                {
                    SceneManager.LoadScene("Gameplay");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("Check room error: " + www.error);
            }

            yield return new WaitForSeconds(2f);
        }
    }
}