using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // COMPONENTS
    private CharacterController controller;
    [SerializeField] private CameraController camera;

    // PHYSICS
    [Header("Physics")]
    [SerializeField] private float characterSpeed = 2.5f;
    private Vector3 inputDir;
    private Vector3 velocity = Vector3.zero;

    private float yVelocity = 0;
    [SerializeField] private float groundedGravity = -0.02f;
    private float gravity = Physics.gravity.y;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (camera == null)
            camera = FindObjectOfType<CameraController>();
    }

    private void Update()
    {
        // Input
        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

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
        velocity.x = inputDir.x; 
        velocity.z = inputDir.z;
        velocity *= (characterSpeed * Time.deltaTime);
        velocity.y = yVelocity;

        controller.Move(velocity);
    }
}
