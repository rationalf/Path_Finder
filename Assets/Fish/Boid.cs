using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid : MonoBehaviour
{
    public float speed = 3f;
    public float neighborRadius = 10f;
    public float separationRadius = 2f;
    private float oppositeDir = 3f;

    private Vector3 velocity;

    void Start()
    {
        velocity = new Vector3(Random.Range(-1, 1), Random.Range(-1,1), Random.Range(-1, 1));
    }

    void Update()
    {
        Vector3 cohesion = Cohesion();
        Vector3 separation = Separation();
        Vector3 alignment = Alignment();

        velocity += cohesion + separation + alignment;
        velocity = velocity.normalized * speed;
        if (transform.position.y < 17)
        {
            velocity = new Vector3(velocity.x, oppositeDir, velocity.z);
        } else if (transform.position.y > 30)
        {
            velocity = new Vector3(velocity.x, -oppositeDir, velocity.z);
        }

        if (transform.position.x < 0)
        {
            velocity = new Vector3(oppositeDir, velocity.y, velocity.z);
        }
        else if (transform.position.x > 50)
        {
            velocity = new Vector3(-oppositeDir, velocity.y, velocity.z);
        }

        if (transform.position.z < 0)
        {
            velocity = new Vector3(velocity.x, velocity.y, oppositeDir);
        }
        else if (transform.position.z > 65)
        {
            velocity = new Vector3(velocity.x, velocity.y, -oppositeDir);
        }
        transform.position += velocity * Time.deltaTime;
        transform.forward = velocity.normalized;
    }

    Vector3 Cohesion()
    {
        Vector3 center = Vector3.zero;
        int count = 0;

        foreach (var boid in FindObjectsOfType<Boid>())
        {
            if (boid != this && Vector3.Distance(transform.position, boid.transform.position) < neighborRadius)
            {
                center += boid.transform.position;
                count++;
            }
        }

        if (count > 0)
        {
            center /= count;
            return (center - transform.position).normalized * 0.7f;
        }

        return Vector3.zero;
    }

    Vector3 Separation()
    {
        Vector3 avoid = Vector3.zero;
        int count = 0;

        foreach (var boid in FindObjectsOfType<Boid>())
        {
            float distance = Mathf.Abs(transform.position.y - boid.transform.position.y);
            if (boid != this && distance < separationRadius)
            {
                avoid += (transform.position - boid.transform.position) / distance;
                count++;
            }
        }

        if (count > 0)
        {
            avoid /= count;
        }

        return avoid.normalized * 1f;
    }

    Vector3 Alignment()
    {
        Vector3 align = Vector3.zero;
        int count = 0;

        foreach (var boid in FindObjectsOfType<Boid>())
        {
            if (boid != this && Vector3.Distance(transform.position, boid.transform.position) < neighborRadius)
            {
                align += boid.velocity;
                count++;
            }
        }

        if (count > 0)
        {
            align /= count;
            return align.normalized * 0.7f;
        }

        return Vector3.zero;
    }
}
