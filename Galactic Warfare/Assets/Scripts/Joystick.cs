using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    public Vector2 axisValue; // -1 to 1 like Input.GetAxis
    RectTransform rt;
    Vector2 originalAnchored;

    private float radius;

    void Start()
    {
        rt = GetComponent<RectTransform>();
        originalAnchored = rt.anchoredPosition;

        // Set radius to half of the joystick background size (assumes parent is the background)
        RectTransform parent = rt.parent.GetComponent<RectTransform>();
        radius = parent.rect.width / 2f;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );

        Vector2 offset = localPoint - originalAnchored;
        offset = Vector2.ClampMagnitude(offset, radius);
        rt.anchoredPosition = offset;

        axisValue = offset / radius; // Normalized value from -1 to 1
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        rt.anchoredPosition = Vector2.zero;
        axisValue = Vector2.zero;
    }
}

