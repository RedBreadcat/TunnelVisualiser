using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour {

    public static CameraController cam;
    public float moveSpeed;
    public float sensitivityX = 15F;
    public float sensitivityY = 15F;

    public float minimumX = -360F;
    public float maximumX = 360F;

    public float minimumY = -60F;
    public float maximumY = 60F;

    float rotationX = 0F;
    float rotationY = 0F;

    private List<float> rotArrayX = new List<float>();
    float rotAverageX = 0F;

    private List<float> rotArrayY = new List<float>();
    float rotAverageY = 0F;

    public float frameCounter = 20;

    Quaternion originalRotation;

    bool freeMode;

    [SerializeField]
    Text text;

    void Awake()
    {
        cam = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originalRotation = transform.localRotation;
    }

    void Update()
    {
        float speedFrame = moveSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            speedFrame *= 25;
        }

        if (freeMode)
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += transform.forward * speedFrame * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position -= transform.forward * speedFrame * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.D))
            {
                transform.position += transform.right * speedFrame * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                transform.position -= transform.right * speedFrame * Time.deltaTime;
            }

            if (Input.GetKey(KeyCode.Space))
            {
                transform.position += Vector3.up * speedFrame * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.LeftControl))
            {
                transform.position -= Vector3.up * speedFrame * Time.deltaTime;
            }
        }
        else
        {
            if (Input.GetKey(KeyCode.W))
            {
                transform.position += new Vector3(0, 0, speedFrame * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position -= new Vector3(0, 0, speedFrame * Time.deltaTime);
            }

            transform.position = new Vector3(0, 0, Mathf.Clamp(transform.position.z, 0, 62000));
        }


        rotAverageY = 0f;
        rotAverageX = 0f;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, -90, 90);
        rotationX += Input.GetAxis("Mouse X") * sensitivityX;

        rotArrayY.Add(rotationY);
        rotArrayX.Add(rotationX);

        if (rotArrayY.Count >= frameCounter)
        {
            rotArrayY.RemoveAt(0);
        }
        if (rotArrayX.Count >= frameCounter)
        {
            rotArrayX.RemoveAt(0);
        }

        for (int j = 0; j < rotArrayY.Count; j++)
        {
            rotAverageY += rotArrayY[j];
        }
        for (int i = 0; i < rotArrayX.Count; i++)
        {
            rotAverageX += rotArrayX[i];
        }

        rotAverageY /= rotArrayY.Count;
        rotAverageX /= rotArrayX.Count;

        rotAverageY = ClampAngle(rotAverageY, minimumY, maximumY);
        rotAverageX = ClampAngle(rotAverageX, minimumX, maximumX);

        Quaternion yQuaternion = Quaternion.AngleAxis(rotAverageY, Vector3.left);
        Quaternion xQuaternion = Quaternion.AngleAxis(rotAverageX, Vector3.up);

        transform.localRotation = originalRotation * xQuaternion * yQuaternion;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        angle = angle % 360;
        if ((angle >= -360F) && (angle <= 360F))
        {
            if (angle < -360F)
            {
                angle += 360F;
            }
            if (angle > 360F)
            {
                angle -= 360F;
            }
        }
        return Mathf.Clamp(angle, min, max);
    }

    public void SetFreeMode(bool free)
    {
        freeMode = free;

        if (!free)
        {
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            text.text = "Press Tab to open and close the menu\nW: move forward\nS: move back\nUse the mouse to look around. Hold shift to move faster.";
        }
        else
        {
            text.text = "Press Tab to open and close the menu\nW: move forward\nS: move back\nA: move left\nD: move right\nSpace: move up\nCtrl: move down\nUse the mouse to look around. Hold shift to move faster.";
        }
    }

    public void ToggleFreeMode()
    {
        SetFreeMode(!freeMode);
    }
}
