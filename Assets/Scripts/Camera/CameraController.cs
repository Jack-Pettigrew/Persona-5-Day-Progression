using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private bool sceneicViewMode = false;

    [SerializeField]
    private Transform cameraTarget;
    private Vector3 cameraTargetPosition;

    private float pitch, yaw;
    private Vector3 cameraRotation;

    private void Start()
    {
        cameraTargetPosition = cameraTarget.position;

        pitch = -transform.localEulerAngles.x;
        yaw = transform.localEulerAngles.y;
    }

    private void Update()
    {
        if(!sceneicViewMode)
        {
            ControlCamera();
        }
    }

    private void ControlCamera()
    {
        yaw += Input.GetAxis("Mouse X");
        pitch += Input.GetAxis("Mouse Y");

        cameraRotation.x = -pitch;
        cameraRotation.y = yaw;

        transform.localEulerAngles = cameraRotation;
    }
}
