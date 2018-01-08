using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Line {

    static float tEnd = 60000;

    double xLine, xLineT;   //Intecept, t-coefficient
    double yLine, yLineT;
    double zLine, zLineT;

    public GameObject go;

    public Line(double xLine, double xLineT, double yLine, double yLineT, double zLine, double zLineT)
    {
        this.xLine = xLine;
        this.xLineT = xLineT;
        this.yLine = yLine;
        this.yLineT = yLineT;
        this.zLine = zLine;
        this.zLineT = zLineT;
    }

    public void PlotLine(Material lineMat)
    {
        go = new GameObject("Line");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = lineMat;
        lr.positionCount = 2;

        Vector3 lineStart = new Vector3((float)xLine, (float)yLine, (float)zLine);  //At t=0, the t coefficient becomes meaningless
        lr.SetPosition(0, lineStart);
        Vector3 lineEnd = new Vector3((float)(xLine + xLineT * tEnd), (float)(yLine + yLineT * tEnd), (float)(zLine + zLineT * tEnd));
        lr.SetPosition(1, lineEnd);
    }
}
