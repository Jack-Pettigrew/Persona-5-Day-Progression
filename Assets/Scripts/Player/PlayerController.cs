using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // COMPONENTS
    private CharacterController controller;
    [SerializeField] private CameraController camController;
    private Transform cameraTransform = null;

    // PHYSICS
    [Header("Locomotion")]
    [SerializeField] private float moveSpeed = 2.5f;
    private Vector3 inputDir;
    private Vector3 velocity = Vector3.zero;

    private float turnSmoothing = 0;
    [SerializeField] private float turnSpeed = 0;

    private float yVelocity = 0;
    [SerializeField] private float groundedGravity = -0.02f;
    private float gravity = Physics.gravity.y;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (camController == null)
        {
            camController = FindObjectOfType<CameraController>();
            cameraTransform = camController.transform;
        }
    }

    private void Update()
    {
        // Input
        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if(inputDir.sqrMagnitude > 0)
        {
            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraTransform.eulerAngles.y;
            transform.eulerAngles = Vector3.up * Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothing, turnSpeed);
        }

        // Gravity
        if (controller.isGrounded)
        {
            yVelocity = groundedGravity;
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        // Move
        velocity = (transform.forward * inputDir.magnitude * moveSpeed) + Vector3.up * yVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    /*
     * TODO:
     * Camera relative move change when inputDir changes
     * Jump
     * On ToPlayView camera is positions behind the player's current heading direction
     */
}
