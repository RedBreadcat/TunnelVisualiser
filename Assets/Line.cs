using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra.Single;

public class Line {

    float xLine, xLineT;   //Intecept, t-coefficient
    float yLine, yLineT;
    float zLine, zLineT;

    public GameObject go;

    public Line(double xLine, double xLineT, double yLine, double yLineT, double zLine, double zLineT)
    {
        this.xLine = (float)xLine;
        this.xLineT = (float)xLineT;
        this.yLine = (float)yLine;
        this.yLineT = (float)yLineT;
        this.zLine = (float)zLine;
        this.zLineT = (float)zLineT;
    }

    public void PlotLine(Material lineMat, int rings)
    {
        go = new GameObject("Line");
        LineRenderer lr = go.AddComponent<LineRenderer>();
        lr.material = lineMat;
        lr.positionCount = 2;

        Vector3 lineStart = new Vector3((float)xLine, (float)yLine, (float)zLine);  //At t=0, the t coefficient becomes meaningless
        lr.SetPosition(0, lineStart);

        //With the current line fitting algorithm, the ring count is z, which is "t"
        //We *30 for visualisation purposes, which should eventually be removed
        Vector3 lineEnd = new Vector3((float)(xLine + xLineT * rings), (float)(yLine + yLineT * rings), (float)(zLine + zLineT * rings) * 30);
        lr.SetPosition(1, lineEnd);
    }

    //https://math.stackexchange.com/questions/1815397/distance-between-point-and-parametric-line
    public float CalculateDistance(float ptX, float ptY, float ptZ)
    {
        //Line minus point
        float Xm = xLine - ptX;
        float Ym = yLine - ptY;
        float Zm = zLine - ptZ;

        float tCoefficient = xLineT * xLineT + yLineT * yLineT + zLineT * zLineT;
        float rhs = -xLineT * Xm - yLineT * Ym - zLineT * Zm;

        var A = Matrix.Build.DenseOfArray(new float[,] { { tCoefficient }  });
        var b = Vector.Build.Dense(new float[] { rhs });

        var t = A.Solve(b);

        float qx = xLine + xLineT * t[0];
        float qy = yLine + yLineT * t[0];
        float qz = zLine + zLineT * t[0];

        return Mathf.Sqrt(Mathf.Pow(ptX - qx, 2) + Mathf.Pow(ptY - qy, 2) + Mathf.Pow(ptZ - qz, 2));
    }
    
    //Based on assumption that Z is directly tied to t, therefore no solving is necessary
    public float CalculateDistanceSquaredFast(float ptX, float ptY, float ptZ, out float qx, out float qy)
    {
        int t = (int) ptZ;

        qx = xLine + xLineT * t;
        qy = yLine + yLineT * t;

        return (ptX - qx) * (ptX - qx) + (ptY - qy) * (ptY - qy);
    }
}
