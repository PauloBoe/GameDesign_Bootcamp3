using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Spider-Man Unlimited - Simple Endless Runner (Unity 6 + New Input System)
/// Handles: Auto-run, lane switching, obstacle detection, speed progression, coin pickups
///
/// SETUP:
///   1. Attach to Player GameObject
///   2. Set the 3 Lane X positions in the Inspector
///   3. Tag obstacles as "Obstacle", coins as "Coin"
///   4. Add a Rigidbody + CapsuleCollider to the player
///   5. Set groundLayer to your ground layer mask
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
public class SpidermanRunner : MonoBehaviour
{
    [Header("Lane Settings")]
    public float[] laneXPositions = { -2f, 0f, 2f };   // Left, Center, Right
    public float laneSwitchSpeed = 10f;

    [Header("Run Speed")]
    public float startSpeed = 5f;
    public float maxSpeed = 20f;
    public float speedIncreaseRate = 0.1f;              // Units per second

    [Header("Jump")]
    public float jumpForce = 8f;
    public LayerMask groundLayer;

    [Header("Score & Coins")]
    public int coins = 0;
    public int score = 0;

    // ── Private ──
    private Rigidbody rb;
    private int currentLane = 1;                        // Start center lane
    private float currentSpeed;
    private bool isAlive = true;
    private bool isGrounded;

    // ── New Input System: keyboard reference ──────────────────────────────────
    private Keyboard kb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.linearDamping = 0f;
    }

    void Start()
    {
        currentSpeed = startSpeed;
        kb = Keyboard.current;                          // Grab current keyboard device
    }

    void Update()
    {
        if (!isAlive) return;

        kb = Keyboard.current;                          // Re-check each frame (device may reconnect)
        if (kb == null) return;

        HandleInput();
        SmoothLaneSnap();
        RampSpeed();
        score = Mathf.FloorToInt(transform.position.z);
    }

    void FixedUpdate()
    {
        if (!isAlive) return;
        AutoRun();
    }

    // ── Auto run forward ──────────────────────────────────────────────────────
    void AutoRun()
    {
        Vector3 vel = rb.linearVelocity;
        vel.z = currentSpeed;
        rb.linearVelocity = vel;
    }

    // ── Speed ramp ────────────────────────────────────────────────────────────
    void RampSpeed()
    {
        currentSpeed = Mathf.Min(currentSpeed + speedIncreaseRate * Time.deltaTime, maxSpeed);
    }

    // ── Input (New Input System) ──────────────────────────────────────────────
    void HandleInput()
    {
        // Lane left
        if (kb.leftArrowKey.wasPressedThisFrame || kb.aKey.wasPressedThisFrame) ShiftLane(-1);
        // Lane right
        if (kb.rightArrowKey.wasPressedThisFrame || kb.dKey.wasPressedThisFrame) ShiftLane(1);
        // Jump
        if (kb.spaceKey.wasPressedThisFrame || kb.wKey.wasPressedThisFrame) TryJump();
    }

    // ── Lane switching ────────────────────────────────────────────────────────
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

    // ── Jump ──────────────────────────────────────────────────────────────────
    void TryJump()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
        if (!isGrounded) return;

        Vector3 vel = rb.linearVelocity;
        vel.y = 0f;
        rb.linearVelocity = vel;

        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    // ── Collision detection ───────────────────────────────────────────────────
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
    }

    // ── Death ─────────────────────────────────────────────────────────────────
    void Die()
    {
        isAlive = false;
        rb.linearVelocity = Vector3.zero;
        Debug.Log("Hit obstacle! Score: " + score + " | Coins: " + coins);
    }

    // ── Public reset ──────────────────────────────────────────────────────────
    public void ResetPlayer(Vector3 startPosition)
    {
        transform.position = startPosition;
        currentLane = 1;
        currentSpeed = startSpeed;
        coins = 0;
        score = 0;
        isAlive = true;
        rb.linearVelocity = Vector3.zero;
    }

    // ── Debug UI ──────────────────────────────────────────────────────────────
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 200, 20), "Score: " + score);
        GUI.Label(new Rect(10, 30, 200, 20), "Coins: " + coins);
        GUI.Label(new Rect(10, 50, 200, 20), "Speed: " + currentSpeed.ToString("F1"));
    }
}