using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring {

    public int id;  //Necessary
    public Vector2 offset;
    public float angle; //Radians
    public List<Point> points;

    public class Point
    {
        public bool valid = true;   //default
        public bool pickedForRANSAC = false;
        public Vector2 pos;
        public float distance;
    }

    public Ring(int ringID)
    {
        id = ringID;
        points = new List<Point>(1080);
    }

    public void AddPoint(float range, float angle)
    {
        angle *= Mathf.Deg2Rad;
        Point pt = new Point();

        pt.pos = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * range;
        points.Add(pt);
    }

    public Vector2 GetPointAligned(int i)
    {
        Vector2 alignedPoint = points[i].pos;

        float s = Mathf.Sin(angle);
        float c = Mathf.Cos(angle);

        float xNew = alignedPoint.x * c - alignedPoint.y * s;
        float yNew = alignedPoint.x * s + alignedPoint.y * c;
        return new Vector2(xNew, yNew) + offset;    //Apply translation, then rotation
    }
}
