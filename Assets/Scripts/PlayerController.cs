using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    [SerializeField] private Rigidbody2D rb;

    [Header("Joystick")]
    public Joystick joystick;

    bool isGrounded;

    void Awake()
    {
        rb.freezeRotation = true;
    }

    void FixedUpdate()
    {
        float x = joystick != null ? joystick.Horizontal : Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(x * speed, rb.linearVelocity.y);

        if (Mathf.Abs(x) > 0.01f)
        {
            Vector3 s = transform.localScale;
            s.x = Mathf.Sign(x) * Mathf.Abs(s.x);
            transform.localScale = s;
        }
    }

    public void Jump()
    {
        if (!isGrounded)
            return;

        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                return;
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        isGrounded = false;
    }
}
