using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using DG.Tweening;

public class ShipController : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Drag Settings")]
    [SerializeField] private float dragSmoothing;
    [SerializeField, Tooltip("Speed of ship snapping to parent. The lower it is, the faster it snaps")]
    private float snapSpeed;

    [Header("Parent")]
    [SerializeField] private Transform parentPosition;

    [Header("Placement")]
    [SerializeField] private Tilemap playerbaseTilemap;
    [SerializeField] private BoxCollider2D boardBounds;
    [SerializeField] private int shipSize = 3;
    [SerializeField] private bool isHorizontal = true;
    [SerializeField] private GameObject highlightTilePrefab;

    private bool isDragging;
    private bool isShipPlacedAtTile;

    private List<GameObject> highlightPool = new List<GameObject>();
    private Vector3 offset;
    private Tween moveTween;

    // Global occupied cells (all ships)
    private static HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

    // This ship's occupied cells
    private List<Vector3Int> currentShipOccupiedCells = new List<Vector3Int>();

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isShipPlacedAtTile)
            Debug.Log("Drag Begins");

        Vector3 touchPos = GetWorldPosition(eventData);

        offset = transform.position - touchPos;
        isDragging = true;

        FreeOccupiedCells();
        moveTween?.Kill();
        ClearHighlights();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        Vector3 targetPos = ClampToBoard(GetWorldPosition(eventData) + offset);

        moveTween = transform.DOMove(targetPos, dragSmoothing).SetEase(Ease.Linear);

        if (playerbaseTilemap != null)
        {
            Vector3Int currentCell = GetStartCell(targetPos);
            UpdateHighlights(currentCell);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Ends");

        if (!isDragging)
            return;

        if (playerbaseTilemap == null)
        {
            SnapToParent();
            isDragging = false;
            return;
        }

        Vector3 targetPos = ClampToBoard(GetWorldPosition(eventData) + offset);

        Vector3Int startCell = GetStartCell(targetPos);

        if (CanPlaceShip(startCell, out List<Vector3Int> shipCells))
        {
            PlaceShip(shipCells);
        }
        else
        {
            SnapToParent();
        }

        ClearHighlights();
        isDragging = false;
    }

    private Vector3Int GetStartCell(Vector3 worldPosition)
    {
        Vector3 offsetVector = isHorizontal ? new Vector3((shipSize - 1) * 0.5f, 0, 0) : new Vector3(0, (shipSize - 1) * 0.5f, 0);
        return playerbaseTilemap.WorldToCell(worldPosition - offsetVector);
    }

    bool CanPlaceShip(Vector3Int startCell, out List<Vector3Int> cells)
    {
        cells = new List<Vector3Int>();

        for (int i = 0; i < shipSize; i++)
        {
            Vector3Int cell = startCell +
                (isHorizontal ? new Vector3Int(i, 0, 0) : new Vector3Int(0, i, 0));

            if (playerbaseTilemap.GetTile(cell) == null)
                return false;

            if (occupiedCells.Contains(cell))
                return false;

            cells.Add(cell);
        }

        return true;
    }

    void PlaceShip(List<Vector3Int> cells)
    {
        currentShipOccupiedCells.Clear();

        foreach (var cell in cells)
        {
            occupiedCells.Add(cell);
            currentShipOccupiedCells.Add(cell);
        }

        Vector3 offsetVector = isHorizontal ? new Vector3((shipSize - 1) * 0.5f, 0, 0) : new Vector3(0, (shipSize - 1) * 0.5f, 0);
        Vector3 worldPos = playerbaseTilemap.GetCellCenterWorld(cells[0]) + offsetVector;

        moveTween = transform.DOMove(worldPos, snapSpeed);

        isShipPlacedAtTile = true;
    }

    void FreeOccupiedCells()
    {
        if (currentShipOccupiedCells.Count == 0)
            return;

        foreach (var cell in currentShipOccupiedCells)
            occupiedCells.Remove(cell);

        currentShipOccupiedCells.Clear();
    }

    void UpdateHighlights(Vector3Int startCell)
    {
        ClearHighlights();

        if (CanPlaceShip(startCell, out List<Vector3Int> shipCells))
        {
            for (int i = 0; i < shipCells.Count; i++)
            {
                Vector3Int cell = shipCells[i];
                Vector3 worldPos = playerbaseTilemap.GetCellCenterWorld(cell);

                GameObject highlight = GetHighlight(i);
                highlight.transform.position = worldPos;
                highlight.SetActive(true);
            }
        }
    }

    GameObject GetHighlight(int index)
    {
        if (index >= highlightPool.Count)
        {
            GameObject obj = Instantiate(highlightTilePrefab);
            highlightPool.Add(obj);
        }
        return highlightPool[index];
    }

    void ClearHighlights()
    {
        foreach (var highlight in highlightPool)
        {
            highlight.SetActive(false);
        }
    }

    public void SnapToParent()
    {
        moveTween = transform.DOMove(parentPosition.position, snapSpeed);
        isShipPlacedAtTile = false;
    }

    private Vector3 GetWorldPosition(PointerEventData eventData)
    {
        Vector3 screenPos = eventData.position;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    Vector3 ClampToBoard(Vector3 position)
    {
        if (!isShipPlacedAtTile || boardBounds == null)
            return position;

        Bounds bounds = boardBounds.bounds;

        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Vector3 extents = sr.bounds.extents;

        position.x = Mathf.Clamp(position.x, bounds.min.x + extents.x, bounds.max.x - extents.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y + extents.y, bounds.max.y - extents.y);

        return position;
    }
}
