using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Rigidbody2D rb;

    private Vector2 movementInput;
    private PlayerControls controls;
    private PlayerStats stats;

    private void Awake()
    {
        controls = new PlayerControls();
        stats = GetComponent<PlayerStats>();
    }

    private void OnEnable()
    {
        controls.Gameplay.Enable();
        controls.Gameplay.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => movementInput = Vector2.zero;
    }

    private void OnDisable()
    {
        controls.Gameplay.Move.performed -= ctx => movementInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled -= ctx => movementInput = Vector2.zero;
        controls.Gameplay.Disable();
    }


    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput * stats.moveSpeed * Time.fixedDeltaTime);
    }


}
