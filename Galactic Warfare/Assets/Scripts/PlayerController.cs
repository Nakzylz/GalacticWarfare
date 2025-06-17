using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public Joystick joystick;

    [Header("Dodge")]
    public float dodgeDistance = 3f;
    public float swipeCooldown = 0.5f;
    private float lastSwipeTime = -999f;

    [Header("Rotation Limits")]
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform shootOrigin;
    public float shootDelay = 0.5f;
    private float lastShootTime = -999f;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        HandleJoystickMovement();
        HandleRotation();
        CheckSwipeDodge();
        HandleTapShoot();
    }

    void HandleJoystickMovement()
    {
        if (joystick == null) return;

        float hInput = joystick.axisValue.x;
        Vector3 move = new Vector3(hInput * moveSpeed, rb.velocity.y, 0f);
        rb.velocity = move;
    }

    void HandleRotation()
    {
        // Map player's X position to Y rotation from -20° (left) to +20° (right)
        float normalizedX = Mathf.InverseLerp(minX, maxX, transform.position.x);
        float targetYRotation = Mathf.Lerp(-20f, 20f, normalizedX);

        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void CheckSwipeDodge()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Ended)
            {
                if (Time.time - lastSwipeTime < swipeCooldown)
                    return;

                Vector2 swipe = touch.position - touch.rawPosition;

                if (swipe.magnitude > Screen.dpi * 0.25f)
                {
                    Vector3 direction = swipe.x < 0 ? Vector3.left : Vector3.right;
                    Vector3 targetPosition = transform.position + direction * dodgeDistance;

                    if (targetPosition.x >= minX && targetPosition.x <= maxX)
                    {
                        StartCoroutine(FlipRoutine(direction));
                        lastSwipeTime = Time.time;
                    }
                }
            }
        }
    }

    IEnumerator FlipRoutine(Vector3 direction)
    {
        float duration = 0.3f;
        float totalRotation = direction == Vector3.left ? 360f : -360f;
        float t = 0f;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + direction * dodgeDistance;

        float currentYRotation = transform.localEulerAngles.y;

        while (t < duration)
        {
            t += Time.deltaTime;
            float normalizedTime = Mathf.Clamp01(t / duration);

            float currentZRotation = Mathf.Lerp(0f, totalRotation, normalizedTime);

            transform.localEulerAngles = new Vector3(0f, currentYRotation, currentZRotation);
            transform.position = Vector3.Lerp(startPos, endPos, normalizedTime);

            yield return null;
        }

        transform.position = endPos;
        transform.localEulerAngles = new Vector3(0f, currentYRotation, 0f);
    }

    void HandleTapShoot()
    {
        if (Time.time - lastShootTime < shootDelay)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryShootAtTap(Input.mousePosition);
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            TryShootAtTap(Input.GetTouch(0).position);
        }
    }

    void TryShootAtTap(Vector3 tapPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(tapPosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 2f); // Visual debug in Scene view

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycast hit: " + hit.collider.name + " Tag: " + hit.collider.tag);

            if (hit.collider.CompareTag("Enemy"))
            {
                Vector3 direction = (hit.collider.transform.position - shootOrigin.position).normalized;
                Quaternion rotation = Quaternion.LookRotation(direction);

                Instantiate(projectilePrefab, shootOrigin.position, rotation);
                lastShootTime = Time.time;
            }
        }
        else
        {
            Debug.Log("Raycast missed");
        }
    }

}









