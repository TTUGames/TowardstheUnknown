using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Rotate the <c>Camera</c>
/// </summary>
public class CameraRotator : MonoBehaviour
{
    [SerializeField] private float rotationTimeInSeconds = 1.2f;
    [SerializeField] private float zoomStep = 3f;
    [SerializeField] private float zoomMax = 20f;
    [SerializeField] private float zoomMin = 5f;

    private float defaultZoom;
    private bool isRotating = false;
    private Camera cam;

    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        defaultZoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetAxis("Horizontal") <= -0.1f)
            if (!isRotating)
            {
                StartCoroutine(RotateObject(90, Vector3.up, rotationTimeInSeconds));
                isRotating = true;
            }

        if (Input.GetAxis("Horizontal") >= 0.1f)
            if (!isRotating)
            {
                StartCoroutine(RotateObject(90, Vector3.down, rotationTimeInSeconds));
                isRotating = true;
            }

        //W
        if (Input.GetAxis("Vertical") >= 0.1f)
        {
            if(cam.orthographicSize > zoomMin)
                cam.orthographicSize -= zoomStep * Time.deltaTime;
        }

        //S
        if (Input.GetAxis("Vertical") <= -0.1f)
        {
            if(cam.orthographicSize < zoomMax)
                cam.orthographicSize += zoomStep * Time.deltaTime;
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

    IEnumerator ZoomInAndOut(float zoom, float inTime)
    {
        float zoomSpeed = zoom / inTime;
        float zoomDone = 0;
        while (zoomDone < Mathf.Abs(zoom))
        {
            zoomDone = Mathf.Abs(zoomSpeed * Time.deltaTime);
            cam.orthographicSize += zoomSpeed * Time.deltaTime;
            yield return null;
        }
    }
}
