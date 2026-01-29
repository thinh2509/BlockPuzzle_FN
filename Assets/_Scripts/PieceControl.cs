using System.Diagnostics;
using UnityEngine;

public class PieceControl : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalScale; // Lưu lại kích thước ban đầu

    void Start()
    {
        // Lưu lại kích thước gốc (thường là 1) khi bắt đầu game
        originalScale = transform.localScale;
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
                offset = transform.position - mousePos;

                // --- ĐOẠN SỬA QUAN TRỌNG ---
                // Tính toán để gạch to ra bằng đúng kích thước thật (Scale = 1)
                // Công thức: 1 / Scale của cha (0.8) = 1.25
                if (transform.parent != null)
                {
                    float targetScale = 1f / transform.parent.localScale.x;
                    transform.localScale = Vector3.one * targetScale;
                }
                // ----------------------------
            }
        }

        // 2. KHI ĐANG GIỮ CHUỘT (Giữ nguyên)
        if (isDragging && Input.GetMouseButton(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePos.x + offset.x, mousePos.y + offset.y, 0);
        }

        // 3. KHI THẢ CHUỘT RA
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            isDragging = false;

            // Trả về kích thước gốc trước khi xử lý logic
            transform.localScale = originalScale;

            // --- THÊM MỚI TỪ ĐÂY ---
            CheckPlacement();
            // -----------------------
        }
    }
    void CheckPlacement()
    {
        // 1. CÔNG THỨC CHUẨN: Làm tròn số nguyên (0, 1, 2...)
        // Giúp toạ độ tính toán luôn KHỚP TÂM với ô lưới của bạn
        Vector3 snapPosition = new Vector3(
            Mathf.Round(transform.position.x),
            Mathf.Round(transform.position.y),
            transform.position.z
        );

        // In ra để kiểm tra xem toạ độ này có đẹp không (VD: 1.0, 2.0 là đẹp)
        UnityEngine.Debug.Log("Vị trí chuẩn sẽ là: " + snapPosition);

        // 2. BỎ CHỨC NĂNG DÍNH (SNAP)
        // Mình đã comment (khóa) dòng này lại theo yêu cầu của bạn.
        // Gạch sẽ KHÔNG tự nhảy vào lưới nữa.

        // transform.position = snapPosition;  <-- Đã tắt dòng này

        // 3. GIỮ NGUYÊN KÍCH THƯỚC TO
        // Đảm bảo gạch không bị bé lại khi thả ra
        if (transform.parent != null)
        {
            float targetScale = 1f / transform.parent.localScale.x;
            transform.localScale = Vector3.one * targetScale;
        }
    }
}