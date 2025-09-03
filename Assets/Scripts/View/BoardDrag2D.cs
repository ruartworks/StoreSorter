
using UnityEngine;

// Drag manager: while dragging, hide source slot's renderer to avoid visual duplication.
public class BoardDrag2D : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private BoardController board;

    private bool dragging;
    private int fromCell = -1, fromSlot = -1;
    private SpriteRenderer dragGhost;
    private SpriteRenderer sourceRenderer; // to hide while dragging

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
            TryBeginDrag();
        else if (dragging && Input.GetMouseButton(0))
            UpdateDrag();
        else if (dragging && Input.GetMouseButtonUp(0))
            EndDrag();
    }

    void TryBeginDrag()
    {
        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(wp, Vector2.zero, 0.01f);
        if (!hit.collider) return;

        var view = hit.collider.GetComponentInParent<CellView2D>();
        if (!view) return;

        int slot = -1;
        for (int i = 0; i < view.visibleHotspots.Length; i++)
            if (hit.collider == view.visibleHotspots[i]) { slot = i; break; }
        if (slot < 0) return;

        var sr = view.visibleSR[slot];
        if (sr == null || !sr.enabled || sr.sprite == null) return;

        dragging = true;
        fromCell = view.cellIndex; fromSlot = slot;
        sourceRenderer = sr;
        sourceRenderer.enabled = false; // hide source while dragging

        if (!dragGhost)
        {
            var go = new GameObject("DragGhost");
            dragGhost = go.AddComponent<SpriteRenderer>();
            dragGhost.sortingOrder = 1000;
        }
        dragGhost.sprite = sr.sprite;
        dragGhost.enabled = true;
        UpdateDrag();
    }

    void UpdateDrag()
    {
        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        wp.z = 0f;
        if (dragGhost) dragGhost.transform.position = wp;
    }

    void EndDrag()
    {
        dragging = false;
        if (dragGhost) dragGhost.enabled = false;

        Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
        var hit = Physics2D.Raycast(wp, Vector2.zero, 0.01f);

        bool moved = false;
        if (hit.collider)
        {
            var view = hit.collider.GetComponentInParent<CellView2D>();
            if (view)
            {
                int slot = -1;
                for (int i = 0; i < view.visibleHotspots.Length; i++)
                    if (hit.collider == view.visibleHotspots[i]) { slot = i; break; }
                if (slot >= 0)
                {
                    moved = board.TryMoveVisible(fromCell, fromSlot, view.cellIndex, slot);
                }
            }
        }

        if (!moved && sourceRenderer != null)
            sourceRenderer.enabled = true;

        sourceRenderer = null;
        fromCell = -1; fromSlot = -1;
    }
}
