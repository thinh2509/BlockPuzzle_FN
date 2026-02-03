using UnityEngine;

public class PieceControl : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalScale; 
    private Vector3 originalPosition; 
    private GridManager gridManager;
    private PieceSpawner pieceSpawner; 

    void Start()
    {
        
        originalScale = transform.localScale;
        
        gridManager = FindObjectOfType<GridManager>();
        pieceSpawner = FindObjectOfType<PieceSpawner>();
    }

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.transform.parent == transform)
            {
                isDragging = true;
                originalPosition = transform.position;
                offset = transform.position - mousePos;

                
                if (transform.parent != null)
                {
                    float targetScale = 1f / transform.parent.localScale.x;
                    transform.localScale = Vector3.one * targetScale;
                }
            }
        }

        
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, 0);
        }

       
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CheckPlacement();
        }
    }

    void CheckPlacement()
    {
        
        Vector3 snappedPosition = gridManager.GetNearestCellCenter(transform.position);

        bool canPlace = true;
        
        Vector3[] childBlockPositions = new Vector3[transform.childCount];

        
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            Vector3 positionOffset = child.position - transform.position;
            
            childBlockPositions[i] = snappedPosition + positionOffset;
        }

        
        foreach (var blockPos in childBlockPositions)
        {
            
            Vector2Int gridCoords = gridManager.WorldToGrid(blockPos);

            
            if (gridManager.IsCellOccupied(gridCoords.x, gridCoords.y))
            {
                canPlace = false;
                break; 
            }
        }

        
        if (canPlace)
        {
            
            transform.position = snappedPosition;
            
            transform.localScale = originalScale;
            
            
            Transform[] childBlockTransforms = new Transform[transform.childCount];
            for(int i = 0; i < transform.childCount; i++)
            {
                childBlockTransforms[i] = transform.GetChild(i);
                ScoreManager.Instance.AddPoints(10); 
            }
            gridManager.PlacePiece(childBlockTransforms);

            Debug.Log("Piece placed successfully!");

            
            this.enabled = false;

           
            pieceSpawner.OnPiecePlaced(gameObject);

            
            gridManager.CheckForCompletedLines();
        }
        else
        {
            
            transform.position = originalPosition;
            transform.localScale = originalScale; 
            Debug.Log("Invalid placement, returning to start.");
        }
    }
}