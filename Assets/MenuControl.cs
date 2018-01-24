using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        mc = this;
        enabled = false;
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
            bool active = !freeCameraButton.activeInHierarchy;
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
        }
    }
}
