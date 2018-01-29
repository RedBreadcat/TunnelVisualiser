using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System.Threading;
using SFB;

public class PointCloudManager : MonoBehaviour {

    public static PointCloudManager pcm;
    public Material matVertex;

    [HideInInspector]
    public GameObject pointCloud;
    public LineManager lm;

    public float zSpacing = 30;

    private int pointLimit = 65000; //TODO: no longer a factor???. Unity limitation of number of points in a Mesh.

    [HideInInspector]
    public List<Ring> rings;
    int numPoints = 0;
    private Vector3[] points;
    private Color[] colours;
    private Vector3 minValue;

    [SerializeField]
    Material lineMat;
    [SerializeField]
    DiameterCalculator diameterCalculator;

    bool loaded = false;

    string tunnelPath, linePath, adjustmentPath;

    private void Awake()
    {
        pcm = this;    
    }

    public void Construct()
    {
        try
        {
            CameraController.ShowCursor();
            string[] files1 = StandaloneFileBrowser.OpenFilePanel("Select the tunnel scan file", "", "txt", false);
            tunnelPath = files1[0];

            adjustmentPath = tunnelPath.Remove(tunnelPath.Length - 4) + "_adjustments.txt"; ;    //Remove the .txt from the path, and add rest of path
            if (!File.Exists(adjustmentPath))  //If adjustments doesn't exist automatically, enter it manually
            {
                string[] files2 = StandaloneFileBrowser.OpenFilePanel("Select the adjustments file", "", "txt", false);
                adjustmentPath = files2[0];

            }
            string[] files3 = StandaloneFileBrowser.OpenFilePanel("Select the lines file", "", "txt", false);
            linePath = files3[0];

            CameraController.HideCursor();

            LoadLines();
            LoadPointCloud();
            diameterCalculator.gameObject.SetActive(true);
        }
        catch
        {
            DestroyEverything();
        }
    }

    private void Update()
    {
        if (loaded)
        {
            if (Input.GetKeyDown(KeyCode.L))
            {
                lm.ToggleVisibility();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                pointCloud.SetActive(!pointCloud.activeInHierarchy);
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                CameraController.cam.ToggleFreeMode();
            }
        }
    }

