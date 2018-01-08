using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.UI;

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
        LoadPointCloud();
        LoadLines();
        lm.PlotLines(lineMat);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            lm.ToggleVisibility();
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
        int numPoints = 0;
        Ring ring = new Ring();
        while ((line = file.ReadLine()) != null)
        {
            string[] elements = line.Split(',');

            int id = Convert.ToInt32(elements[0]);
            float angle = (float)Convert.ToDouble(elements[1]); //Not necessary given that angle is same each time. Steps aren't linear though, so I at least need to think
            float range = (float)Convert.ToDouble(elements[2]);

            /*numPoints += */ring.AddPoint(range, angle);
            numPoints++;
            if (id == 1080)
            {
                rings.Add(ring);
                ring = new Ring();
            }

            if (numPoints % 30000 == 0)
            {
                text.text = numPoints.ToString();
                yield return null;
            }
        }

        file.Close();

        points = new Vector3[numPoints];
        colours = new Color[numPoints];

        descriptionText.text = "Building point cloud";
        int pointNum = 0;
        for (int i = 0; i < rings.Count; i++)
        {
            for (int j = 0; j < rings[i].points.Count; j++)
            {
                if (rings[i].points[j].valid || true)
                {
                    Vector2 pt = rings[i].GetPointWithOffset(j);
                    points[pointNum] = new Vector3(pt.x, pt.y, i * 30);
                    float h = Mathf.Lerp(0, 1, pt.magnitude / 5000f);

                    colours[pointNum] = Color.HSVToRGB(h, 1, 1); //TODO: based on distance from surface

                    pointNum++;
                }
            }

            if (pointNum % 500 == 0)
            {
                text.text = i.ToString();
                yield return null;
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
        yield return null;
    }
	
	void InstantiateMesh(int meshInd, int nPoints){
		// Create Mesh
		GameObject pointGroup = new GameObject(meshInd.ToString());
		pointGroup.AddComponent<MeshFilter>();
		pointGroup.AddComponent<MeshRenderer>();
		pointGroup.GetComponent<Renderer>().material = matVertex;

		pointGroup.GetComponent<MeshFilter>().mesh = CreateMesh (meshInd, nPoints, pointLimit);
		pointGroup.transform.parent = pointCloud.transform;


		// Store Mesh
		//UnityEditor.AssetDatabase.CreateAsset(pointGroup.GetComponent<MeshFilter> ().mesh, "Assets/Resources/PointCloudMeshes/" + filename + @"/" + filename + meshInd + ".asset");
		//UnityEditor.AssetDatabase.SaveAssets ();
		//UnityEditor.AssetDatabase.Refresh();
	}

	Mesh CreateMesh(int id, int nPoints, int limitPoints){
		
		Mesh mesh = new Mesh ();
		
		Vector3[] myPoints = new Vector3[nPoints]; 
		int[] indecies = new int[nPoints];
		Color[] myColors = new Color[nPoints];

		for(int i=0;i<nPoints;++i) {
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
}
