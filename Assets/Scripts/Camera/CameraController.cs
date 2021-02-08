using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Transitioning")]
    [SerializeField] private float transitionSpeed = 1.0f;
    private Coroutine transitionCoroutine = null;

    [Header("Scenic View")]
    private bool inScenicMode = false;

    [Header("Play View")]
    [SerializeField] private Transform cameraTarget;
    [SerializeField] private Vector3 targetOffset = Vector3.zero;

    // Controller Values
    private float pitch, yaw;
    Vector3 targetRot = Vector3.zero;

    private void Start()
    {
        pitch = -transform.localEulerAngles.x;
        yaw = transform.localEulerAngles.y;
    }

    private void LateUpdate()
    {
        if(!inScenicMode)
        {
            ControlCamera();
        }
    }

    private void ResetRotation()
    {
        pitch = 0;
        yaw = cameraTarget.eulerAngles.y;
    }

    private void ControlCamera()
    {
        yaw += Input.GetAxis("Mouse X");
        pitch += Input.GetAxis("Mouse Y");

        // Camera to play position
        targetRot = new Vector3(-pitch, yaw, 0);
        transform.eulerAngles = targetRot;

        transform.position = cameraTarget.position - (transform.forward * targetOffset.z) + transform.up * targetOffset.y;
    }

    public void SwapToScenicView(Vector3 position, Vector3 rotation)
    {
        inScenicMode = true;

        transitionCoroutine = StartCoroutine(MoveTo(position, Quaternion.Euler(rotation)));
    }

    public void SwapToPlayView()
    {
        inScenicMode = false;

        ResetRotation();
        // Lerp behind the player

        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);
    }

    private IEnumerator MoveTo(Vector3 position, Quaternion rotation)
    {
        float posDist = float.MaxValue, rotDiff = float.MaxValue;

        while(posDist > 0.02 || rotDiff > 0.02)
        {
            if(rotDiff > 0.02)
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * transitionSpeed);

            if(posDist > 0.02)
                transform.position = Vector3.Lerp(transform.position, position, Time.deltaTime * transitionSpeed);

            posDist = Vector3.Distance(transform.position, position);
            rotDiff = Quaternion.Angle(transform.rotation, rotation);

            yield return null;
        }
    }
}
