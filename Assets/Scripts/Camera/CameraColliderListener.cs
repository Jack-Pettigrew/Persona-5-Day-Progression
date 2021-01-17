using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraColliderListener : MonoBehaviour
{
    private CameraController cameraController;

    private bool hasEntered = false;

    private void Awake()
    {
        cameraController = FindObjectOfType<CameraController>();
    }

    private void Update()
    {
        if(hasEntered)
        {
            TestExitCameraView();
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.collider.tag == "CameraView")
        {
            hasEntered = true;
        }
    }

    private void TestExitCameraView()
    {

    }
}
