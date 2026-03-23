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
    public GameObject winPanel;
    public GameObject losePanel;

    private HubConnection connection;
    private int opponentScore = 0;
    
    private float timeLeft = 180f; // 180 seconds = 3 minutes
    private bool isGameActive = false;
    
    // Dispatcher cho SignalR event
    private readonly ConcurrentQueue<Action> _executionQueue = new ConcurrentQueue<Action>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // Bỏ qua DontDestroyOnLoad nếu bạn chỉ kéo vào Gameplay scene
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
        // Khởi tạo kết nối tới Hub. (Lưu ý: URL server của bạn là https://localhost:7051)
        connection = new HubConnectionBuilder()
            .WithUrl("https://localhost:7051/gamehub")
            .WithAutomaticReconnect()
            .Build();

        // Lắng nghe sự kiện từ Server
        connection.On<int>("UpdateOpponentScore", (newScore) =>
        {
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
            UpdateTimerUI();

            if (timeLeft <= 0)
            {
                timeLeft = 0;
                isGameActive = false;
                EvaluateWinner();
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeLeft / 60);
            int seconds = Mathf.FloorToInt(timeLeft % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void UpdateOpponentScoreUI(int score)
    {
        if (opponentScoreText != null)
        {
            opponentScoreText.text = "Opponent: " + score;
        }
    }

    public async void SendScoreUpdate(int newScore)
    {
        if (connection != null && connection.State == HubConnectionState.Connected)
        {
            try
            {
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

        if (isWin && winPanel != null) winPanel.SetActive(true);
        else if (!isWin && losePanel != null) losePanel.SetActive(true);
    }

    private void ShowDraw()
    {
        isGameActive = false;
        Time.timeScale = 0f;

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
