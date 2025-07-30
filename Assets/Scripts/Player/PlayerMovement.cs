using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private PlayerStats stats;
    private Animator animator;
    private PlayerControls controls;
    private Vector2 moveInput;

    private Vector2 lastDirection = Vector2.down; 
    
    public static float FacingDirection { get; private set; } = 1f;
    public static float VerticalMoveDirection { get; private set; } = 0f; 
    public static Vector2 LastDiscretizedDirection { get; private set; } = Vector2.down;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Gameplay.Move.performed += OnMovePerformed;
        controls.Gameplay.Move.canceled += OnMoveCanceled;
        controls.Gameplay.Enable();
    }

    private void OnDisable()
    {
        controls.Gameplay.Move.performed -= OnMovePerformed;
        controls.Gameplay.Move.canceled -= OnMoveCanceled;
        controls.Gameplay.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void FixedUpdate()
    {
        float currentSpeed = (stats != null) ? stats.moveSpeed : this.moveSpeed;
        rb.MovePosition(rb.position + moveInput * currentSpeed * Time.fixedDeltaTime);
    }

    private void Update()
    {
        UpdateAnimationAndDirection();
    }

    private void UpdateAnimationAndDirection()
    {
        if (animator == null) return;

        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("isMoving", isMoving);

        Vector2 directionToAnimate;

        if (isMoving)
        {
            directionToAnimate = DiscretizeDirection(moveInput);
            lastDirection = directionToAnimate;
        }
        else
        {
            directionToAnimate = lastDirection;
        }
        
        LastDiscretizedDirection = directionToAnimate;
        
        animator.SetFloat("moveX", directionToAnimate.x);
        animator.SetFloat("moveY", directionToAnimate.y);
        
        if (Mathf.Abs(directionToAnimate.x) > 0.1f)
        {
            FacingDirection = Mathf.Sign(directionToAnimate.x);
        }
        
        VerticalMoveDirection = directionToAnimate.y;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * FacingDirection, transform.localScale.y, transform.localScale.z);
    }
    
    private Vector2 DiscretizeDirection(Vector2 vector)
    {
        Vector2 normalizedVector = vector.normalized;
        float x = Mathf.Round(normalizedVector.x);
        float y = Mathf.Round(normalizedVector.y);
        Vector2 result = new Vector2(x, y);
        
        if (result.sqrMagnitude > 0.1f)
        {
            return result.normalized;
        }
        return normalizedVector;
    }
}