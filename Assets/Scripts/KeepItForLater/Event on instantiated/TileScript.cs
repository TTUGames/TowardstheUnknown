using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TileScript : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetColor, highlightColor; //All possible Colors
    [SerializeField] private MeshRenderer renderer;

    private Color fixedColor; //This var will store the color the algo decided for this Tile

    private Collider hitObject;

    private Camera camera;

    private void Start()
    {
        camera = Camera.main;
    }
    private void Update()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit) && hit.collider.CompareTag("TestFloor"))
        {
            if (hitObject != null)
                hitObject.GetComponent<Renderer>().material.color = fixedColor;

            hitObject = hit.collider;
            hitObject.GetComponent<Renderer>().material.color = highlightColor;
        }
        else if (hitObject != null)
        {
            hitObject.GetComponent<Renderer>().material.color = fixedColor;
            hitObject = null;
        }
    }

    public void Colorize(bool isOffset)
    {
        fixedColor = isOffset ? offsetColor : baseColor;
        renderer.material.color = fixedColor;
    }

    /*void OnMouseEnter()
    {
        renderer.material.color = highlightColor;
        Debug.Log("entered");
    }

    void OnMouseExit()
    {
        renderer.material.color = fixedColor;
    }*/
}
