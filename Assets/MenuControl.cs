using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuControl : MonoBehaviour {

    public static MenuControl mc;

    [SerializeField]
    GameObject freeCameraButton;
    [SerializeField]
    GameObject fixedCameraButton;
    [SerializeField]
    GameObject linesButton;
    [SerializeField]
    GameObject pointsButton;
    [SerializeField]
    GameObject resetButton;
    [SerializeField]
    GameObject quitButton;
    [SerializeField]
    GameObject menuText;

    bool active;
    Texture2D screenTex;
    [SerializeField]
    Text distanceText;

    int lastMouseX;
    int lastMouseY;

    private void Awake()
    {
        mc = this;
        enabled = false;
        screenTex = new Texture2D(Screen.width, Screen.height);
    }

    public void CloudReady()
    {
        menuText.SetActive(true);
        enabled = true;
        CameraController.cam.SetFreeMode(false);
    }

    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Tab))
        {
            active = !freeCameraButton.activeInHierarchy;
            Cursor.visible = active;
            if (active)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
            CameraController.cam.enabled = !active;
            freeCameraButton.SetActive(active);
            fixedCameraButton.SetActive(active);
            linesButton.SetActive(active);
            pointsButton.SetActive(active);
            resetButton.SetActive(active);
            quitButton.SetActive(active);
            distanceText.gameObject.SetActive(active);
        }
    }

    private void OnPostRender()
    {
        if (active && ((int)Input.mousePosition.x != lastMouseX || (int)Input.mousePosition.y != lastMouseY))
        {
            screenTex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

            List<float> innerMeasurements = new List<float>();
            List<float> outerMeasurements = new List<float>();

            //Scanning a 10x10 area and getting average distance of it
            for (int i = -10; i < 10; i++)
            {
                for (int j = -10; j < 10; j++)
                {
                    int x = (int)Input.mousePosition.x + i;
                    int y = (int)Input.mousePosition.y + j;

                    if (x > 0 && x < screenTex.width && y > 0 && y < screenTex.height)
                    {
                        Color col = screenTex.GetPixel(x, y);
                        float h, s, v;
                        Color.RGBToHSV(col, out h, out s, out v);

                        if (col.r > 0.95f && col.b < 0.8f)
                        {
                            innerMeasurements.Add(s);

                        }
                        else if (col.b > 0.95f && col.r < 0.8f)
                        {
                            outerMeasurements.Add(s);
                            distanceText.text = "Distance of point at mouse cursor: " + s * PointCloudManager.pcm.maxDistanceOuter + "mm";
                            lastMouseX = (int)Input.mousePosition.x;
                            lastMouseY = (int)Input.mousePosition.y;
                        }
                        else
                        {
                            distanceText.text = "";
                        }
                    }
                }
            }

            if (innerMeasurements.Count > 0 || outerMeasurements.Count > 0)
            {
                float distance;
                if (innerMeasurements.Count > outerMeasurements.Count)
                {
                    float averageSaturation = 0;
                    for (int i = 0; i < innerMeasurements.Count; i++)
                    {
                        averageSaturation += innerMeasurements[i];
                    }

                    distance = averageSaturation / innerMeasurements.Count * PointCloudManager.pcm.maxDistanceInner;
                }
                else
                {
                    float averageSaturation = 0;
                    for (int i = 0; i < outerMeasurements.Count; i++)
                    {
                        averageSaturation += outerMeasurements[i];
                    }

                    distance = averageSaturation / outerMeasurements.Count * PointCloudManager.pcm.maxDistanceOuter;
                }

                distanceText.text = "Distance of point at mouse cursor: " + distance + "mm";
                lastMouseX = (int)Input.mousePosition.x;
                lastMouseY = (int)Input.mousePosition.y;
            }
            else
            {
                distanceText.text = "";
            }
        }
    }
}
