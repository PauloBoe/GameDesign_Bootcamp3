using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class SpidermanRunner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float[] laneXPositions = { -2f, 0f, 2f };
    public float laneSwitchSpeed = 10f;

    [Header("Run Speed")]
    public float startSpeed = 5f;
    public float maxSpeed = 20f;
    public float speedIncreaseRate = 0.1f;

    [Header("Visual Knockback Settings")]
    [Tooltip("How many units backward Spider-Man gets instantly pushed.")]
    public float knockbackDistance = 1.5f;
    [Tooltip("How fast he snaps / recovers forward back to his normal running position.")]
    public float knockbackRecoverSpeed = 8f;
    [Tooltip("How much speed his forward momentum loses during impact.")]
    public float knockbackSpeedLoss = 4f;
    [Tooltip("How fast his speedometer recovers.")]
    public float speedRecoveryRate = 12f;

    [Header("Jump")]
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    [Header("Score & Coins")]
    public int coins = 0;
    public int score = 0;

    // ── Private ──
    private Rigidbody rb;
    private int currentLane = 1;
    private float currentSpeed;
    private float targetSpeed;
    private bool isAlive = true;
    private bool isGrounded;

    // Knockback offset logic
    private float currentZOffset = 0f;

    private Keyboard kb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
    }

    void Start()
    {
        targetSpeed = startSpeed;
        currentSpeed = startSpeed;
        kb = Keyboard.current;
    }

    void Update()
    {
        if (!isAlive) return;

        kb = Keyboard.current;
        if (kb == null) return;

        HandleInput();
        SmoothLaneSnap();
        CalculateSpeed();
        HandleKnockbackRecovery();

        score = Mathf.FloorToInt(transform.position.z);
    }

    void FixedUpdate()
    {
        if (!isAlive) return;
        AutoRun();
    }

    void AutoRun()
    {
        Vector3 vel = rb.linearVelocity;
        vel.z = currentSpeed;
        rb.linearVelocity = vel;
    }

    void CalculateSpeed()
    {
        targetSpeed = Mathf.Min(targetSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedRecoveryRate * Time.deltaTime);
    }

    // ── Handle the physical visual slide back ─────────────────────────────────
    void HandleKnockbackRecovery()
    {
        if (currentZOffset < 0f)
        {
            // Smoothly move the offset back to 0 over time
            float previousOffset = currentZOffset;
            currentZOffset = Mathf.MoveTowards(currentZOffset, 0f, knockbackRecoverSpeed * Time.deltaTime);

            // Physically apply the change in offset to the player's position
            float offsetDelta = currentZOffset - previousOffset;
            transform.position += new Vector3(0f, 0f, offsetDelta);
        }
    }

    void HandleInput()
    {
        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) ShiftLane(-1);
        if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) ShiftLane(1);
        if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) TryJump();
    }

    void ShiftLane(int direction)
    {
        currentLane = Mathf.Clamp(currentLane + direction, 0, laneXPositions.Length - 1);
    }

    void SmoothLaneSnap()
    {
        float targetX = laneXPositions[currentLane];
        Vector3 pos = transform.position;
        pos.x = Mathf.Lerp(pos.x, targetX, Time.deltaTime * laneSwitchSpeed);
        transform.position = pos;
    }

    void TryJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        if (!isGrounded) return;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle"))
        {
            Die();
        }
        else if (other.CompareTag("Coin"))
        {
            coins++;
            other.gameObject.SetActive(false);
            Debug.Log("Coin collected! Total: " + coins);
        }
        else if (other.CompareTag("Box"))
        {
            BreakableBox box = other.GetComponent<BreakableBox>();
            if (box != null)
            {
                box.Break();
                ApplyKnockback();
            }
        }
    }

    // ── Physical Knockback Trigger ────────────────────────────────────────────
    void ApplyKnockback()
    {
        // 1. Instantly drop forward speed meter
        currentSpeed = Mathf.Max(currentSpeed - knockbackSpeedLoss, 3f);

        // 2. Teleport him back visually by a few units instantly
        currentZOffset -= knockbackDistance;
        transform.position -= new Vector3(0f, 0f, knockbackDistance);

        Debug.Log("Smashed box! Stumbled backward physically.");
    }

    void Die()
    {
        isAlive = false;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Hit obstacle! Score: " + score + " | Coins: " + coins);
    }

    public void ResetPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
        currentLane = 1;
        targetSpeed = startSpeed;
        currentSpeed = startSpeed;
        currentZOffset = 0f;
        coins = 0;
        score = 0;
        isAlive = true;
        rb.linearVelocity = Vector3.zero;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Score: " + score);
        GUI.Label(new Rect(10, 30, 200, 20), "Coins: " + coins);
        GUI.Label(new Rect(10, 50, 200, 20), "Speed: " + currentSpeed.ToString("F1"));
    }
}