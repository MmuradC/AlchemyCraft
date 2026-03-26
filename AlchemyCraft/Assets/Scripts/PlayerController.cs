using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider2D))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float jumpForce = 14f;

    [Header("Dash")]
    public float dashSpeed = 18f;
    public float dashDuration = 0.18f;
    public float dashCooldown = 0.8f;

    [Header("Tilt (slope lean)")]
    public float tiltAngle = 20f;
    public float tiltSlopeThreshold = 15f;   // degrees before tilt animation fires

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.18f;
    public LayerMask groundLayer;

    [HideInInspector] public bool isGrounded;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public float horizontalInput;

    Rigidbody2D rb;
    Animator anim;
    bool facingRight = true;
    float dashTimer;
    float dashCooldownTimer;
    float dashDirection;

    void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        HandleJump();
        HandleDash();
        HandleFlip();
        HandleTilt();
        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = new Vector2(dashDirection * dashSpeed, 0f);
            dashTimer -= Time.fixedDeltaTime;
            if (dashTimer <= 0f)
            {
                isDashing      = false;
                rb.gravityScale = 3f;
            }
            return;
        }

        rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            anim.SetTrigger("JumpTrigger");
        }
    }

    void HandleDash()
    {
        dashCooldownTimer -= Time.deltaTime;
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0f)
        {
            isDashing          = true;
            dashTimer          = dashDuration;
            dashCooldownTimer  = dashCooldown;
            dashDirection      = facingRight ? 1f : -1f;
            rb.gravityScale    = 0f;
            anim.SetTrigger("DashTrigger");
        }
    }

    void HandleFlip()
    {
        if (horizontalInput > 0 && !facingRight) Flip();
        if (horizontalInput < 0 &&  facingRight) Flip();
    }

    void HandleTilt()
    {
        // Ray downward from feet to detect slope normal
        RaycastHit2D hit = Physics2D.Raycast(groundCheck.position, Vector2.down, 0.5f, groundLayer);
        if (hit && isGrounded)
        {
            float slopeAngle = Vector2.SignedAngle(Vector2.up, hit.normal);
            float lean       = Mathf.Clamp(-slopeAngle, -tiltAngle, tiltAngle);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                Quaternion.Euler(0f, 0f, lean),
                Time.deltaTime * 10f);

            anim.SetBool("isTilting", Mathf.Abs(slopeAngle) > tiltSlopeThreshold);
        }
        else
        {
            transform.rotation = Quaternion.Lerp(
                transform.rotation, Quaternion.identity, Time.deltaTime * 10f);
            anim.SetBool("isTilting", false);
        }
    }

    void UpdateAnimator()
    {
        anim.SetFloat("Speed",      Mathf.Abs(horizontalInput));
        anim.SetFloat("yVelocity",  rb.linearVelocity.y);
        anim.SetBool ("isGrounded", isGrounded);
        anim.SetBool ("isDashing",  isDashing);
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(
            -transform.localScale.x,
             transform.localScale.y,
             transform.localScale.z);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}