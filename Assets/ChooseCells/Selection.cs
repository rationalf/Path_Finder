using System;
using UnityEngine;

public class HexagonSelector : MonoBehaviour
{
    public static GameObject startHex; // Начальная точка
    public static GameObject endHex;   // Конечная точка
    private static bool isStartSelected = false;
    public static bool isEndSelected = false;
    public int counter = 0;
    private Renderer hexRenderer;

    void Start()
    {
        hexRenderer = GetComponent<Renderer>();
    }

    void OnMouseDown()
    {
        Debug.Log(counter);
        if (!isStartSelected)
        {
            
            // Устанавливаем начальную точку
            startHex = gameObject;
            hexRenderer.material.color = Color.green; // Задаем цвет для начальной точки
            isStartSelected = true;
            Debug.Log($"Start hex selected: {startHex.GetComponent<HexagonalPrismGenerator>().index}");
            return;
        }
        if (isStartSelected && endHex == null)
        {
            // Устанавливаем конечную точку
            endHex = gameObject;
            hexRenderer.material.color = Color.red; // Задаем цвет для конечной точки
            Debug.Log($"End hex selected: {endHex.GetComponent<HexagonalPrismGenerator>().index}");
            endHex.GetComponentInParent<HexGrid>().isEndSelected = true;
        }
    }
}