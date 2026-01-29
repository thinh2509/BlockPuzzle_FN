using UnityEngine;

public class PieceControl : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalScale; // Lưu lại kích thước ban đầu
    private Vector3 originalPosition; // Lưu vị trí trước khi kéo
    private GridManager gridManager;
    private PieceSpawner pieceSpawner; // Thêm tham chiếu đến PieceSpawner

    void Start()
    {
        // Lưu lại kích thước gốc
        originalScale = transform.localScale;
        // Tìm các manager trong Scene
        gridManager = FindObjectOfType<GridManager>();
        pieceSpawner = FindObjectOfType<PieceSpawner>();
    }

    void Update()
    {
        // 1. KHI BẤM CHUỘT XUỐNG
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.transform.parent == transform)
            {
                isDragging = true;
                originalPosition = transform.position; // Lưu vị trí gốc
                offset = transform.position - mousePos;

                // Phóng to gạch khi kéo
                if (transform.parent != null)
                {
                    float targetScale = 1f / transform.parent.localScale.x;
                    transform.localScale = Vector3.one * targetScale;
                }
            }
        }

        // 2. KHI ĐANG GIỮ CHUỘT
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, 0);
        }

        // 3. KHI THẢ CHUỘT RA
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            CheckPlacement();
        }
    }

    void CheckPlacement()
    {
        // Vị trí snap tiềm năng cho tâm của khối gạch
        Vector3 snappedPosition = gridManager.GetNearestCellCenter(transform.position);

        bool canPlace = true;
        // Tạo một mảng để lưu vị trí tương lai của các ô vuông con
        Vector3[] childBlockPositions = new Vector3[transform.childCount];

        // 1. Tính toán vị trí tương lai của tất cả các ô vuông con
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            // Tính độ dời của ô con so với tâm
            Vector3 positionOffset = child.position - transform.position;
            // Vị trí tương lai của ô con = vị trí snap của tâm + độ dời
            childBlockPositions[i] = snappedPosition + positionOffset;
        }

        // 2. Kiểm tra xem tất cả các vị trí tương lai đó có hợp lệ không
        foreach (var blockPos in childBlockPositions)
        {
            // Chuyển vị trí thế giới thành tọa độ grid
            Vector2Int gridCoords = gridManager.WorldToGrid(blockPos);

            // Kiểm tra xem có nằm ngoài grid hoặc đã bị chiếm chưa
            if (gridManager.IsCellOccupied(gridCoords.x, gridCoords.y))
            {
                canPlace = false;
                break; // Nếu 1 ô không hợp lệ, dừng kiểm tra ngay
            }
        }

        // 3. Quyết định đặt gạch hoặc trả về
        if (canPlace)
        {
            // Nếu hợp lệ, di chuyển khối gạch đến vị trí snap
            transform.position = snappedPosition;
            // Trả về kích thước ban đầu
            transform.localScale = originalScale;
            
            // Báo cho GridManager biết để đánh dấu các ô này là đã bị chiếm
            // [NÂNG CẤP] Truyền mảng Transform của các ô vuông nhỏ
            Transform[] childBlockTransforms = new Transform[transform.childCount];
            for(int i = 0; i < transform.childCount; i++)
            {
                childBlockTransforms[i] = transform.GetChild(i);
            }
            gridManager.PlacePiece(childBlockTransforms);

            Debug.Log("Piece placed successfully!");

            // Vô hiệu hóa script này để không kéo được nữa
            this.enabled = false;

            // BÁO CHO PIECESPAWNER BIẾT KHỐI NÀY ĐÃ ĐƯỢC ĐẶT
            pieceSpawner.OnPiecePlaced(gameObject);

            // [MỚI] KIỂM TRA VÀ XÓA CÁC HÀNG ĐÃ ĐẦY
            gridManager.CheckForCompletedLines();
        }
        else
        {
            // Nếu không hợp lệ, trả khối gạch về vị trí ban đầu
            transform.position = originalPosition;
            transform.localScale = originalScale; // Vẫn phải trả về kích thước cũ
            Debug.Log("Invalid placement, returning to start.");
        }
    }
}