using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.UI;
using System.Threading;
using System.Threading.Tasks;

public class PointCloudManager : MonoBehaviour {

	public string pathWithoutExtension;
	public Material matVertex;

	private GameObject pointCloud;
    LineManager lm;

	public float scale = 1;
	public bool invertYZ = false;
	public bool forceReload = false;

	private int pointLimit = 65000; //TODO: no longer a factor???. Unity limitation of number of points in a Mesh.

    List<Ring> rings;
    int numPoints = 0;
    private Vector3[] points;
	private Color[] colours;
	private Vector3 minValue;

    [SerializeField]
    Text text;
    [SerializeField]
    Text descriptionText;
    [SerializeField]
    Material lineMat;

    void Start()
    {
        LoadLines();
        LoadPointCloud();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            lm.ToggleVisibility();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            pointCloud.SetActive(!pointCloud.activeInHierarchy);
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Vector3 pos = Camera.main.transform.position;
            float d = lm.CalculateDistance(pos.x, pos.y, pos.z / 30.0f);
            print(d);
        }
    }

    void LoadPointCloud()
    {
        StartCoroutine(LoadPoints(pathWithoutExtension + ".txt"));
    }

    void LoadLines()
    {
        lm = new LineManager();
        lm.LoadLines(pathWithoutExtension);
    }

    IEnumerator LoadPoints(string path)
    {
        var file = new StreamReader(path);

        string line;

        descriptionText.text = "Loaded points";
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

            if (numPoints % 30000 == 0)
            {
                text.text = numPoints.ToString();
                yield return null;
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

        //For some reason using a loop skips the first one, and the debugger breaks???
        Thread t1 = new Thread(() => ThreadedDistanceCalculation(0, ringsPerThread));
        t1.Start();
        threads.Add(t1);
        Thread t2 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread, ringsPerThread * 2));
        t2.Start();
        threads.Add(t2);
        Thread t3 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 2, ringsPerThread * 3));
        t3.Start();
        threads.Add(t3);
        Thread t4 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 3, ringsPerThread * 4));
        t4.Start();
        threads.Add(t4);
        Thread t5 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 4, ringsPerThread * 5));
        t5.Start();
        threads.Add(t5);
        Thread t6 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 5, ringsPerThread * 6));
        t6.Start();
        threads.Add(t6);
        Thread t7 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 6, ringsPerThread * 7));
        t7.Start();
        threads.Add(t7);
        Thread t8 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 7, ringsPerThread * 8));
        t8.Start();
        threads.Add(t8);
        Thread t9 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 8, ringsPerThread * 9));
        t9.Start();
        threads.Add(t9);
        Thread t10 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 9, ringsPerThread * 10));
        t10.Start();
        threads.Add(t10);
        Thread t11 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 10, ringsPerThread * 11));
        t11.Start();
        threads.Add(t11);
        Thread t12 = new Thread(() => ThreadedDistanceCalculation(ringsPerThread * 11, rings.Count));
        t12.Start();
        threads.Add(t12);

        for (int i = 0; i < threads.Count; i++)
        {
            threads[i].Join();
        }

        float longestDistance = 200;
        //Colour the points and fill the position array
        pointNum = 0;
        for (int i = 0; i < rings.Count; i++)
        {
            for (int j = 0; j < rings[i].points.Count; j++)
            {
                if (rings[i].points[j].valid)
                {
                    float s = rings[i].points[j].distance / longestDistance;
                    colours[pointNum] = Color.HSVToRGB(1, s, 1);
                    Vector2 pt = rings[i].points[j].pos;
                    points[pointNum] = new Vector3(pt.x, pt.y, i * 30);
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

        descriptionText.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        lm.PlotLines(lineMat, rings.Count);
    }

    void ThreadedDistanceCalculation(int startIndex, int endIndex)
    {
        for (int i = startIndex; i < endIndex; i++)
        {
            for (int j = 0; j < rings[i].points.Count; j++)
            {
                if (rings[i].points[j].valid)
                {
                    Vector2 pt = rings[i].GetPointWithOffset(j);
                    rings[i].points[j].distance = lm.CalculateDistance(pt.x, pt.y, rings[i].id);    //Note: *1 here because the linefitting algorithm had a 1:1 tie between t and z.;
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

		pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh (meshInd, nPoints, pointLimit);
		pointGroup.transform.parent = pointCloud.transform;
	}

	Mesh CreateMesh(int id, int nPoints, int limitPoints)
    {	
		Mesh mesh = new Mesh();
		
		Vector3[] myPoints = new Vector3[nPoints]; 
		int[] indecies = new int[nPoints];
		Color[] myColors = new Color[nPoints];

		for (int i=0;i<nPoints;++i)
        {
			myPoints[i] = points[id*limitPoints + i] - minValue;
			indecies[i] = i;
			myColors[i] = colours[id*limitPoints + i];
		}


		mesh.vertices = myPoints;
		mesh.colors = myColors;
		mesh.SetIndices(indecies, MeshTopology.Points,0);
		mesh.uv = new Vector2[nPoints];
		mesh.normals = new Vector3[nPoints];

		return mesh;
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
        StreamReader sr = new StreamReader(pathWithoutExtension + "_adjustments.txt");

        int currentRing = 0;
        string line = sr.ReadLine();
        while (line != null)
        {
            float xAdjust = (float) Convert.ToDouble(sr.ReadLine());
            float yAdjust = (float) Convert.ToDouble(sr.ReadLine());
            rings[currentRing].offset = new Vector2(xAdjust, yAdjust);

            line = sr.ReadLine();
            while (line != "RING" && line != null)
            {
                int pointID = Convert.ToInt32(line);
                rings[currentRing].points[pointID].valid = false;
                numPoints--;
                line = sr.ReadLine();
            }

            currentRing++;
        }

        sr.Close();
    }
}
