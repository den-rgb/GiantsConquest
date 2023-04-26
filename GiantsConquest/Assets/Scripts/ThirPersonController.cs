
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class ThirPersonController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float jumpForce = 5.0f;
    public Animator animator;
    private float _lastAttackTime;
    public float attackCooldown = 1.0f;
    private Vector3 _velocity;


    private CharacterController _characterController;
    private Vector2 _inputVector;
    private bool _isJumping;
    private bool _isAttacking;
    public bool _isGrounded;
    public NegotiationIndex negotiationIndex;

    

    private PlayerInputActions _inputActions;
    private InputAction _moveAction;
    private InputAction _jumpAction;
    private InputAction _attackAction;
    private InputAction _interactAction;

    private InputAction _sprintAction;
    private bool _isSprinting;

    public float _turnSmoothTime = 0.1f;
    private float _turnSmoothVelocity;
    public float rotationSpeed = 360f;
    public Transform _cameraTransform;

    public float groundCheckDistance = 0.1f;
    public LayerMask groundLayer;


    private void Start()
    {
        _characterController = GetComponent<CharacterController>();
        negotiationIndex = GameObject.Find("MapGenerator").GetComponent<NegotiationIndex>();
        animator = GetComponent<Animator>();
        _turnSmoothVelocity = 0f;
        negotiationIndex.dialogueUI.SetActive(false);
    }

    void Update()
    {
        _isGrounded = _characterController.isGrounded;
        Vector3 moveDirection = new Vector3(_inputVector.x, 0, _inputVector.y);
        moveDirection.Normalize();
        

        if (moveDirection != Vector3.zero) 
            {
                float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + _cameraTransform.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref _turnSmoothVelocity, _turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }

        float currentMoveSpeed = _isSprinting ? moveSpeed * 2 : moveSpeed;
        _characterController.Move(moveDirection * currentMoveSpeed * Time.deltaTime);
        
        if (_isJumping && _characterController.isGrounded)
        {
            Debug.Log("Jumping");
            _velocity.y = jumpForce;
            _isJumping = false;
        }
        else
        {
            _velocity.y += Physics.gravity.y * 2 * Time.deltaTime;
        }

        _characterController.Move(_velocity * Time.deltaTime);

        // Update animator parameters for blend tree
        animator.SetBool("IsJumping", !_characterController.isGrounded);
        animator.SetFloat("Vertical", _inputVector.y);
        animator.SetFloat("Horizontal", _inputVector.x);
        animator.SetFloat("Speed", currentMoveSpeed);

    }

    private void OnEnable()
    {
        _inputActions = new PlayerInputActions();
        _moveAction = _inputActions.Player.Move;
        _jumpAction = _inputActions.Player.Jump;
        _attackAction = _inputActions.Player.Attack1;
        _sprintAction = _inputActions.Player.Sprint;
        _interactAction = _inputActions.Player.Interact;

        _moveAction.performed += OnMovePerformed;
        _moveAction.canceled += OnMoveCanceled;
        _jumpAction.performed += OnJumpPerformed;
        _attackAction.performed += OnAttackPerformed;
        _sprintAction.performed += OnSprintPerformed;
        _sprintAction.canceled += OnSprintCanceled;
        _interactAction.performed += OnInteractPerformed;

        _moveAction.Enable();
        _jumpAction.Enable();
        _attackAction.Enable();
        _sprintAction.Enable();
        _interactAction.Enable();
    }

    private void OnDisable()
    {
        _moveAction.performed -= OnMovePerformed;
        _moveAction.canceled -= OnMoveCanceled;
        _jumpAction.performed -= OnJumpPerformed;
        _attackAction.performed -= OnAttackPerformed;
        _sprintAction.performed -= OnSprintPerformed;
        _sprintAction.canceled -= OnSprintCanceled;
        _interactAction.performed -= OnInteractPerformed;

        _moveAction.Disable();
        _jumpAction.Disable();
        _attackAction.Disable();
        _sprintAction.Disable();
        _interactAction.Disable();
    }

    private void OnMovePerformed(InputAction.CallbackContext ctx)
    {
        _inputVector = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        _inputVector = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        _isJumping = true;
    }

    private void OnAttackPerformed(InputAction.CallbackContext ctx)
    {
        if (Time.time >= _lastAttackTime + attackCooldown)
        {
            _isAttacking = true;
            _lastAttackTime = Time.time;
            StartCoroutine(PerformAttack());
        }
    }

    private IEnumerator PerformAttack()
    {
        animator.SetTrigger("Punch");
        yield return new WaitForSeconds(0.3f);
        _isAttacking = false;
    }

    private void OnSprintPerformed(InputAction.CallbackContext ctx)
    {
        _isSprinting = true;
    }

    private void OnSprintCanceled(InputAction.CallbackContext ctx)
    {
        _isSprinting = false;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx) 
    {
        GameObject villageInRange = negotiationIndex.GetVillageInRange(100f);
        if (villageInRange != null && negotiationIndex.tradedVillages.Contains(villageInRange) == false || villageInRange == negotiationIndex.capital)
        {
            float negotiationDifficulty = negotiationIndex.negotiationDifficulty[villageInRange];
            negotiationIndex.negotiationIndexText.text = "Negotiation Index: " + negotiationDifficulty.ToString()  +"%\n Production Rate: " + negotiationIndex.productionRate[villageInRange].ToString() + "%";
            negotiationIndex.woodText.text = "Wood: " + negotiationIndex.villageResourceRequirements[villageInRange].woodRequired.ToString();
            negotiationIndex.stoneText.text = "Stone: " + negotiationIndex.villageResourceRequirements[villageInRange].stoneRequired.ToString();
            negotiationIndex.dialogueUI.SetActive(true);
        }
        else
        {
            negotiationIndex.dialogueUI.SetActive(false);
        }
    }

    


}
