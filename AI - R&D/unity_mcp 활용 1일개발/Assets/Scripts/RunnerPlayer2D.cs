using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class RunnerPlayer2D : MonoBehaviour
{
    public float runSpeed = 6f;
    public float jumpVelocity = 11f;
    public LayerMask groundMask = ~0;

    public float groundCheckDistance = 0.1f;

    Rigidbody2D _rb;
    Collider2D _col;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _col = GetComponent<Collider2D>();
    }

void FixedUpdate()
    {
        // Player stays in place horizontally; world will scroll instead.
        var v = _rb.linearVelocity;
        v.x = 0f;
        _rb.linearVelocity = v;
    }

void Update()
    {
        // Space jump (works even if Input Manager mapping differs)
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Jump")) && IsGrounded())
        {
            var v = _rb.linearVelocity;
            v.y = jumpVelocity;
            _rb.linearVelocity = v;
        }
    }

bool IsGrounded()
    {
        if (_col == null) return false;

        // More reliable than a tiny ray when colliders scale: check current contacts.
        return _col.IsTouchingLayers(groundMask);
    }
}