    void LoadPointCloud()
    {
        try
        {
            var file = new StreamReader(tunnelPath);

            string line;
            rings = new List<Ring>();
            int ringID = 0;
            Ring ring = new Ring(ringID);
            while ((line = file.ReadLine()) != null)
            {
                string[] elements = line.Split(',');

                int id = Convert.ToInt32(elements[0]);
                float angle = (float)Convert.ToDouble(elements[1]); //Not necessary given that angle is same each time. Steps aren't linear though, so I at least need to think
                float range = (float)Convert.ToDouble(elements[2]);

                ring.AddPoint(range, angle);
                numPoints++;
                if (id == 1080)
                {
                    rings.Add(ring);
                    ring = new Ring(++ringID);
                }
            }

            file.Close();

            LoadAdjustments();

            points = new Vector3[numPoints];
            colours = new Color[numPoints];

            int pointNum = 0;

            int threadCount = Environment.ProcessorCount;
            var threads = new List<Thread>();
            int ringsPerThread = rings.Count / threadCount;

            for (int i = 0; i < 11; i++)
            {
                int temp = i;   //Must be done because of "closures" and "captured variables" https://stackoverflow.com/questions/26631939/captured-variables-in-a-thread-in-a-loop-in-c-what-is-the-solution
                Thread t = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * temp, ringsPerThread * (temp + 1)));
                t.Start();
                threads.Add(t);
            }
            Thread t12 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 11, rings.Count));
            t12.Start();
            threads.Add(t12);

            for (int i = 0; i < threads.Count; i++)
            {
                threads[i].Join();
            }

            float longestDistance = 400;
            //Colour the points and fill the position array
            pointNum = 0;
            for (int i = 0; i < rings.Count; i++)
            {
                for (int j = 0; j < rings[i].points.Count; j++)
                {
                    if (rings[i].points[j].valid)
                    {
                        float distanceWithDeadZone = Mathf.Abs(rings[i].points[j].distance) - 80;
                        if (distanceWithDeadZone < 0)
                        {
                            distanceWithDeadZone = 0;
                        }
                        float s = distanceWithDeadZone / longestDistance;
                        float h;
                        if (rings[i].points[j].distance > 0)    //Positive distance indicates the point is bulging out
                        {
                            h = 0.65f; //blue
                        }
                        else
                        {
                            h = 1;  //red
                        }
                        colours[pointNum] = Color.HSVToRGB(h, s, 1);

                        Vector2 pt = rings[i].GetPointAligned(j);
                        points[pointNum] = new Vector3(pt.x, pt.y, i * zSpacing);
                        pointNum++;
                    }
                }
            }

            //Instantiate Point Groups
            int numPointGroups = Mathf.CeilToInt(numPoints * 1.0f / pointLimit * 1.0f);

            pointCloud = new GameObject("Point cloud");

            for (int i = 0; i < numPointGroups - 1; i++)
            {
                InstantiateMesh(i, pointLimit);
            }
            InstantiateMesh(numPointGroups - 1, numPoints - (numPointGroups - 1) * pointLimit);

            MenuControl.mc.CloudReady();
            lm.PlotLines(lineMat, rings.Count);
            lm.ToggleVisibility();
            loaded = true;
        }
        catch
        {
            DestroyEverything();
        }
    }

    void ThreadedDistanceCalculation(int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            for (int j = 0; j < rings[i].points.Count; j++)
            {
                if (rings[i].points[j].valid)
                {
                    Vector2 pt = rings[i].GetPointAligned(j);
                    rings[i].points[j].distance = lm.CalculateDistance(j, pt.x, pt.y, rings[i].id);    //Note: *1 here because the linefitting algorithm had a 1:1 tie between t and z.;
                }
            }
        }
    }

    void InstantiateMesh(int meshInd, int nPoints)
    {
        GameObject pointGroup = new GameObject(meshInd.ToString());
        pointGroup.AddComponent<MeshFilter>();
        pointGroup.AddComponent<MeshRenderer>();
        pointGroup.GetComponent<Renderer>().material = matVertex;

        pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh(meshInd, nPoints, pointLimit);
        pointGroup.transform.parent = pointCloud.transform;
    }

    Mesh CreateMesh(int id, int nPoints, int limitPoints)
    {
        Mesh mesh = new Mesh();

        Vector3[] myPoints = new Vector3[nPoints];
        int[] indices = new int[nPoints];
        Color[] myColors = new Color[nPoints];

        for (int i = 0; i < nPoints; ++i)
        {
            myPoints[i] = points[id * limitPoints + i] - minValue;
            indices[i] = i;
            myColors[i] = colours[id * limitPoints + i];
        }

        mesh.vertices = myPoints;
        mesh.colors = myColors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        mesh.uv = new Vector2[nPoints];
        mesh.normals = new Vector3[nPoints];

        return mesh;
    }

    void LoadLines()
    {
        lm = new LineManager();
        lm.LoadLines(linePath);
    }

	void CalculateMin(Vector3 point)
    {
		if (minValue.magnitude == 0)
			minValue = point;

		if (point.x < minValue.x)
			minValue.x = point.x;
		if (point.y < minValue.y)
			minValue.y = point.y;
		if (point.z < minValue.z)
			minValue.z = point.z;
	}

    void LoadAdjustments()
    {
        StreamReader sr = new StreamReader(adjustmentPath);

        int currentRing = 0;
        string line = sr.ReadLine();
        while (line != null)
        {
            float xAdjust = (float) Convert.ToDouble(sr.ReadLine());
            float yAdjust = (float) Convert.ToDouble(sr.ReadLine());
            rings[currentRing].offset = new Vector2(xAdjust, yAdjust);
            rings[currentRing].angle = (float)Convert.ToDouble(sr.ReadLine());

            line = sr.ReadLine();
            while (line != "RANSAC")             //while (line != "RING" && line != null)
            {
                int pointID = Convert.ToInt32(line);
                rings[currentRing].points[pointID].valid = false;
                numPoints--;
                line = sr.ReadLine();
            }

            line = sr.ReadLine();
            while (line != "RING" && line != null)
            {
                int pointID = Convert.ToInt32(line);
                rings[currentRing].points[pointID].pickedForRANSAC = true;
                line = sr.ReadLine();
            }

            currentRing++;
        }

        sr.Close();
    }

    void DestroyEverything()
    {
        SceneManager.LoadScene(0);
        CameraController.ShowCursor();  //So load button can be clicked
    }
}
