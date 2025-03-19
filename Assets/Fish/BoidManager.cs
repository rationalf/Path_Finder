using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    public GameObject boidPrefab;
    public int boidCount = 50;
    public int spawnRange = 1;

    void Start()
    {
        for (int i = 0; i < boidCount; i++)
        {
            Vector3 spawnPos = new Vector3(
                transform.position.x + i,
                30,  // Фиксируем высоту Y
                transform.position.z
            );

            Instantiate(boidPrefab, spawnPos, Quaternion.identity);
        }
    }
}
