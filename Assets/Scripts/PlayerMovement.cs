using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Components")]
    private Rigidbody2D rb;
    private Animator ani;
    private SpriteRenderer sr;

      [Header ("Layer Masks")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Movement Variables")]
    [SerializeField] private float moveAcceleration;
    [SerializeField] private float maxSpeed;
    [SerializeField] private float linearGroundDrag;
    private float horizontalDirection;
    private bool changeDirection => (rb.velocity.x > 0f && horizontalDirection < 0f || (rb.velocity.x < 0f && horizontalDirection > 0f));
    private bool facingRight = true;

    [Header ("Jump Variables")]
    [SerializeField] private float jumpForce;
    [SerializeField] private float linearAirDrag;
    [SerializeField] private float fallMultiplier;
    [SerializeField] private float lowJumpFallMultiplier;
    [SerializeField] private float jumpDelay;
    [SerializeField] private int extraJumps;
    private float jumpTimer;
    private int extraJumpValue;
    private bool canJump => Input.GetButtonDown("Jump") && (onGround || extraJumpValue > 0);

    [Header ("Ground Collision Variables")]
    [SerializeField] private float groundRaycastLength;
    [SerializeField] private Vector3 groundRaycastOffset;
    private bool onGround;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        ani = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update() 
    {
        horizontalDirection = GetInput().x;
        if(canJump) Jump();

        // Animation
        ani.SetBool("isGrounded", onGround);
        ani.SetFloat("horizontalDirection", Mathf.Abs(horizontalDirection));
        if (horizontalDirection < 0f && facingRight)
        {
            Flip();
        }
        else if (horizontalDirection > 0f && !facingRight)
        {
            Flip();
        }
    }

    private void FixedUpdate() 
    {
        CheckCollisions();
        Move();
        if(onGround)
        {
            extraJumpValue = extraJumps;
            ApplyLinearGroundDrag();
        }
        else 
        {
            ApplyLinearAirDrag();
            FallMultiplier();
        }
    }

    private Vector2 GetInput()
    {
        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    private void Move()
    {
        rb.AddForce(new Vector2(horizontalDirection, 0f) * moveAcceleration);

        if(Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);
    }

    private void ApplyLinearGroundDrag()
    {
        if (Mathf.Abs(horizontalDirection) < 0.4f || changeDirection)
        {
            rb.drag = linearGroundDrag;
        }
        else
        {
            rb.drag = 0f;
        }
    }

    private void ApplyLinearAirDrag()
    {
        rb.drag = linearAirDrag;
    }

    private void Jump()
    {
        if(!onGround)
            extraJumpValue--;
        
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void FallMultiplier()
    {
        if (rb.velocity.y < 0)
        {
            rb.gravityScale = fallMultiplier;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.gravityScale = lowJumpFallMultiplier;
        }
        else 
        {
            rb.gravityScale = 1f;
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }
    
    private void CheckCollisions()
    {
        onGround = Physics2D.Raycast(transform.position + groundRaycastOffset, Vector2.down, groundRaycastLength, groundLayer) || Physics2D.Raycast(transform.position - groundRaycastOffset, Vector2.down, groundRaycastLength, groundLayer);
    }
}