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
    public float speedIncreaseRate = 0.1f;              // Permanent speed growth over time

    [Header("Knockback Settings")]
    [Tooltip("How much speed Spider-Man loses instantly when hitting a box.")]
    public float knockbackSpeedLoss = 5f;
    [Tooltip("How fast he recovers his speed. A higher value means he gets back to full speed faster.")]
    public float speedRecoveryRate = 12f;              // High value = snaps back to full speed within ~1 second

    [Header("Jump")]
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    [Header("Score & Coins")]
    public int coins = 0;
    public int score = 0;

    // ── Private ──
    private Rigidbody rb;
    private int currentLane = 1;
    private float currentSpeed;                         // The actual speed applied to the player
    private float targetSpeed;                          // The baseline speed the player *should* be at
    private bool isAlive = true;
    private bool isGrounded;

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

    // ── New Speed Management ──────────────────────────────────────────────────
    void CalculateSpeed()
    {
        // 1. Gradually increase the baseline max speed over time (normal runner progression)
        targetSpeed = Mathf.Min(targetSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);

        // 2. Smoothly catch currentSpeed up to targetSpeed. 
        // If currentSpeed dropped from a box, this forces it to rapidly climb back up.
        currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, speedRecoveryRate * Time.deltaTime);
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

    // ── Tweaked Knockback Logic ───────────────────────────────────────────────
    void ApplyKnockback()
    {
        // Instantly slash current speed, but don't drop below a minimum crawl (e.g., 3f)
        currentSpeed = Mathf.Max(currentSpeed - knockbackSpeedLoss, 3f);

        Debug.Log($"Slammed a box! Dropped to {currentSpeed:F1}. Recovering back to {targetSpeed:F1}...");
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