using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiameterCalculator : MonoBehaviour {

    [SerializeField]
    LineRenderer lr;
    [SerializeField]
    LineRenderer lrExtend1;
    [SerializeField]
    LineRenderer lrExtend2;

    [SerializeField]
    Text text;

    float nextCalc;

    int ring = 0;
    int lastRing = -1;

    float CalculateDiameter(int ring)
    {
        float longestDistance = float.MinValue;
        int iLongest = 0;
        int jLongest = 0;

        for (int i = 0; i < PointCloudManager.pcm.rings[ring].points.Count - 1; i+= 10)
        {
            if (PointCloudManager.pcm.rings[ring].points[i].valid)
            {
                for (int j = 1; j < PointCloudManager.pcm.rings[ring].points.Count; j += 10)
                {
                    if (PointCloudManager.pcm.rings[ring].points[j].valid)
                    {
                        float distance = Vector2.Distance(PointCloudManager.pcm.rings[ring].GetPointAligned(i), PointCloudManager.pcm.rings[ring].GetPointAligned(j));

                        if (distance > longestDistance)
                        {
                            longestDistance = distance;
                            iLongest = i;
                            jLongest = j;
                        }
                    }
                }
            }
        }

        Vector3 start = PointCloudManager.pcm.rings[ring].GetPointAligned3D(iLongest);
        Vector3 end = PointCloudManager.pcm.rings[ring].GetPointAligned3D(jLongest);
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        return longestDistance;
    }

    private void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetKey(KeyCode.LeftShift))
        {
            scroll *= 10;
        }

        if (Mathf.Abs(scroll) > 0)
        {
            text.text = "Calculating diameter";
            ring += (int)(scroll * 40);
            ring = Mathf.Clamp(ring, 0, PointCloudManager.pcm.rings.Count);
            lrExtend1.SetPosition(0, new Vector3(-4000, 0, ring * PointCloudManager.pcm.zSpacing));
            lrExtend1.SetPosition(1, new Vector3(4000, 0, ring * PointCloudManager.pcm.zSpacing));
            lrExtend2.SetPosition(0, new Vector3(0, -4000, ring * PointCloudManager.pcm.zSpacing));
            lrExtend2.SetPosition(1, new Vector3(0, 4000, ring * PointCloudManager.pcm.zSpacing));
            lr.enabled = false;
        }

        if (Time.time > nextCalc && lastRing != ring)
        {
            text.text = "Diameter: " + (int)CalculateDiameter(ring) + "mm";
            lastRing = ring;
            nextCalc = Time.time + 3;
            lr.enabled = true;
        }
    }
}
