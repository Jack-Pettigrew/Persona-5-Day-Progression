using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class CameraTrigger : MonoBehaviour
{
    [SerializeField] private Vector3 viewPosition;
    [SerializeField] private Vector3 viewRotation;

    private CameraController cameraController = null;

    private void Awake()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        cameraController = other.GetComponent<CameraTriggerCollider>().CamController;
        cameraController.SwapToScenicView(viewPosition, viewRotation);
    }

    private void OnTriggerExit(Collider other)
    {
        if(cameraController != null)
        {
            cameraController.SwapToPlayView();
            cameraController = null;
        }
    }
}
