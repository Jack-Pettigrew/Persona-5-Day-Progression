using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider), typeof(Rigidbody))]
public class CameraTriggerCollider : MonoBehaviour
{
    public CameraController CamController
    {
        private set; get;
    }

    private void Awake()
    {
        GetComponent<Rigidbody>().isKinematic = true;
    }

    void Start()
    {
        CamController = FindObjectOfType<CameraController>();
    }
}
