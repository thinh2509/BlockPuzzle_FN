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
        
        gridManager = FindFirstObjectByType<GridManager>();
        pieceSpawner = FindFirstObjectByType<PieceSpawner>();
    }

    void Update()
    {
        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsGameOver)
            return;

        if (Camera.main == null)
            return;
        

        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

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
            mousePos.z = 0f;
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, 0);
        }

        
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CheckPlacement();
        }
    }

 

    private void CheckPlacement()
    {
        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsGameOver)
            return;


        if (ChallengeManager.Instance != null && ChallengeManager.Instance.IsGameOver)
            return;

        if (gridManager == null) return;

        Vector2Int anchor = gridManager.GetNearestGridPos(transform.position);
        bool canPlace = gridManager.CanPlacePieceAt(transform, anchor);

        if (canPlace)
        {
            transform.position = gridManager.GridToWorld(anchor);
            transform.localScale = originalScale;

            int placedBlockCount = transform.childCount;

            bool placed = gridManager.PlacePiece(transform);

            if (placed)
            {
                if (ScoreManager.Instance != null)
                    ScoreManager.Instance.AddPoints(placedBlockCount * 10);

                if (pieceSpawner != null)
                    pieceSpawner.OnPiecePlaced(gameObject);
            }
            else
            {
                transform.position = originalPosition;
                transform.localScale = originalScale;
            }
        }
        else
        {
            transform.position = originalPosition;
            transform.localScale = originalScale;
        }
    }
}