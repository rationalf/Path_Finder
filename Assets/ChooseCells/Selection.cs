using System;
using UnityEngine;

public class HexagonSelector : MonoBehaviour
{
    public static GameObject startHex;
    public static GameObject endHex; 
    private static bool isStartSelected;
    private static bool isEndSelected;
    private Renderer hexRenderer;

    void Start()
    {
        hexRenderer = GetComponent<Renderer>();
    }

    void OnMouseDown()
    {
        if (!isStartSelected)
        {
            // Place starting point
            startHex = gameObject;
            hexRenderer.material.color = Color.green;
            isStartSelected = true;
            return;
        }
        if (isStartSelected && !isEndSelected)
        {
            // Place ending point
            endHex = gameObject;
            hexRenderer.material.color = Color.green;
            isEndSelected = true;
            endHex.GetComponentInParent<HexGrid>().isEndSelected = true;
        }
    }
}