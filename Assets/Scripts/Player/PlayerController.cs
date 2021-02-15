using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    // COMPONENTS
    private CharacterController controller;
    [SerializeField] private CameraController camController;

    // LOCOMOTION
    [Header("Locomotion")]
    [SerializeField] private float moveSpeed = 2.5f;
    private Vector3 inputDir;
    private Vector3 velocity = Vector3.zero;

    private float turnSmoothing = 0;
    [SerializeField] private float turnSpeed = 0;

    // CAMERA RELATIVE
    [SerializeField] private float changeMoveRelativityThreshold = 5.0f;
    private float cameraRelativeMoveAngle = 0;
    private bool updateCameraMovementRelativity = true;
    private Coroutine relativeMoveCoroutine = null;

    // PHYSICS
    private float yVelocity = 0;
    [SerializeField] private float groundedGravity = -0.02f;
    private float gravity = Physics.gravity.y;

    // ANIMATION
    [SerializeField] private Animator animator = null;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (camController == null)
        {
            camController = FindObjectOfType<CameraController>();
        }
    }

    private void OnEnable()
    {
        camController.OnViewChange += ChangeMovementRelativity;
    }

    private void OnDisable()
    {
        camController.OnViewChange -= ChangeMovementRelativity;
    }

    private void Update()
    {
        // Input
        inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        if(inputDir.sqrMagnitude > 0)
        {
            cameraRelativeMoveAngle = updateCameraMovementRelativity ? camController.transform.eulerAngles.y : cameraRelativeMoveAngle;

            float targetAngle = Mathf.Atan2(inputDir.x, inputDir.z) * Mathf.Rad2Deg + cameraRelativeMoveAngle;
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

        animator.SetFloat("Speed", inputDir.magnitude);
    }

    /// <summary>
    /// Starts the movement relativity change process (an associated coroutine).
    /// </summary>
    private void ChangeMovementRelativity()
    {
        relativeMoveCoroutine = StartCoroutine(WaitForInputDirChange());
    }

    /// <summary>
    /// Waits for the player's inputDir to change by the changeMoveRelativityThreshold (coroutine must be explicitly stopped otherwise).
    /// </summary>
    private IEnumerator WaitForInputDirChange()
    {
        updateCameraMovementRelativity = false;
        Vector3 onChangeInputDir = inputDir;

        while(!updateCameraMovementRelativity)
        {
            if(Vector3.Angle(onChangeInputDir, inputDir) > changeMoveRelativityThreshold 
                || inputDir == Vector3.zero)
            {
                updateCameraMovementRelativity = true;
            }

            yield return null;
        }

        relativeMoveCoroutine = null;
    }

    /*
     * TODO:
     * Jump
     */
}
