using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject cellPrefab; // Kéo Prefab vào đây
    public float spacing = 1.1f; // Khoảng cách giữa các ô (1.0 là dính liền)

    // [NÂNG CẤP] Thay đổi mảng bool thành mảng Transform để lưu trữ các khối gạch
    private Transform[,] grid; 

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        // Khởi tạo grid mới
        grid = new Transform[width, height];

        float gridW = width * spacing;
        float gridH = height * spacing;
        Vector2 startPos = new Vector2(-gridW / 2 + spacing / 2, -gridH / 2 + spacing / 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                Vector2 pos = new Vector2(x * spacing, y * spacing) + startPos;
                newCell.transform.localPosition = pos;
                newCell.name = $"Cell {x}x{y}";
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying) return;
        Gizmos.color = Color.yellow;
        float gridW = width * spacing;
        float gridH = height * spacing;
        Vector2 startPos = new Vector2(-gridW / 2 + spacing / 2, -gridH / 2 + spacing / 2);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 pos = new Vector2(x * spacing, y * spacing) + startPos;
                Gizmos.DrawWireCube(transform.position + (Vector3)pos, new Vector3(spacing, spacing, 0));
            }
        }
    }
    
    private Vector3 GetGridBottomLeft()
    {
        Vector3 gridCenter = transform.position;
        float gridW = width * spacing;
        float gridH = height * spacing;
        return gridCenter - new Vector3(gridW / 2, gridH / 2, 0) + new Vector3(spacing / 2, spacing / 2, 0);
    }
    
    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 gridBottomLeft = GetGridBottomLeft();
        Vector3 relativePos = worldPos - gridBottomLeft;
        int gridX = Mathf.RoundToInt(relativePos.x / spacing);
        int gridY = Mathf.RoundToInt(relativePos.y / spacing);
        return new Vector2Int(gridX, gridY);
    }
    
    public Vector3 GetNearestCellCenter(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        Vector3 gridBottomLeft = GetGridBottomLeft();
        Vector3 snappedWorldPos = gridBottomLeft + new Vector3(gridPos.x * spacing, gridPos.y * spacing, 0);
        return snappedWorldPos;
    }

    // [NÂNG CẤP] Kiểm tra xem ô có bị chiếm chưa bằng cách xem nó có null không
    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return true;
        }
        // Nếu trong ô có một Transform, tức là nó đã bị chiếm
        return grid[x, y] != null;
    }

    // [NÂNG CẤP] Hàm này nhận vào mảng các Transform của các ô vuông nhỏ
    public void PlacePiece(Transform[] blockPieces)
    {
        foreach (var block in blockPieces)
        {
            Vector2Int gridPos = WorldToGrid(block.position);
            if (gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height)
            {
                // Lưu trữ Transform của ô vuông nhỏ vào grid
                grid[gridPos.x, gridPos.y] = block;
            }
        }
    }
    
    public bool IsWithinGrid(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        if (gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height)
        {
            return true;
        }
        return false;
    }

    // --- LOGIC XÓA HÀNG ---

    public void CheckForCompletedLines()
    {
        for (int y = 0; y < height; y++)
        {
            if (IsRowComplete(y))
            {
                ClearRow(y);
                ShiftRowsDown(y + 1);
                y--; // Giảm y để kiểm tra lại hàng hiện tại (vì nó đã được dịch chuyển từ trên xuống)
            }
        }
    }

    private bool IsRowComplete(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
            {
                return false; // Tìm thấy ô trống, hàng chưa đầy
            }
        }
        return true; // Không tìm thấy ô trống nào, hàng đã đầy
    }

    private void ClearRow(int y)
    {
        Debug.Log($"Clearing row {y}");
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject); // Hủy GameObject của khối gạch
                grid[x, y] = null; // Xóa tham chiếu khỏi grid
            }
        }
    }

    private void ShiftRowsDown(int startY)
    {
        for (int y = startY; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                {
                    // Di chuyển khối gạch trong mảng dữ liệu
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;

                    // Di chuyển khối gạch trong thế giới game (visual)
                    grid[x, y - 1].position += Vector3.down * spacing;
                }
            }
        }
    }
}
