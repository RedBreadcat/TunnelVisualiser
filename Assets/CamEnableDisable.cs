using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamEnableDisable : MonoBehaviour {

    CameraController cc;

    private void Start()
    {
        cc = GetComponent<CameraController>();
    }

    void Update()
    {
		if (Input.GetKeyDown(KeyCode.Z))
        {
            cc.enabled = !cc.enabled;
        }
	}
}
