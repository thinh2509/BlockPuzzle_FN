using UnityEngine;

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


    public void CheckForCompletedLines()
    {
        int clearedLinesThisTurn = 0;

        for (int y = 0; y < height; y++)
        {
            if (IsRowComplete(y))
            {
                ClearRow(y);
                ShiftRowsDown(y + 1);
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
            ScoreManager.Instance.IncrementCombo();
            
            int basePoints = 100;
            int comboMultiplier = ScoreManager.Instance.ComboCount + 1; 
            ScoreManager.Instance.AddPoints(basePoints * clearedLinesThisTurn * comboMultiplier);
        }
        else
        {
            ScoreManager.Instance.ResetCombo();
        }
    }

    private bool IsRowComplete(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
            {
                return false; 
            }
        }
        return true; 
    }

    private void ClearRow(int y)
    {
        Debug.Log($"Clearing row {y}");
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject); 
                grid[x, y] = null; 
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
                    
                    grid[x, y - 1] = grid[x, y];
                    grid[x, y] = null;

                    
                    grid[x, y - 1].position += Vector3.down * spacing;
                }
            }
        }
    }

    private bool IsColumnComplete(int x)
    {
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] == null)
            {
                return false; 
            }
        }
        return true; 
    }

    private void ClearColumn(int x)
    {
        Debug.Log($"Clearing column {x}");
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] != null)
            {
                Destroy(grid[x, y].gameObject);
                grid[x, y] = null;
            }
        }
    }
}
