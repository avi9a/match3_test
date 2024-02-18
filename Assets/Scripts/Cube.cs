using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Cube : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public int value;
    public Point index;

    [HideInInspector] public Vector2 position;
    [HideInInspector] public RectTransform rect;
    [HideInInspector] public Cube flipped;
    private bool isUpdating;
    private Image image;

    public void Initialize(int v, Point point, Sprite piece)
    {
        flipped = null;
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        value = v;
        SetIndex(point);
        image.sprite = piece;
    }

    public void SetIndex(Point point)
    {
        index = point;
        ResetPosition();
        UpdateName();
    }

    public void ResetPosition()
    {
        position = new Vector2(-120 + (64 * index.x), -170 - (64 * index.y));
    }

    private void UpdateName()
    {
        transform.name = "GameBoard [" + index.x + ", " + index.y + "]";
    }

    public void MovePosition(Vector2 move)
    {
        rect.anchoredPosition += move * Time.deltaTime * 1f;
    }
    
    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition += Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 1f);
    }

    public bool UpdateBlock()
    {
        if (Vector3.Distance(rect.anchoredPosition, position) > 1)
        {
            MovePositionTo(position);
            isUpdating = true;
            return true;
        }
        else
        {
            rect.anchoredPosition = position;
            isUpdating = false;
            return false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUpdating) return;
        Move.instance.MoveBlock(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Move.instance.DropBlock();
    }
}
