using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed;
    private float horizontalDirection;
    // Update is called once per frame
    void Update()
    {
        horizontalDirection = Input.GetAxis("Horizontal");
        transform.Rotate(0, -horizontalDirection * rotationSpeed * Time.deltaTime, 0);
    }
}
