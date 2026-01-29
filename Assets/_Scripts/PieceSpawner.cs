using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Cài đặt Gạch")]
    public GameObject[] blockPrefabs; // Danh sách các loại gạch (L, Vuông, Dài...)
    public Transform[] spawnPoints;   // 3 vị trí spawn (Pos_1, Pos_2, Pos_3)

    void Start()
    {
        SpawnBlocks();
    }

    public void SpawnBlocks()
    {
        // Duyệt qua từng vị trí spawn
        foreach (Transform point in spawnPoints)
        {
            // 1. Chọn ngẫu nhiên 1 loại gạch
            int randomIndex = Random.Range(0, blockPrefabs.Length);
            GameObject randomBlock = blockPrefabs[randomIndex];

            // 2. Sinh ra gạch tại vị trí đó
            Instantiate(randomBlock, point.position, Quaternion.identity);
        }
    }
}