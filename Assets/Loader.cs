using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class Loader : MonoBehaviour {

    [SerializeField]
    Mesh sphere;
    [SerializeField]
    Material mat;

    List<Matrix4x4[]> matrices;

    List<Ring> rings;



	void Start()
    {
        var file = new StreamReader(@"C:\Users\Roby\Downloads\yarraValley_scan_1.txt");

        string line;

        rings = new List<Ring>();
        int i = 0;
        Ring ring = new Ring();
        while ((line = file.ReadLine()) != null)
        {
            string[] elements = line.Split(',');

            int id = Convert.ToInt32(elements[0]);
            float angle = (float)Convert.ToDouble(elements[1]); //Not necessary given that angle is same each time. Steps aren't linear though, so I at least need to think
            float range = (float)Convert.ToDouble(elements[2]);

            ring.AddPoint(range, angle);

            if (id == 1080)
            {
                rings.Add(ring);
                /*if (rings.Count == 20)
                {
                    break;
                }*/
                i++;
                ring = new Ring();
            }
        }

        file.Close();

        for (i = 0; i < rings.Count - 1; i++)
        {
            MinimiseError(i, i + 1);
        }

        matrices = new List<Matrix4x4[]>();
        Matrix4x4[] mArray = new Matrix4x4[1023];
        int currentElement = 0;
        for (i = 0; i < rings.Count; i++)
        {
            for (int j = 0; j < rings[i].points.Count; j++)
            {
                if (rings[i].points[j].valid)
                {
                    Vector2 pt2d = rings[i].GetPointWithOffset(j);
                    Vector3 pt = new Vector3(pt2d.x, pt2d.y, i * 100);
                    mArray[currentElement++] = Matrix4x4.TRS(pt, Quaternion.identity, new Vector3(10, 10, 10));

                    if (currentElement == 1023)
                    {
                        currentElement = 0;
                        matrices.Add(mArray);
                        mArray = new Matrix4x4[1023];
                    }
                }
            }
        }
	}

    private void Update()
    {
        for (int i = 0; i < matrices.Count; i++)
        {
            Graphics.DrawMeshInstanced(sphere, 0, mat, matrices[i]);
        }
    }

    void MinimiseError(int a, int b)
    {
        Vector2 difference = Vector2.zero;
        int validPointsTested = 0;
        for (int i = 0; i < 1080; i++)
        {
            if (rings[a].points[i].valid && rings[b].points[i].valid)
            {
                difference += rings[b].points[i].pos - rings[a].GetPointWithOffset(i);
                validPointsTested++;
            }
        }
        difference = difference / validPointsTested;
        //rings[b].offset = difference;
        rings[b].offset = Vector2.zero;

        //print(difference);
    }

    void MakeLine(int id)
    {
        for (int i = 0; i < rings.Count; i++)
        {

        }
    }
}
