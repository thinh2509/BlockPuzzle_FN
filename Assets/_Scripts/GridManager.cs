using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject cellPrefab; // Kéo Prefab vào đây
    public float spacing = 1.1f; // Khoảng cách giữa các ô (1.0 là dính liền)

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Tính toán để căn giữa bàn cờ
        float gridW = width * spacing;
        float gridH = height * spacing;
        // Dời gốc tọa độ từ (0,0) về giữa bàn cờ
        Vector2 startPos = new Vector2(-gridW / 2 + spacing / 2, -gridH / 2 + spacing / 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                // Tạo ô mới
                GameObject newCell = Instantiate(cellPrefab, transform);

                // Đặt vị trí
                Vector2 pos = new Vector2(x * spacing, y * spacing) + startPos;
                newCell.transform.localPosition = pos;

                newCell.name = $"Cell {x}x{y}";
            }
        }
    }
}