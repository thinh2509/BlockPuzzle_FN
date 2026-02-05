using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Cài đặt Gạch")]
    public GameObject[] blockPrefabs;
    public Transform[] spawnPoints;
    public GameObject gameOverPanel;

    private List<GameObject> spawnedPieces = new List<GameObject>();
    private GridManager gridManager;

    void Start()
    {
        gridManager = FindObjectOfType<GridManager>();
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        SpawnBlocks();
    }

    public void SpawnBlocks()
    {
        // Xóa các khối cũ (nếu có)
        foreach (var piece in spawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }
        spawnedPieces.Clear();

        foreach (Transform point in spawnPoints)
        {
            // Chọn ngẫu nhiên một khối
            int randomIndex = UnityEngine.Random.Range(0, blockPrefabs.Length);
            GameObject randomBlockPrefab = blockPrefabs[randomIndex];

            // Tạo khối tại vị trí Spawn Point
            GameObject newPiece = Instantiate(randomBlockPrefab, point.position, Quaternion.identity);

            // --- ĐÃ XÓA DÒNG CHỈNH SIZE ĐỂ NÓ KHỚP VỚI PREFAB CỦA EM ---

            spawnedPieces.Add(newPiece);
        }

        // Kiểm tra xem vừa đẻ ra đã thua chưa (trường hợp hiếm)
        CheckForGameOver();
    }

    public void OnPiecePlaced(GameObject piece)
    {
        if (spawnedPieces.Contains(piece))
        {
            spawnedPieces.Remove(piece);
        }

        if (spawnedPieces.Count == 0)
        {
            UnityEngine.Debug.Log("--- HẾT GẠCH: Tạo đợt mới ---");
            SpawnBlocks();
        }
        else
        {
            // Còn gạch thì kiểm tra xem gạch đó có chỗ đặt không
            CheckForGameOver();
        }
    }

    void CheckForGameOver()
    {
        UnityEngine.Debug.Log($"[KIỂM TRA THUA] Đang kiểm tra {spawnedPieces.Count} khối còn lại...");

        foreach (var piece in spawnedPieces)
        {
            if (piece == null) continue;

            // Kiểm tra từng ô trên bàn cờ
            for (int x = 0; x < gridManager.width; x++)
            {
                for (int y = 0; y < gridManager.height; y++)
                {
                    // Nếu tìm thấy ÍT NHẤT 1 chỗ đặt được
                    if (gridManager.CanPlaceAt(piece, new Vector2Int(x, y)))
                    {
                        UnityEngine.Debug.Log($"-> SỐNG: {piece.name} đặt vừa tại ({x},{y}).");
                        return; // Game chưa thua, thoát hàm ngay lập tức!
                    }
                }
            }

            // Nếu chạy hết bàn cờ mà không tìm thấy chỗ cho khối này
            UnityEngine.Debug.LogWarning($"-> CHẾT: Khối {piece.name} không tìm thấy chỗ đặt nào!");
        }

        // Nếu chạy hết vòng lặp mà không return -> Nghĩa là tất cả khối đều không có chỗ đặt
        UnityEngine.Debug.LogError("GAME OVER CHÍNH THỨC: Không còn nước đi nào!");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f; // Dừng game
    }
}