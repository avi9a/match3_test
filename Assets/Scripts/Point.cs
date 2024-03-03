using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Point
{
    public int x;
    public int y;

    public Point(int newX, int newY)
    {
        x = newX;
        y = newY;
    }
    
    public void Mult(int m)
    {
        x *= m;
        y *= m;
    }
    
    public void Add(Point point)
    {
        x += point.x;
        y += point.y;
    }

    public Vector2 ToVector()
    {
        return new Vector2(x, y);
    }

    public bool Equals(Point point)
    {
        return (x == point.x && y == point.y);
    }

    public static Point FromVector(Vector2 vector)
    {
        return new Point((int)vector.x, (int)vector.y);
    }
    
    public static Point FromVector(Vector3 vector)
    {
        return new Point((int)vector.x, (int)vector.y);
    }
    
    public static Point Mult(Point point, int m)
    {
        return new Point(point.x * m, point.y * m);
    }

    public static Point Add(Point point, Point o)
    {
        return new Point(point.x + o.x, point.y + o.y);
    }
    
    public static Point Clone(Point point)
    {
        return new Point(point.x, point.y);
    }

    public static Point Zero
    {
        get { return new Point(0, 0); }
    }
    
    public static Point Up
    {
        get { return new Point(0, 1); }
    }
    
    public static Point Down
    {
        get { return new Point(0, -1); }
    }
    
    public static Point Right
    {
        get { return new Point(1, 0); }
    }
    
    public static Point Left
    {
        get { return new Point(-1, 0); }
    }
}
