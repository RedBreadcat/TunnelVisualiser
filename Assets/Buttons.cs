using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Buttons : MonoBehaviour {

    public void StartButton()
    {
        Destroy(gameObject);
        PointCloudManager.pcm.Construct();
        CameraController.cam.enabled = true;
    }

    public void FreeCam()
    {
        CameraController.cam.SetFreeMode(true);
    }

    public void FixedCam()
    {
        CameraController.cam.SetFreeMode(false);
    }

    public void ToggleLines()
    {
        PointCloudManager.pcm.lm.ToggleVisibility();
    }

    public void TogglePoints()
    {
        PointCloudManager.pcm.pointCloud.SetActive(!PointCloudManager.pcm.pointCloud.activeInHierarchy);
    }

    public void ResetCamera()
    {
        CameraController.cam.transform.position = new Vector3(0, 0, 0);
        CameraController.cam.transform.rotation = Quaternion.identity;
    }

    public void Quit()
    {
        Application.Quit();
    }
}
