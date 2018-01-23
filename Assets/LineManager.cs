using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class LineManager {

    List<Line> lines;

	public void LoadLines(string pathWithoutExtension)
    {
        lines = new List<Line>();
        StreamReader sr = new StreamReader(pathWithoutExtension + "_lines.txt");
        string line;

        while ((line = sr.ReadLine()) != null)
        {
            double xLine = Convert.ToDouble(line);
            double xLineT = Convert.ToDouble(sr.ReadLine());
            double yLine = Convert.ToDouble(sr.ReadLine());
            double yLineT = Convert.ToDouble(sr.ReadLine());
            double zLine = Convert.ToDouble(sr.ReadLine());
            double zLineT = Convert.ToDouble(sr.ReadLine());

            Line l = new Line(xLine, xLineT, yLine, yLineT, zLine, zLineT);
            lines.Add(l);
        }

        sr.Close();
    }

    public void PlotLines(Material lineMat, int rings)
    {
        foreach (Line l in lines)
        {
            l.PlotLine(lineMat, rings);
        }
    }

    public void ToggleVisibility()
    {
        foreach (Line l in lines)
        {
            l.go.SetActive(!l.go.activeInHierarchy);
        }
    }

    public float CalculateDistance(int pointID, float ptX, float ptY, float ptZ)
    {
        float shortestDistance = float.MaxValue;
        int lineStart = pointID - 20;
        if (lineStart < 0)
        {
            lineStart = 0;
        }

        int lineEnd = pointID + 20;
        if (lineEnd > lines.Count)
        {
            lineEnd = lines.Count;
        }
        float qx, qy, shortestQx = 0, shortestQy = 0;

        for (int i = lineStart; i < lineEnd; i++)
        {
            float distance = lines[i].CalculateDistanceFast(ptX, ptY, ptZ, out qx, out qy);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                shortestQx = qx;
                shortestQy = qy;
            }
        }

        float distanceZeroToPointSquared = ptX * ptX + ptY * ptY;   //Distance^2 from 0,0 to point
        float distanceZeroToLineSquared = shortestQx * shortestQx + shortestQy * shortestQy;    //Distance^2 from 0,0 to line

        if (distanceZeroToLineSquared > distanceZeroToPointSquared) //If line is further away than the point, then the point is bulging inside of the tunnel
        {
            shortestDistance *= -1;
        }

        return shortestDistance;
    }
}
