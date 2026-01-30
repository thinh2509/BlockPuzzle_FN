using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Cài đặt Gạch")]
    public GameObject[] blockPrefabs; // Danh sách các loại gạch (L, Vuông, Dài...)
    public Transform[] spawnPoints;   // 3 vị trí spawn (Pos_1, Pos_2, Pos_3)

    // Danh sách để theo dõi các khối gạch đã được sinh ra
    private List<GameObject> spawnedPieces = new List<GameObject>();

    void Start()
    {
        SpawnBlocks();
    }

    public void SpawnBlocks()
    {
        // Xóa các khối gạch cũ còn lại trong danh sách (nếu có)
        spawnedPieces.Clear();

        // Duyệt qua từng vị trí spawn
        foreach (Transform point in spawnPoints)
        {
            // 1. Chọn ngẫu nhiên 1 loại gạch
            int randomIndex = Random.Range(0, blockPrefabs.Length);
            GameObject randomBlockPrefab = blockPrefabs[randomIndex];

            // 2. Sinh ra gạch tại vị trí đó
            GameObject newPiece = Instantiate(randomBlockPrefab, point.position, Quaternion.identity);

            // 3. Thêm khối gạch mới vào danh sách theo dõi
            spawnedPieces.Add(newPiece);
        }
    }

    // Hàm này sẽ được gọi bởi PieceControl khi một khối gạch được đặt thành công
    public void OnPiecePlaced(GameObject piece)
    {
        // Xóa khối gạch đã đặt khỏi danh sách theo dõi
        if (spawnedPieces.Contains(piece))
        {
            spawnedPieces.Remove(piece);
        }

        // Nếu danh sách rỗng (cả 3 khối đã được đặt), thì sinh ra các khối mới
        if (spawnedPieces.Count == 0)
        {
            Debug.Log("Tất cả các khối đã được đặt. Tạo khối mới!");
            SpawnBlocks();
        }
    }
}