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
        // Clear any remaining pieces (if any) before spawning new ones
        foreach (var piece in spawnedPieces)
        {
            if (piece != null)
            {
                Destroy(piece);
            }
        }
        spawnedPieces.Clear();

       
        foreach (Transform point in spawnPoints)
        {
            
            int randomIndex = Random.Range(0, blockPrefabs.Length);
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
            Debug.Log("Tất cả các khối đã được đặt. Tạo khối mới!");
            SpawnBlocks();
        }
        else
        {
            // If there are pieces remaining, check if they have any valid moves.
            CheckForGameOver();
        }
    }

    void CheckForGameOver()
    {
        // Loop through each piece waiting to be placed
        foreach (var piece in spawnedPieces)
        {
            if (piece == null) continue;

            // Check every single cell on the grid as a potential placement anchor
            for (int x = 0; x < gridManager.width; x++)
            {
                for (int y = 0; y < gridManager.height; y++)
                {
                    // Use the new, unified method in GridManager to check for validity
                    if (gridManager.CanPlaceAt(piece, new Vector2Int(x, y)))
                    {
                        // If we find even one valid spot for one piece, the game is not over.
                        Debug.Log($"Valid move found for {piece.name} at ({x},{y}). Game continues.");
                        return; // Exit the function, game continues
                    }
                }
            }
        }

        // If we get here, the loops completed without finding any valid moves for any pieces.
        Debug.LogError("GAME OVER: No valid moves left for any piece.");
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }
}