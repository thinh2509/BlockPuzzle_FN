using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    public static GridManager Instance;

    public int width = 8;
    public int height = 8;
    public GameObject cellPrefab;
    public float spacing = 1.1f;

    private Transform[,] grid;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        ClearGeneratedCells();
        GenerateGrid();
    }

    private void EnsureGridInitialized()
    {
        if (grid == null)
        {
            ClearGeneratedCells();
            GenerateGrid();
        }
    }



    private void GenerateGrid()
    {
        grid = new Transform[width, height];

        float gridW = width * spacing;
        float gridH = height * spacing;

        Vector2 startPos = new Vector2(
            -gridW / 2 + spacing / 2,
            -gridH / 2 + spacing / 2
        );

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                GameObject newCell = Instantiate(cellPrefab, transform);
                Vector2 pos = new Vector2(x * spacing, y * spacing) + startPos;
                newCell.transform.localPosition = pos;
                newCell.name = $"Cell_{x}_{y}";

                SpriteRenderer cellRenderer = newCell.GetComponent<SpriteRenderer>();
                if (cellRenderer != null)
                {
                    cellRenderer.sortingLayerName = "Default";
                    cellRenderer.sortingOrder = 0;
                }
            }
        }
    }

    private Vector3 GetGridBottomLeft()
    {
        Vector3 gridCenter = transform.position;
        float gridW = width * spacing;
        float gridH = height * spacing;

        return gridCenter - new Vector3(gridW / 2, gridH / 2, 0)
             + new Vector3(spacing / 2, spacing / 2, 0);
    }

    public Vector2Int WorldToGrid(Vector3 worldPos)
    {
        Vector3 gridBottomLeft = GetGridBottomLeft();
        Vector3 relativePos = worldPos - gridBottomLeft;

        int gridX = Mathf.RoundToInt(relativePos.x / spacing);
        int gridY = Mathf.RoundToInt(relativePos.y / spacing);

        return new Vector2Int(gridX, gridY);
    }

    public Vector3 GridToWorld(Vector2Int gridPos)
    {
        Vector3 gridBottomLeft = GetGridBottomLeft();

        return gridBottomLeft + new Vector3(
            gridPos.x * spacing,
            gridPos.y * spacing,
            0f
        );
    }

    public Vector2Int GetNearestGridPos(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGrid(worldPos);

        gridPos.x = Mathf.Clamp(gridPos.x, 0, width - 1);
        gridPos.y = Mathf.Clamp(gridPos.y, 0, height - 1);

        return gridPos;
    }

    public Vector3 GetNearestCellCenter(Vector3 worldPos)
    {
        return GridToWorld(GetNearestGridPos(worldPos));
    }

    public bool IsWithinGrid(Vector2Int gridPos)
    {
        return gridPos.x >= 0 && gridPos.x < width &&
               gridPos.y >= 0 && gridPos.y < height;
    }

    public bool IsWithinGrid(Vector3 worldPos)
    {
        return IsWithinGrid(WorldToGrid(worldPos));
    }

    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= width || y < 0 || y >= height)
            return true;

        return grid[x, y] != null;
    }

    public List<Vector2Int> GetPieceOffsets(Transform pieceRoot)
    {
        List<Vector2Int> offsets = new List<Vector2Int>();

        foreach (Transform child in pieceRoot)
        {
            int offsetX = Mathf.RoundToInt(child.localPosition.x / spacing);
            int offsetY = Mathf.RoundToInt(child.localPosition.y / spacing);
            offsets.Add(new Vector2Int(offsetX, offsetY));
        }

        return offsets;
    }

    public bool CanPlacePieceAt(Transform pieceRoot, Vector2Int anchor)
    {
        List<Vector2Int> offsets = GetPieceOffsets(pieceRoot);

        foreach (var offset in offsets)
        {
            Vector2Int target = anchor + offset;

            if (!IsWithinGrid(target))
                return false;

            if (grid[target.x, target.y] != null)
                return false;
        }

        return true;
    }

    public bool CanPlaceAt(GameObject piece, Vector2Int gridPos)
    {
        return CanPlacePieceAt(piece.transform, gridPos);
    }

    public bool PlacePiece(Transform pieceRoot)
    {
        Vector2Int anchor = GetNearestGridPos(pieceRoot.position);
        List<Vector2Int> offsets = GetPieceOffsets(pieceRoot);

        if (!CanPlacePieceAt(pieceRoot, anchor))
        {
            Debug.LogWarning("PlacePiece called on invalid position.");
            return false;
        }

        List<Transform> childBlocks = new List<Transform>();
        foreach (Transform child in pieceRoot)
            childBlocks.Add(child);

        for (int i = 0; i < childBlocks.Count; i++)
        {
            Transform block = childBlocks[i];
            Vector2Int target = anchor + offsets[i];

            block.SetParent(transform);
            block.position = GridToWorld(target);
            grid[target.x, target.y] = block;

            SpriteRenderer blockRenderer = block.GetComponent<SpriteRenderer>();
            if (blockRenderer != null)
            {
                blockRenderer.sortingLayerName = "Default";
                blockRenderer.sortingOrder = 5;
            }
        }

        Destroy(pieceRoot.gameObject);

        CheckForCompletedLines();

        if (ChallengeManager.Instance != null)
            ChallengeManager.Instance.UseMove();

        return true;
    }

    public void CheckForCompletedLines()
    {
        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsGameOver)
            return;

        List<int> fullRows = new List<int>();
        List<int> fullCols = new List<int>();

        for (int y = 0; y < height; y++)
        {
            if (IsRowComplete(y))
                fullRows.Add(y);
        }

        for (int x = 0; x < width; x++)
        {
            if (IsColumnComplete(x))
                fullCols.Add(x);
        }

        int clearedLines = fullRows.Count + fullCols.Count;

        if (clearedLines == 0)
        {
            if (ScoreManager.Instance != null)
                ScoreManager.Instance.ResetCombo();
            return;
        }

        foreach (int y in fullRows)
            ClearRow(y);

        foreach (int x in fullCols)
            ClearColumn(x);

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.IncrementCombo();
            int basePoints = 100;
            int comboMultiplier = ScoreManager.Instance.ComboCount + 1;
            int pointsToAdd = basePoints * clearedLines * comboMultiplier;
            ScoreManager.Instance.AddPoints(pointsToAdd);

            if (GameManager1v1.Instance != null)
            {
                GameManager1v1.Instance.UpdateMyScore(pointsToAdd);
            }
        }
    }

    private bool IsRowComplete(int y)
    {
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] == null)
                return false;
        }
        return true;
    }

    private bool IsColumnComplete(int x)
    {
        for (int y = 0; y < height; y++)
        {
            if (grid[x, y] == null)
                return false;
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

    private IEnumerator FadeOutAndDestroy(GameObject block)
    {
        SpriteRenderer renderer = block.GetComponent<SpriteRenderer>();

        if (renderer != null)
        {
            float duration = 0.15f;
            float time = 0f;
            Color original = renderer.color;

            while (time < duration)
            {
                time += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, time / duration);
                renderer.color = new Color(original.r, original.g, original.b, alpha);
                yield return null;
            }
        }

        Destroy(block);
    }

    public bool CanPlaceAnyBlock(GameObject[] pieces)
    {
        foreach (var piece in pieces)
        {
            if (piece == null) continue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (CanPlacePieceAt(piece.transform, new Vector2Int(x, y)))
                        return true;
                }
            }
        }

        return false;
    }

    public bool NoMovesLeft(GameObject[] pieces)
    {
        return !CanPlaceAnyBlock(pieces);
    }

    public void ClearAllBlocks()
    {
        EnsureGridInitialized();

        for (int x = 0; x < width; x++)
        {
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


    public void PlaceObstacleRandom(int amount, GameObject obstaclePrefab)
    {
        if (obstaclePrefab == null)
        {
            Debug.LogError("Obstacle Prefab is NULL");
            return;
        }

        int placed = 0;
        int safe = 0;

        while (placed < amount && safe < 500)
        {
            safe++;

            int x = Random.Range(0, width);
            int y = Random.Range(0, height);

            Vector2Int anchor = new Vector2Int(x, y);

            GameObject obstacle = Instantiate(
                obstaclePrefab,
                GridToWorld(anchor),
                Quaternion.identity,
                transform
            );

            List<Vector2Int> offsets = GetPieceOffsets(obstacle.transform);

            bool valid = true;

            foreach (var offset in offsets)
            {
                Vector2Int target = anchor + offset;

                if (!IsWithinGrid(target) || grid[target.x, target.y] != null)
                {
                    valid = false;
                    break;
                }
            }

            if (!valid)
            {
                Destroy(obstacle);
                continue;
            }

            int i = 0;
            foreach (Transform child in obstacle.transform)
            {
                Vector2Int target = anchor + offsets[i];

                child.SetParent(transform);
                child.position = GridToWorld(target);

                grid[target.x, target.y] = child;

                SpriteRenderer sr = child.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 4;
                }

                i++;
            }

            Destroy(obstacle);
            placed++;
        }
    }

    private void ClearGeneratedCells()
    {
        List<Transform> childrenToDelete = new List<Transform>();

        foreach (Transform child in transform)
        {
            if (child.name.StartsWith("Cell_"))
            {
                childrenToDelete.Add(child);
            }
        }

        foreach (Transform child in childrenToDelete)
        {
            Destroy(child.gameObject);
        }
    }

    public bool CanPlaceObstacleShapeAt(Transform obstacleRoot, Vector2Int anchor)
    {
        List<Vector2Int> offsets = GetPieceOffsets(obstacleRoot);

        foreach (var offset in offsets)
        {
            Vector2Int target = anchor + offset;

            if (!IsWithinGrid(target))
                return false;

            if (grid[target.x, target.y] != null)
                return false;
        }

        return true;
    }

    public void PlaceRandomObstacleShapes(int amount, GameObject[] obstacleShapePrefabs)
    {
        EnsureGridInitialized();

        if (obstacleShapePrefabs == null || obstacleShapePrefabs.Length == 0)
        {
            Debug.LogWarning("No obstacle shape prefabs assigned.");
            return;
        }

        int placed = 0;
        int safe = 0;

        while (placed < amount && safe < 1000)
        {
            safe++;

            GameObject prefab = obstacleShapePrefabs[Random.Range(0, obstacleShapePrefabs.Length)];
            if (prefab == null) continue;

            int x = Random.Range(0, width);
            int y = Random.Range(0, height);
            Vector2Int anchor = new Vector2Int(x, y);

            GameObject obstacleRoot = Instantiate(prefab, GridToWorld(anchor), Quaternion.identity, transform);

            if (!CanPlaceObstacleShapeAt(obstacleRoot.transform, anchor))
            {
                Destroy(obstacleRoot);
                continue;
            }

            List<Vector2Int> offsets = GetPieceOffsets(obstacleRoot.transform);
            List<Transform> childBlocks = new List<Transform>();

            foreach (Transform child in obstacleRoot.transform)
                childBlocks.Add(child);

            for (int i = 0; i < childBlocks.Count; i++)
            {
                Transform block = childBlocks[i];
                Vector2Int target = anchor + offsets[i];

                block.SetParent(transform);
                block.position = GridToWorld(target);
                block.name = $"Obstacle_{target.x}_{target.y}";

                SpriteRenderer sr = block.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.sortingLayerName = "Default";
                    sr.sortingOrder = 4;
                }

                grid[target.x, target.y] = block;
            }

            Destroy(obstacleRoot);
            placed++;
        }
    }

    
}