using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // COMPONENTS
    private CharacterController controller;

    // PHYSICS
    [Header("Physics")]
    [SerializeField] private float characterSpeed = 2.5f;
    private Vector2 input;
    private Vector3 velocity = Vector3.zero;

    private float yVelocity = 0;
    [SerializeField] private float groundedGravity = -0.02f;
    private float gravity = Physics.gravity.y;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    private void Update()
    {
        // Input
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        input *= characterSpeed;

        // Input: Account for Camera

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
        velocity.x = input.x; velocity.z = input.y;
        velocity *= (characterSpeed * Time.deltaTime);
        velocity.y = yVelocity;

        controller.Move(velocity);
    }
}
