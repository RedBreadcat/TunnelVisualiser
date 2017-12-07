using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring {

    public int id;
    public Vector2 offset;
    public List<Point> points;

    public struct Point
    {
        public bool valid;
        public Vector2 pos;
    }

    public Ring()
    {
        points = new List<Point>(1080);
    }

    public int AddPoint(float range, float angle)
    {
        angle *= Mathf.Deg2Rad;
        Point pt;

        pt.pos = new Vector2((float)Mathf.Sin(angle), (float)Mathf.Cos(angle)) * range;
        pt.valid = range < 3600 && range > 1800;
        points.Add(pt);
        return pt.valid ? 1 : 0;
    }

    public Vector2 GetPointWithOffset(int i)
    {
        return points[i].pos - offset;
    }

    //can get 3D point by using ID
}
