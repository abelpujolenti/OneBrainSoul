using ECS.Entities.AI.Combat;
using TMPro;
using UnityEngine;

public class PlayerCharacterController : MonoBehaviour
{
    public Transform orientation;
    public LayerMask groundMask;
    public RaycastHit groundHit;

    [Header("Properties")]
    [Range(1,3)]
    public int jumps = 1;
    [Range(0,3)]
    public float moveSpeedMultiplier = 1f;
    public bool pressToHover = false;

    [Header("Cooldowns")]
    public float ability1Cooldown = 3f;
    public float ability2Cooldown = 5f;

    [Header("Vertical Forces")]
    public float gravityStrength = 45f;
    public float hoverHeight = 2f;
    public float hoverStrength = 45f;
    public float hoverDamp = 20f;

    [Header("Rotation Correction")]
    public float rotationCorrectionStrength = 100f;
    public float rotationCorrectionDamp = 70f;

    [Header("Switch Mode")]
    public float switchModeSpeed = 30f;
    public float switchModeFalloffPower = 3f;
    public float switchModeYFactor = 0.3f;
    public float switchModeRotationFactor = 2f;
    public float switchModeMouseSpeed = .4f;

    public Rigidbody rb { get; private set; }
    public CapsuleCollider capsuleCollider { get; private set; }
    public FirstPersonCamera cam { get; private set; }
    public PlayerHealth health { get; private set; }
    public Hitstop hitstop { get; private set; }
    public bool onGround { get; private set; }
    public float xInput { get; private set; }
    public float yInput { get; private set; }
    public bool jumpInput { get; private set; }
    public bool ability1Input { get; private set; }
    public bool ability2Input { get; private set; }
    public bool switchModeInput { get; private set; }
    public bool canSwitch { get; set; } = true;
    public bool inCombat { get; private set; } = false;

    public float ability1Time = 0f;
    public float ability2Time = 0f;

    public MovementHandler movementHandler;

    public Transform hand;
    public GameObject display;
    public Canvas uiCanvas;
    private TextMeshProUGUI crosshair;
    public SwitchModeUI switchModeUI;
    public AllyIcon allyIcon;

    public Camera switchModeCamera;

    public float airTime = 0f;
    public float switchModeTime = 0f;
    Vector3 startPos;
    
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        cam = GetComponentInChildren<FirstPersonCamera>();
        cam.Setup();
        display.SetActive(false);
        allyIcon.gameObject.SetActive(false);
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        movementHandler = new GroundedMovementHandler();
        uiCanvas.gameObject.SetActive(true);
        crosshair = uiCanvas.GetComponentInChildren<TextMeshProUGUI>();
        hitstop = GetComponent<Hitstop>();
        health = GetComponent<PlayerHealth>();
        startPos = transform.position;
    }

    private void Update()
    {
        CheckGround();
        CheckInput();
        HandWobble();
        CalculateAirTime();
        CalculateCooldowns();
    }
    
    private void FixedUpdate()
    {        
        if (movementHandler.ShouldGravityApply(this))
        {
            ApplyGravity();
        }

        if (movementHandler.ShouldHoverApply(this))
        {
            Hover();
        }

        CorrectRotation();
        
        movementHandler.Move(this);
        VoidReturn();
    }

    private void ApplyGravity()
    {
        float gravity = jumpInput && pressToHover && rb.velocity.y < -4f ? gravityStrength * 0.2f : gravityStrength;
        rb.AddForce(Vector3.down * gravity , ForceMode.Acceleration);
    }

    private void Hover()
    {
        if (!onGround) return;
        float heightDiff = groundHit.distance - hoverHeight;
        rb.AddForce(Vector3.down * (heightDiff * hoverStrength + rb.velocity.y * hoverDamp), ForceMode.Acceleration);
    }

    private void CorrectRotation()
    {
        Quaternion correction = Quaternion.FromToRotation(transform.up, Vector3.up);
        correction.ToAngleAxis(out float alpha, out Vector3 axis);
        rb.AddTorque(axis.normalized * alpha * Mathf.Deg2Rad * rotationCorrectionStrength - rb.angularVelocity * rotationCorrectionDamp, ForceMode.Acceleration);
        Quaternion correction2 = Quaternion.FromToRotation(transform.forward, Vector3.forward);
        correction2.ToAngleAxis(out float alpha2, out Vector3 axis2);
        rb.AddTorque(axis2.normalized * alpha2 * Mathf.Deg2Rad * rotationCorrectionStrength - rb.angularVelocity * rotationCorrectionDamp, ForceMode.Acceleration);
    }

    private void VoidReturn()
    {
        if (transform.position.y < -30f)
        {
            transform.position = startPos;
        }
    }

    private void ApplyTerminalVelocity()
    {
        float tv = 12.5f;
        if (rb.velocity.y <= tv || rb.velocity.y > 0) return;
        Vector3 terminalVelocity = rb.velocity;
        terminalVelocity.y = tv;
        rb.velocity = terminalVelocity;
    }
    private void CheckGround()
    {
        float raycastMargin = 0.5f;
        onGround = Physics.Raycast(transform.position + Vector3.up * (raycastMargin + 0.05f), Vector3.down, out groundHit, hoverHeight * 5.0f, groundMask);
        if (!onGround) groundHit.distance = float.PositiveInfinity;
        onGround &= groundHit.distance < hoverHeight + raycastMargin && Vector3.Angle(groundHit.normal, Vector3.up) < 37.5f;
    }

    private void CheckInput()
    {
        xInput = Input.GetAxis("Horizontal");
        yInput = Input.GetAxis("Vertical");
        jumpInput = Input.GetButton("Jump");
        ability1Input = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        ability2Input = Input.GetKey(KeyCode.E) || Input.GetKey(KeyCode.Return);

        switchModeInput = Input.GetMouseButton(1);
    }

    private void HandWobble()
    {
        if (!onGround) return;
        Vector3 wandPos = new Vector3(Mathf.Sin(Time.time * 5f), Mathf.Sin(Time.time * 6f));
        float wobbleMagnitude = Mathf.Clamp(rb.velocity.magnitude / 25f, 0f, 1f);
        wandPos *= wobbleMagnitude * 0.05f;
        hand.localPosition = wandPos;
    }

    private void CalculateAirTime()
    {
        if (!onGround)
        {
            airTime += Time.deltaTime;
            if (airTime > 5f)
            {
            }
        }
    }

    private void CalculateCooldowns()
    {
        ability1Time = Mathf.Max(0f, ability1Time - Time.deltaTime);
        ability2Time = Mathf.Max(0f, ability2Time - Time.deltaTime);
    }

    public void SetCrosshairColor(Color color)
    {
        crosshair.color = color;
    }
}
