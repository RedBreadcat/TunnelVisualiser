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

    public void PlotLines(Material lineMat)
    {
        foreach (Line l in lines)
        {
            l.PlotLine(lineMat);
        }
    }

    public void ToggleVisibility()
    {
        foreach (Line l in lines)
        {
            l.go.SetActive(!l.go.activeInHierarchy);
        }
    }
}
