using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client; // Yêu cầu NuGetForUnity cài Microsoft.AspNetCore.SignalR.Client
using System;
using System.Collections.Concurrent;
using UnityEngine.SceneManagement;

public class MultiplayerManager : MonoBehaviour
{
    public static MultiplayerManager Instance;

    [Header("UI Reference (Assign in Inspector)")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI opponentScoreText;
    
    [Header("End Game Panels")]
    public GameObject resultPanel; // Bảng cha chứa Win và Lose
    public GameObject winPanel;
    public GameObject losePanel;

    private HubConnection connection;
    private int opponentScore = 0;
    
    private float timeLeft = 30f; // 180 seconds = 3 minutes
    private bool isGameActive = false;
    
    // Dispatcher cho SignalR event
    private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

    private void Awake()
    {
        UnityEngine.Debug.Log($"[Tracer] MultiplayerManager Awake on object: {gameObject.name} (ID: {GetInstanceID()})");
        if (Instance == null)
        {
            Instance = this;
            UnityEngine.Debug.Log($"[Tracer] MultiplayerManager.Instance SELETED => {gameObject.name} (ID: {GetInstanceID()})");
        }
        else
        {
            UnityEngine.Debug.Log($"[Tracer] WARNING! DUPLICATE MultiplayerManager found on: {gameObject.name}. Current Instance is on: {Instance.gameObject.name}");
        }
    }

    private async void Start()
    {
        // Chỉ kích hoạt Mode 1vs1 nếu có RoomCode (từ Waiting Room)
        if (!string.IsNullOrEmpty(RoomData.CurrentRoomCode))
        {
            isGameActive = true;
            UpdateTimerUI();
            UpdateOpponentScoreUI(0);
            await InitializeSignalR();
        }
        else
        {
            // Không phải chế độ 1vs1, ẩn UI
            if (timerText != null) timerText.gameObject.SetActive(false);
            if (opponentScoreText != null) opponentScoreText.gameObject.SetActive(false);
        }
    }

    private async Task InitializeSignalR()
    {
        // Sử dụng cổng HTTP thay vì HTTPS để tránh lỗi chứng chỉ SSL (từ file launchSettings.json)
        connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:5244/gamehub")
            .WithAutomaticReconnect()
            .Build();

        // Lắng nghe sự kiện từ Server
        connection.On<int>("UpdateOpponentScore", (newScore) =>
        {
            Debug.Log($"[SignalR] NHẬN ĐƯỢC điểm của đối phương: {newScore}");
            _executionQueue.Enqueue(() =>
            {
                opponentScore = newScore;
                UpdateOpponentScoreUI(opponentScore);
            });
        });

        connection.On<string>("GameOver", (winnerId) =>
        {
            _executionQueue.Enqueue(() =>
            {
                EndGame(winnerId == connection.ConnectionId);
            });
        });

        try
        {
            await connection.StartAsync();
            Debug.Log("SignalR Connected!");

            // Join Room ngay khi connect thành công
            await connection.InvokeAsync("JoinRoom", RoomData.CurrentRoomCode);
        }
        catch (Exception ex)
        {
            Debug.LogError("Lỗi kết nối SignalR: " + ex.Message);
        }
    }

    private void Update()
    {
        // Xử lý các action từ thread của SignalR
        while (_executionQueue.TryDequeue(out var action))
        {
            action.Invoke();
        }

        if (!isGameActive) return;

        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                isGameActive = false;
                UpdateTimerUI(); // Cập nhật để hiển thị 00:00 thay vì số âm
                EvaluateWinner();
            }
            else
            {
                UpdateTimerUI();
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            // Giữ lại định dạng chữ TIME nhiều màu
            timerText.text = string.Format("<color=#FF5555>T</color><color=#55AAFF>I</color><color=#FFAA00>M</color><color=#FFDD55>E</color>: {0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateOpponentScoreUI(int score)
    {
        if (opponentScoreText != null)
        {
            // Sử dụng màu Rich text giống hệt với ScoreManager
            opponentScoreText.text = "<color=#FF5555>O</color><color=#55AAFF>P</color><color=#FFAA00>P</color><color=#FFDD55>O</color><color=#55FF55>N</color><color=#FF5555>E</color><color=#55AAFF>N</color><color=#FFAA00>T</color> <color=#FFDD55>S</color><color=#55FF55>C</color><color=#FF5555>O</color><color=#55AAFF>R</color><color=#FFAA00>E</color>: " + score;
        }
    }

    public async void SendScoreUpdate(int newScore)
    {
        Debug.Log($"[Tracer] SendScoreUpdate on {gameObject.name} (ID: {GetInstanceID()}) | Connection is {(connection == null ? "NULL" : connection.State.ToString())} | RoomCode: '{RoomData.CurrentRoomCode}' | isGameActive: {isGameActive}");
        if (connection != null && connection.State == HubConnectionState.Connected)
        {
            try
            {
                Debug.Log($"[SignalR] ĐANG GỬI điểm lên server: {newScore}");
                await connection.InvokeAsync("SendScoreUpdate", RoomData.CurrentRoomCode, newScore);
            }
            catch (Exception ex)
            {
                Debug.LogError("Send score failed: " + ex.Message);
            }
        }
    }

    private void EvaluateWinner()
    {
        int myScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        
        if (myScore > opponentScore)
        {
            EndGame(true);
        }
        else if (myScore < opponentScore)
        {
            EndGame(false);
        }
        else
        {
            ShowDraw();
        }
    }

    private void EndGame(bool isWin)
    {
        isGameActive = false;
        Time.timeScale = 0f;

        if (resultPanel != null) resultPanel.SetActive(true); // Bật Panel cha trước
        if (isWin && winPanel != null) winPanel.SetActive(true);
        else if (!isWin && losePanel != null) losePanel.SetActive(true);
    }

    private void ShowDraw()
    {
        isGameActive = false;
        Time.timeScale = 0f;

        if (resultPanel != null) resultPanel.SetActive(true); // Bật Panel cha trước
        // Nếu hòa, tạm thời bật WinPanel (hoặc bạn có thể nhân bản tạo thêm drawPanel)
        if (winPanel != null) winPanel.SetActive(true);
    }

    private async void OnDestroy()
    {
        if (connection != null)
        {
            if (!string.IsNullOrEmpty(RoomData.CurrentRoomCode) && connection.State == HubConnectionState.Connected)
            {
                await connection.InvokeAsync("LeaveRoom", RoomData.CurrentRoomCode);
            }
            await connection.StopAsync();
            await connection.DisposeAsync();
        }
    }
}
