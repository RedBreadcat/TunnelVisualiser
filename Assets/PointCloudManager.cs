using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using UnityEngine.UI;

public class PointCloudManager : MonoBehaviour {

	// File
	public string dataPath;
	private string filename;
	public Material matVertex;

	// PointCloud
	private GameObject pointCloud;

	public float scale = 1;
	public bool invertYZ = false;
	public bool forceReload = false;

	private int pointLimit = 65000; //Unity limitation of number of points in a Mesh

    List<Ring> rings;
    private Vector3[] points;
	private Color[] colours;
	private Vector3 minValue;

    [SerializeField]
    Text text;
    [SerializeField]
    Text descriptionText;

	void Start()
    {
        LoadPointCloud();
        
        
        /*// Create Resources folder
		createFolders ();

		// Get Filename
		filename = Path.GetFileName(dataPath);

		loadScene ();*/
	}



	void loadScene(){
		// Check if the PointCloud was loaded previously
		if(!Directory.Exists (Application.dataPath + "/Resources/PointCloudMeshes/" + filename)){
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources/PointCloudMeshes", filename);
			LoadPointCloud();
		} else if (forceReload){
			UnityEditor.FileUtil.DeleteFileOrDirectory(Application.dataPath + "/Resources/PointCloudMeshes/" + filename);
			UnityEditor.AssetDatabase.Refresh();
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources/PointCloudMeshes", filename);
			LoadPointCloud();
		} else
			// Load stored PointCloud
			loadStoredMeshes();
	}
	
	
	void LoadPointCloud(){
        StartCoroutine("LoadPoints", @"C:\Users\Roby\Downloads\yarraValley_scan_1.txt");
  

        /*// Check what file exists
		if (File.Exists (Application.dataPath + dataPath + ".off")) 
			StartCoroutine("LoadPoints", @"C:\Users\Roby\Downloads\yarraValley_scan_1.txt");
		else 
			Debug.Log ("File '" + dataPath + "' could not be found"); */

    }

    // Load stored PointCloud
    void loadStoredMeshes(){

		Debug.Log ("Using previously loaded PointCloud: " + filename);

		GameObject pointGroup = Instantiate(Resources.Load ("PointCloudMeshes/" + filename)) as GameObject;
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
        rings[b].offset = difference - rings[a].offset;

        //print(difference);
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

            numPoints += ring.AddPoint(range, angle);

            if (id == 1080)
            {
                rings.Add(ring);
                ring = new Ring();
            }

            if (numPoints % 10000 == 0)
            {
                text.text = numPoints.ToString();
                yield return null;
            }
        }

        file.Close();

        descriptionText.text = "Reducing error";
        for (int i = 0; i < rings.Count - 1; i++)
        {
            MinimiseError(i, i + 1);
        }

        points = new Vector3[numPoints];
        colours = new Color[numPoints];

        descriptionText.text = "Building point cloud";
        int pointNum = 0;
        for (int i = 0; i < rings.Count; i++)
        {
            for (int j = 0; j < rings[i].points.Count; j++)
            {
                if (rings[i].points[j].valid)
                {
                    Vector2 pt = rings[i].GetPointWithOffset(j);
                    points[pointNum] = new Vector3(pt.x, pt.y, i * 30);
                    float h = Mathf.Lerp(0, 1, pt.magnitude / 2500f);

                    colours[pointNum] = Color.HSVToRGB(h, 1, 1); //TODO: based on distance from surface
                    pointNum++;
                }
            }

            if (pointNum % 100 == 0)
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
		GameObject pointGroup = new GameObject (filename + meshInd);
		pointGroup.AddComponent<MeshFilter> ();
		pointGroup.AddComponent<MeshRenderer> ();
		pointGroup.GetComponent<Renderer>().material = matVertex;

		pointGroup.GetComponent<MeshFilter> ().mesh = CreateMesh (meshInd, nPoints, pointLimit);
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

	void calculateMin(Vector3 point){
		if (minValue.magnitude == 0)
			minValue = point;


		if (point.x < minValue.x)
			minValue.x = point.x;
		if (point.y < minValue.y)
			minValue.y = point.y;
		if (point.z < minValue.z)
			minValue.z = point.z;
	}

	void createFolders(){
		if(!Directory.Exists (Application.dataPath + "/Resources/"))
			UnityEditor.AssetDatabase.CreateFolder ("Assets", "Resources");

		if (!Directory.Exists (Application.dataPath + "/Resources/PointCloudMeshes/"))
			UnityEditor.AssetDatabase.CreateFolder ("Assets/Resources", "PointCloudMeshes");
	}
}
