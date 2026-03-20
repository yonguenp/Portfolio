using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpHeight = 1.2f;
    public float gravity = -20f;

    private CharacterController _controller;
    private float _verticalVelocity;

    void Awake()
    {
        _controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (_controller == null) return;

        bool grounded = _controller.isGrounded;
        if (grounded && _verticalVelocity < 0f)
            _verticalVelocity = -2f;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(x, 0f, z);
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Move relative to camera yaw when possible
        var cam = Camera.main;
        if (cam != null)
        {
            Vector3 forward = cam.transform.forward; forward.y = 0f; forward.Normalize();
            Vector3 right = cam.transform.right; right.y = 0f; right.Normalize();
            move = (right * move.x + forward * move.z);
        }

        _controller.Move(move * moveSpeed * Time.deltaTime);

        if (grounded && Input.GetButtonDown("Jump"))
            _verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _verticalVelocity += gravity * Time.deltaTime;
        _controller.Move(Vector3.up * _verticalVelocity * Time.deltaTime);

        // Face movement direction
        Vector3 flatVel = new Vector3(move.x, 0f, move.z);
        if (flatVel.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(flatVel);
    }
}
