using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AsteroidSpinner : MonoBehaviour
{
    private Vector3 rotationAxis;
    private float rotationSpeed;

    // Start is called before the first frame update
    void Start()
    {
        // Set a random rotation axis and speed
        rotationAxis = Random.onUnitSphere; // Random direction in 3D space
        rotationSpeed = Random.Range(5f, 15f); // degrees per second
    }

    // Update is called once per frame
    void Update()
    {
        // Apply rotation
        transform.Rotate(rotationAxis, rotationSpeed * Time.deltaTime, Space.Self);
    }

}
