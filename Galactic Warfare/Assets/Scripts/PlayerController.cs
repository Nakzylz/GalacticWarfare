using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Dodge")]
    public float dodgeDistance = 3f;
    public float doubleTapTime = 0.3f;

    [Header("Audio")]
    public AudioClip shootSound;
    private AudioSource audioSource;

    [Header("Rotation Limits")]
    public float minX = -5f;
    public float maxX = 5f;

    [Header("Shooting")]
    public GameObject projectilePrefab;
    public Transform shootOrigin;
    public float shootDelay = 0.5f;

    private float lastShootTime = -999f;
    private Rigidbody rb;

    // Double tap detection
    private float lastTapTimeA = -1f;
    private float lastTapTimeD = -1f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        HandleKeyboardMovement();
        HandleRotation();
        HandleKeyboardDodge();
        HandleTapShoot();
    }

    void HandleKeyboardMovement()
    {
        float moveX = 0f;

        if (Input.GetKey(KeyCode.A)) moveX = -1f;
        if (Input.GetKey(KeyCode.D)) moveX = 1f;

        Vector3 move = new Vector3(moveX * moveSpeed, rb.velocity.y, 0f);
        rb.velocity = move;
    }

    void HandleRotation()
    {
        float normalizedX = Mathf.InverseLerp(minX, maxX, transform.position.x);
        float targetYRotation = Mathf.Lerp(-20f, 20f, normalizedX);

        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
    }

    void HandleKeyboardDodge()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - lastTapTimeA < doubleTapTime)
                TryDodge(Vector3.left);
            lastTapTimeA = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - lastTapTimeD < doubleTapTime)
                TryDodge(Vector3.right);
            lastTapTimeD = Time.time;
        }
    }

    void TryDodge(Vector3 direction)
    {
        Vector3 targetPos = transform.position + direction * dodgeDistance;

        if (targetPos.x >= minX && targetPos.x <= maxX)
            StartCoroutine(FlipRoutine(direction));
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
        if (Time.time - lastShootTime < shootDelay) return;

        if (Input.GetMouseButtonDown(0))
            TryShootAtTap(Input.mousePosition);

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryShootAtTap(Input.GetTouch(0).position);
    }

    void TryShootAtTap(Vector3 tapPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(tapPosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Enemy") || hit.collider.CompareTag("Asteroid"))
            {
                Vector3 dir = (hit.collider.transform.position - shootOrigin.position).normalized;
                Instantiate(projectilePrefab, shootOrigin.position, Quaternion.LookRotation(dir));
                audioSource.PlayOneShot(shootSound);
                lastShootTime = Time.time;
            }
        }
    }
}










