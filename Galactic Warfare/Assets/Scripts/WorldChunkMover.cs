using UnityEngine;

public class WorldChunkMover : MonoBehaviour
{
    public float moveSpeed = 10f;

    void Update()
    {
        transform.position -= new Vector3(0, 0, moveSpeed * Time.deltaTime);
    }
}

