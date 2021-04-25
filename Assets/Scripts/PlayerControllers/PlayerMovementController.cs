using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
class MovementAdvancedOptions
{
    [Min(0)]
    [SerializeField] public float jumpBuffer = 0.15f;
    [Min(0)]
    [SerializeField] public float coyoteTime = 0.075f;
}

[Serializable]
class DetectionAdvancedOptions
{
    [Tooltip("Will ignore these layers when checking if the player is grounded.")]
    [SerializeField] public LayerMask ignoreLayers = new LayerMask();
    [Tooltip("The amount of gameobjects that it will check for. (Recommended 3)")]
    [Min(2)]
    [SerializeField] public int checkDepth = 3;
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerMovementController : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [SerializeField] private float gravity = -1f;
    [SerializeField] private float jumpStrength = 10f;
    [SerializeField] MovementAdvancedOptions advanced = null;

    //[Header("TODO:")]
    //[SerializeField] private float accelTime = 0f;
    //[SerializeField] private float decelTime = 0f;

    [Header("Ground Detection")]
    [Tooltip("The Collider2D that will check if the player is grounded.")]
    [SerializeField] private Collider2D groundDetector = null;

    [Header("Side Detection")]
    [Tooltip("The Collider2D that will check if the player is touching the left side.")]
    [SerializeField] private Collider2D leftDetector = null;
    [Tooltip("The Collider2D that will check if the player is touching the right side.")]
    [SerializeField] private Collider2D rightDetector = null;

    [Header("Ceiling Detection")]
    [Tooltip("The Collider2D that will check if the player is grounded.")]
    [SerializeField] private Collider2D ceilingDetector = null;

    [Space]
    [SerializeField] private DetectionAdvancedOptions detectionAdvanced = null;

    [Header("Events")]
    [SerializeField] private UnityEvent OnJumpEvent = null;
    [SerializeField] private UnityEvent OnLandEvent = null;
    
    [HideInInspector] public bool useInertia;
    [HideInInspector] public bool stopMovement;

    [HideInInspector] public float yMov = 0f;
    private float xMov = 0f;

    private float jumpBufferCountdown = 0f;
    private float hangtimeCountdown = 0f;

    private bool onJump = false;
    private bool wasGrounded = false;

    private bool isGrounded = false;
    [HideInInspector] public bool isGroundedRaw = false;
    private bool isTouchingCeiling = false;
    //private bool isTouchingRight = false;
    //private bool isTouchingLeft = false;

