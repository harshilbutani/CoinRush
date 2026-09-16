using Fusion;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    [SerializeField] private Rigidbody2D rb;

    [Header("Joystick")]
    public Joystick joystick;

    [Header("Player Name")]
    [SerializeField] private TextMeshProUGUI nameText;

    private NetworkObject networkObject;
    private NetworkPlayerData networkPlayerData;
    private bool jumpRequested;

    public static PlayerController LocalPlayer { get; private set; }
    public bool HasInputAuthority => networkObject != null && networkObject.HasInputAuthority;
    public bool HasStateAuthority => networkObject != null && networkObject.HasStateAuthority;

    void Awake()
    {
        transform.localScale = Vector3.one;

        if (rb != null)
            rb.freezeRotation = true;

        networkObject = GetComponent<NetworkObject>();
        networkPlayerData = GetComponent<NetworkPlayerData>();
        if (joystick == null)
            // joystick = FindObjectOfType<Joystick>();
            this.joystick = UIManager.Instance.GetScreen<GameScreen>()?.joystick;
    }

    void OnEnable()
    {
        if (HasInputAuthority)
            LocalPlayer = this;
    }

    void OnDisable()
    {
        if (LocalPlayer == this)
            LocalPlayer = null;
    }

    void FixedUpdate()
    {
        if (networkObject == null || networkPlayerData == null)
            return;

        if (HasInputAuthority)
        {
            LocalPlayer = this;
            UpdateNameLabel();
        }

        if (HasStateAuthority)
            networkPlayerData.CapturePosition();

        LockRotation();
    }

    public void RegisterAsLocalPlayer()
    {
        if (HasInputAuthority)
        {
            LocalPlayer = this;
            Debug.Log($"[PlayerController] Local player registered: {name}, Object={networkObject.Id}");
        }
    }

    public void RequestJump()
    {
        jumpRequested = true;
        Debug.Log($"[PlayerController] Jump requested. Player={name}, HasInputAuthority={HasInputAuthority}");
    }

    private void LockRotation()
    {
        if (rb == null)
            return;

        rb.rotation = 0f;
        rb.angularVelocity = 0f;
        transform.rotation = Quaternion.identity;
    }

    public PlayerInputData ReadInput()
    {
        float horizontal = joystick != null ? joystick.Horizontal : 0f;
        bool jump = jumpRequested;
        jumpRequested = false;

        if (jump)
            Debug.Log($"[PlayerController] Jump input read by Fusion. Player={name}");

        return new PlayerInputData
        {
            Horizontal = horizontal,
            Jump = jump
        };
    }

    public void ApplyInput(PlayerInputData input)
    {
        if (rb == null)
            return;

        LockRotation();
        rb.linearVelocity = new Vector2(input.Horizontal * speed, rb.linearVelocity.y);

        if (input.Jump)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            Debug.Log($"[PlayerController] Jump applied immediately. Player={name}");
        }
    }

    public void UpdateNameLabel()
    {
        if (nameText != null && networkPlayerData != null)
            nameText.text = networkPlayerData.PlayerName.ToString();
    }

}
