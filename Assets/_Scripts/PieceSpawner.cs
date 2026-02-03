using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [Header("Cài đặt Gạch")]
    public GameObject[] blockPrefabs; 
    public Transform[] spawnPoints;   

    
    private List<GameObject> spawnedPieces = new List<GameObject>();

    void Start()
    {
        SpawnBlocks();
    }

    public void SpawnBlocks()
    {
        
        spawnedPieces.Clear();

       
        foreach (Transform point in spawnPoints)
        {
            
            int randomIndex = Random.Range(0, blockPrefabs.Length);
            GameObject randomBlockPrefab = blockPrefabs[randomIndex];

            
            GameObject newPiece = Instantiate(randomBlockPrefab, point.position, Quaternion.identity);

            
            spawnedPieces.Add(newPiece);
        }
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
    }
}