    private Rigidbody2D _rb;
    private PlayerDamageController _pdc;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _pdc = GetComponent<PlayerDamageController>();
    }

    private void Update()
    {
        if (!_pdc.die)
        {
            isGroundedRaw = CheckForTouching(groundDetector);

            if (!stopMovement)
            {
                isGrounded = CheckForTouching(groundDetector);
            }
            else
            {
                isGrounded = false;
            }
            isTouchingCeiling = CheckForTouching(ceilingDetector);

            if (hangtimeCountdown >= 0)
            {
                hangtimeCountdown -= Time.deltaTime;
            }
            if (jumpBufferCountdown >= 0)
            {
                jumpBufferCountdown -= Time.deltaTime;
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                if (isGrounded || hangtimeCountdown >= 0)
                {
                    onJump = true;
                }
                else
                {
                    jumpBufferCountdown = advanced.jumpBuffer;
                }
            }

            if (isGrounded && jumpBufferCountdown >= 0)
            {
                onJump = true;
            }
        }
        else
        {
            _rb.gravityScale = 1f;
            xMov = 0f;
            yMov = 0f;
        }
    }

    private void FixedUpdate()
    {
        if (!_pdc.die)
        {
            Move();
        }
    }

    public void OnJump()
    {
        //Debug.Log("Jump");
        //StartCoroutine(SFXController.instance.Play(SFX.JUMP, 1f));
        SFXController.instance.Play(SFX.JUMP, 0.7f);
    }

    public void OnLand()
    {

        //Debug.Log("Land");
    }

    private void Move()
    {
        xMov = 0f;
        if (Input.GetKey(KeyCode.A))
        {
            xMov = -1f;
        }
        if (Input.GetKey(KeyCode.D))
        {
            xMov = 1f;
        }

        xMov *= moveSpeed;

        if (isGrounded)
        {
            useInertia = false;
            yMov = -1f;
            if (!wasGrounded)
            {
                wasGrounded = true;
                OnLandEvent.Invoke();
            }
        }
        else
        {
            if (wasGrounded)
            {
                hangtimeCountdown = advanced.coyoteTime;
                wasGrounded = false;
            }

            if (isTouchingCeiling)
            {
                yMov = 0f;
                jumpBufferCountdown = 0f;
            }
            yMov += gravity;
        }
        if (hangtimeCountdown >= 0 || isGrounded)
        {
            if (onJump)
            {
                yMov = jumpStrength;
                onJump = false;
                wasGrounded = false;
                OnJumpEvent.Invoke();
            }
        }
        if (useInertia)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, yMov);
        }
        else
        {
            if (!_pdc.die)
            {
                _rb.velocity = new Vector2(xMov, yMov);
            }
        }
    }

    private bool CheckForTouching(Collider2D check)
    {
        Collider2D[] colliders = new Collider2D[detectionAdvanced.checkDepth];

        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false;
        filter.useLayerMask = true;
        filter.layerMask = ~detectionAdvanced.ignoreLayers;

        check.OverlapCollider(filter, colliders);
        foreach (Collider2D collider in colliders)
        {
            if (collider != null)
            {
                if (collider.transform != this.transform && collider != groundDetector && collider != leftDetector && collider != rightDetector)
                {
                    return true;
                }
            }
        }
        return false;
    }


    //private void Update()
    //{
    //    isGrounded = CheckForTouching(groundDetector);
    //    isTouchingCeiling = CheckForTouching(ceilingDetector);
    //    isTouchingLeft = CheckForTouching(leftDetector);
    //    isTouchingRight = CheckForTouching(rightDetector);

    //    if (hangtimeCountdown >= 0)
    //    {
    //        hangtimeCountdown -= Time.deltaTime;
    //    }
    //    if (jumpBufferCountdown >= 0)
    //    {
    //        jumpBufferCountdown -= Time.deltaTime;
    //    }

    //    if (Input.GetKeyDown(KeyCode.W))
    //    {
    //        if (isGrounded || hangtimeCountdown >= 0)
    //        {
    //            onJump = true;
    //        }
    //        else
    //        {
    //            jumpBufferCountdown = advanced.jumpBuffer;
    //        }
    //    }

    //    if (isGrounded && jumpBufferCountdown >= 0)
    //    {
    //        onJump = true;
    //    }
    //}

    //private void FixedUpdate()
    //{
    //    Move();
    //}

    //public void OnJump()
    //{
    //    //Debug.Log("Jump");
    //}

    //public void OnLand()
    //{

    //    //Debug.Log("Land");
    //}

    //private void Move()
    //{
    //    xMov = 0f;
    //    if (Input.GetKey(KeyCode.A))
    //    {
    //        if (!isTouchingLeft)
    //        {
    //            xMov = -1f;
    //        }
    //    }
    //    if (Input.GetKey(KeyCode.D))
    //    {
    //        if (!isTouchingRight)
    //        {
    //            xMov = 1f;
    //        }
    //    }

    //    if (isGrounded || isTouchingLeft || isTouchingRight)
    //    {
    //        useInertia = false;
    //    }

    //    xMov *= moveSpeed;

    //    if (isGrounded)
    //    {
    //        yMov = -1f;
    //        if (!wasGrounded)
    //        {
    //            wasGrounded = true;
    //            OnLandEvent.Invoke();
    //        }
    //    }
    //    else
    //    {
    //        if (wasGrounded)
    //        {
    //            hangtimeCountdown = advanced.coyoteTime;
    //            wasGrounded = false;
    //        }

    //        if (isTouchingCeiling)
    //        {
    //            yMov = 0f;
    //            jumpBufferCountdown = 0f;
    //        }
    //        yMov += gravity;
    //    }
    //    if (hangtimeCountdown >= 0 || isGrounded)
    //    {
    //        if (onJump)
    //        {
    //            yMov = jumpStrength;
    //            onJump = false;
    //            wasGrounded = false;
    //            OnJumpEvent.Invoke();
    //        }
    //    }

    //    if (useInertia)
    //    {
    //        _rb.velocity = new Vector2(_rb.velocity.x, yMov);
    //    }
    //    else
    //    {
    //        _rb.velocity = new Vector2(xMov, yMov);
    //    }
    //}

    //private bool CheckForTouching(Collider2D check)
    //{
    //    Collider2D[] colliders = new Collider2D[detectionAdvanced.checkDepth];

    //    ContactFilter2D filter = new ContactFilter2D();
    //    filter.useTriggers = false;
    //    filter.useLayerMask = true;
    //    filter.layerMask = ~detectionAdvanced.ignoreLayers;

    //    check.OverlapCollider(filter, colliders);
    //    foreach (Collider2D collider in colliders)
    //    {
    //        if (collider != null)
    //        {
    //            if (collider.transform != this.transform && collider != groundDetector && collider != leftDetector && collider != rightDetector)
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}
}