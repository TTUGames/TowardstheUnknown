using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotate the <c>Camera</c>
/// </summary>
public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float rotationTimeInSeconds = 1.2f;

    private bool isRotating = false;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
            if (!isRotating)
            {
                StartCoroutine(RotateObject(90, Vector3.down, rotationTimeInSeconds));
                isRotating = true;
            }

        if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.A))
            if (!isRotating)
            {
                StartCoroutine(RotateObject(90, Vector3.up, rotationTimeInSeconds));
                isRotating = true;
            }
    }

    /// <summary>
    /// Rotate the <c>Camera</c> over time
    /// </summary>
    /// <param name="angle">Degrees the <c>Camera</c> must rotate</param>
    /// <param name="axis">Rotation direction</param>
    /// <param name="inTime">Time to rotate</param>
    /// <returns></returns>
    IEnumerator RotateObject(float angle, Vector3 axis, float inTime)
    {
        // calculate rotation speed
        float rotationSpeed = angle / inTime;

        // save starting rotation position
        Quaternion startRotation = transform.rotation;

        float deltaAngle = 0;

        // rotate until reaching angle
        while (deltaAngle < angle)
        {
            deltaAngle += rotationSpeed * Time.deltaTime;
            deltaAngle = Mathf.Min(deltaAngle, angle);

            transform.rotation = startRotation * Quaternion.AngleAxis(deltaAngle, axis);

            yield return null;
        }

        // delay here
        isRotating = false;
        yield return new WaitForSeconds(0);
    }
}
