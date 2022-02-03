using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Tile : MonoBehaviour
{
    [SerializeField] private Color baseColor, offsetColor, highlightColor; //All possible Colors
    [SerializeField] private MeshRenderer renderer;

    private Color fixedColor; //This var will store the color the algo decided for this Tile

    private bool mouseOver;

    private Ray ray;
    private RaycastHit hit;

    private GameObject currentHit;

    void Update()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Color startColor = Color.white;

        if (Physics.Raycast(ray, out hit))
        {
            currentHit = hit.collider.gameObject;
            if (currentHit.GetComponent<Collider>().tag.Equals("TestFloor"))
            {
                mouseOver = true;
                currentHit.GetComponent<Renderer>().material.color = highlightColor;
            }
            else
            {
                mouseOver = false;
            }
        }

        if (!mouseOver)
        {
            if (currentHit.tag.Equals("TestFloor"))
            {
                currentHit.GetComponent<Renderer>().material.color = fixedColor;
                currentHit = null;
            }
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
