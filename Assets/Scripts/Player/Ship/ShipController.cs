using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

[RequireComponent(typeof(Collider2D))]
public class ShipController : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private float dragSmoothing;
    [SerializeField,Tooltip("Speed of ship snapping to parent. The lower it is, the faster it snaps")] private float snapSpeed;
    [SerializeField] private Transform parentPosition;
    [SerializeField] private Transform targetPosition;
    private bool isDragging;
    private Vector3 offset;
    private Tween moveTween;

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Begins");
        Vector3 touchPos = GetWorldPosition(eventData);

        Collider2D hit = Physics2D.OverlapPoint(touchPos);
        if (hit == null || hit != GetComponent<Collider2D>())
            return;

        offset = transform.position - touchPos;
        isDragging = true;

        moveTween?.Kill();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
        Debug.Log("Dragging the ship");
        Vector3 targetPos = GetWorldPosition(eventData) + offset;

        moveTween = transform.DOMove(targetPos, dragSmoothing).SetEase(Ease.Linear);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("Drag Ends");
        if (!isDragging)
            return;
        if (transform.position != targetPosition.position)
            SnapToParent();

        isDragging = false;
        // TODO
        // ValidatePlacement();
    }
    public void SnapToParent()
    {
        moveTween = transform.DOMove(parentPosition.position, snapSpeed);
        Debug.Log("Ship snapped back to parent");
    }

    private Vector3 GetWorldPosition(PointerEventData eventData)
    {
        Vector3 screenPos = eventData.position;
        screenPos.z = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(screenPos);
    }
}
