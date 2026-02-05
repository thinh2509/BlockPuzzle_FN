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
        foreach (var piece in spawnedPieces)
        {
            if (piece != null) Destroy(piece);
        }
        spawnedPieces.Clear();

        foreach (Transform point in spawnPoints)
        {
            int randomIndex = UnityEngine.Random.Range(0, blockPrefabs.Length);
            GameObject randomBlockPrefab = blockPrefabs[randomIndex];

            GameObject newPiece = Instantiate(randomBlockPrefab, point.position, Quaternion.identity);

            spawnedPieces.Add(newPiece);
        }

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
            SpawnBlocks();
        }
        else
        {
            CheckForGameOver();
        }
    }

    void CheckForGameOver()
    {

        foreach (var piece in spawnedPieces)
        {
            if (piece == null) continue;

            for (int x = 0; x < gridManager.width; x++)
            {
                for (int y = 0; y < gridManager.height; y++)
                {
                    if (gridManager.CanPlaceAt(piece, new Vector2Int(x, y)))
                    {
                        return;
                    }
                }
            }
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
}