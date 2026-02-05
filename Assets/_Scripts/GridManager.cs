using UnityEngine;
using System.Collections;

public class GridManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject cellPrefab;
    public float spacing = 1.1f;

    private Transform[,] grid;

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
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
        // --- SỬA LỖI 1: Thêm UnityEngine. trước Application ---
        if (UnityEngine.Application.isPlaying) return;

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
        return GridToWorld(gridPos);
    }

    // --- SỬA LỖI 2: Thêm lại hàm IsWithinGrid ---
    public bool IsWithinGrid(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);
        return gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height;
    }

    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
        {
            return true;
        }
        return grid[x, y] != null;
    }

    public void PlacePiece(Transform[] blockPieces)
    {
        foreach (var block in blockPieces)
        {
            Vector2Int gridPos = WorldToGrid(block.position);
            if (gridPos.x >= 0 && gridPos.x < width && gridPos.y >= 0 && gridPos.y < height)
            {
                grid[gridPos.x, gridPos.y] = block;
            }
        }
        CheckForCompletedLines();
    }

    public void CheckForCompletedLines()
    {
        int clearedLinesThisTurn = 0;

        for (int y = 0; y < height; y++)
        {
            if (IsRowComplete(y))
            {
                ClearRow(y);
                clearedLinesThisTurn++;
                y--;
            }
        }

        for (int x = 0; x < width; x++)
        {
            if (IsColumnComplete(x))
            {
                ClearColumn(x);
                clearedLinesThisTurn++;
                x--;
            }
        }

        if (clearedLinesThisTurn > 0)
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.IncrementCombo();
                int basePoints = 100;
                int comboMultiplier = ScoreManager.Instance.ComboCount + 1;
                ScoreManager.Instance.AddPoints(basePoints * clearedLinesThisTurn * comboMultiplier);
            }
        }
        else
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.ResetCombo();
        }
    }

    private bool IsRowComplete(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null) return false;
        }
        return true;
    }

    private void ClearRow(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                StartCoroutine(FadeOutAndDestroy(grid[x, y].gameObject));
                grid[x, y] = null;
            }
        }
    }

    private bool IsColumnComplete(int x)
    {
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] == null) return false;
        }
        return true;
    }

    private void ClearColumn(int x)
    {
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] != null)
            {
                StartCoroutine(FadeOutAndDestroy(grid[x, y].gameObject));
                grid[x, y] = null;
            }
        }
    }

    private IEnumerator FadeOutAndDestroy(GameObject blockToDestroy)
    {
        SpriteRenderer renderer = blockToDestroy.GetComponent<SpriteRenderer>();
        if (renderer != null)
        {
            float duration = 0.3f;
            float elapsedTime = 0f;
            Color originalColor = renderer.color;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / duration);
                renderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                yield return null;
            }
        }
        Destroy(blockToDestroy);
    }

    public Transform[,] GetGridData()
    {
        return grid;
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3 gridBottomLeft = GetGridBottomLeft();
        return gridBottomLeft + new Vector3(gridPos.x * spacing, gridPos.y * spacing, 0);
    }

    public bool CanPlaceAt(GameObject piece, Vector2Int gridAnchorPos)
    {
        if (piece.transform.childCount == 0) return false;

        foreach (Transform child in piece.transform)
        {
            Vector3 localPos = child.localPosition;
            int offsetX = Mathf.RoundToInt(localPos.x);
            int offsetY = Mathf.RoundToInt(localPos.y);

            int targetX = gridAnchorPos.x + offsetX;
            int targetY = gridAnchorPos.y + offsetY;

            if (IsCellOccupied(targetX, targetY))
            {
                return false;
            }
        }

        return true;
    }
}