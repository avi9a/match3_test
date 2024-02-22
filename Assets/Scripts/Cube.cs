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
    private bool isUpdating;
    private Image image;

    public void Initialize(int v, Point point, Sprite piece)
    {
        // LoadData();
        image = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        value = v;
        SetIndex(point);
        image.sprite = piece;
    }

    public void SetIndex(Point point)
    {
        index = point; //new Point(2, 2);
        ResetPosition();
        UpdateName();
    }

    public void ResetPosition()
    {
        position = new Vector2(50 + (64 * index.x), -50 - (64 * index.y));
    }

    private void UpdateName()
    {
        transform.name = "GameBoard [" + index.x + ", " + index.y + "]";
    }

    // public void MovePosition(Vector2 move)
    // {
    //     rect.anchoredPosition += move * Time.deltaTime * 10f;
    // }
    
    public void MovePositionTo(Vector2 move)
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, move, Time.deltaTime * 10f);
    }

    public bool UpdateBlock()
    {
        if (Vector3.Distance(rect.anchoredPosition, position) > 1)
        {
            MovePositionTo(position);
            isUpdating = true;
            SaveData();
            // SaveSystem.SaveBlock(this);
            return true;
        }
        else
        {
            rect.anchoredPosition = position;
            isUpdating = false;
            return false;
        }
    }
    
    public void SaveData()
    {
        PlayerPrefs.SetInt("IndexX", index.x);
        PlayerPrefs.SetInt("IndexY", index.y);
        Debug.Log("Save indexes " + index.x + " " + index.y);
        // PlayerPrefs.SetInt("PositionX", (int)position.x);
        // PlayerPrefs.SetInt("PositionY", (int)position.y);
        // Debug.Log("Save position " + position);
        PlayerPrefs.Save();
    }

    public void LoadData(int x, int y)
    {
        x = index.x;
        y = index.y;
        index.x =  PlayerPrefs.GetInt("IndexX");
        index.y =  PlayerPrefs.GetInt("IndexY");
        Debug.Log("Load indexes " + index.x + " " + index.y);
        // var blockPosition = position;
        // blockPosition.x =  PlayerPrefs.GetInt("PositionX");
        // blockPosition.y =  PlayerPrefs.GetInt("PositionY");
        // position = blockPosition;
        // Debug.Log("Load position " + blockPosition);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (isUpdating) return;
        Move.Instance.MoveBlock(this);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Move.Instance.DropBlock();
    }
}
