using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TerrainEngine;
using Unity.Netcode;
//using Photon.Pun;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UserInterface;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;
using UnityEditor;
using UnityEngine.Serialization;
using Menu = UserInterface.Menu;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace XRControls{

    /* Note: animations are called via the controller for the character using animator null checks */

    /// <summary>
    /// This script handles all desktop interactions pertaining to the DesktopRig in the scene.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class FirstPersonController : MonoBehaviour, DesktopControls.IPlayerActions {

        #region FIELDS
        [Header("Player")]
        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 4.0f;
        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 6.0f;
        [Tooltip("Rotation speed of the character")]
        public float RotationSpeed = 1.0f;
        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;
        public bool analogMovement;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;
        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;
        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;
        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;
        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;
        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;
        
        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 90.0f;
        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -90.0f;

        // camera
        private float _cameraTargetPitch;

        // player
        private float _speed;
        private float _animationBlend;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        
        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

        private Animator _animator;
        private CharacterController _controller;
        private GameObject _mainCamera;

        private const float _threshold = 0.01f;

        [SerializeField] private bool _hasAnimator;

        // input
        private DesktopControls _controls;
        [SerializeField] private ActionBasedController _actionBasedController;
        private Vector3 move;
        private Vector2 look;
        private bool sprint;
        public bool jump;
        private bool rotateAnchor;
        private Vector2 anchorRotation;

        //platform movement (controlled by player)
        [Header("Platform GameObject")]
        [SerializeField] private GameObject platform;
        [SerializeField] private GameObject terrainTiles;
        private bool movePlatform = false;
        
        public bool cursorLocked; 
        private Vector2 elevatePlatform;

        #endregion

        #region MONO

        private void Start() {
            // get a reference to our main camera
            if (_mainCamera == null) {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }
            
            _controls = new DesktopControls();
            _controls.Player.SetCallbacks(this);
            _controls.Enable();
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            cursorLocked = false; //start with cursor unlocked for desktop player
        }

        private void Update() {
            _hasAnimator = TryGetComponent(out _animator);

            if (!GameState.IsVR)
            {
                GroundedCheck();
            }
            if (cursorLocked)
            {
                if(movePlatform) MovePlatform();
                else Move();
                
                // why is this here?
                /*_animator.SetFloat(_animIDSpeed, 0);
                _animator.SetFloat(_animIDMotionSpeed, 0);*/
            }
        }

        private void LateUpdate() {
            if (cursorLocked) {

                if(!GameState.IsVR)
                {
                    CameraRotation();
                }
            }
        }

        #endregion


        #region SPECIFIC
        
        private void AssignAnimationIDs() {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        public void TeleportPlayer(Transform destination, bool matchRotation) {
            transform.position = destination.position;
            if (!matchRotation) return;
            transform.rotation = destination.rotation;
            _mainCamera.transform.rotation = destination.rotation;
        }
        
        private void GroundedCheck() {
            // set sphere position, with offset
            var spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);
            
            // update animator if using character
            if (_hasAnimator) {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation() {
            // if item anchor is using mouse vector
            if (rotateAnchor || !cursorLocked) { return; }
            
            // if there is an input
            if (!(look.sqrMagnitude >= _threshold)) return;
            _cameraTargetPitch += look.y * SettingsController.MouseSensitivity() * Time.deltaTime;
            _rotationVelocity = look.x * SettingsController.MouseSensitivity() * Time.deltaTime;

            // clamp our pitch rotation
            _cameraTargetPitch = ClampAngle(_cameraTargetPitch, BottomClamp, TopClamp);

            // Update Cinemachine camera target pitch
            
            GetComponentInChildren<Camera>().transform.localRotation = Quaternion.Euler(_cameraTargetPitch, 0.0f, 0.0f);

            // rotate the player left and right
            transform.Rotate(Vector3.up * _rotationVelocity);
        }

        private void Move() {
            // set target speed based on move speed, sprint speed and if sprint is pressed
            var targetSpeed = sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (move == Vector3.zero) targetSpeed = 0.0f;

            // a reference to the players current horizontal velocity
            var currentHorizontalSpeed = new Vector3(_controller.velocity.x, 0.0f, _controller.velocity.z).magnitude;

            var speedOffset = 0.1f;
            var inputMagnitude = analogMovement ? move.magnitude : 1f;

            // accelerate or decelerate to target speed
            if (currentHorizontalSpeed < targetSpeed - speedOffset || currentHorizontalSpeed > targetSpeed + speedOffset) {
                // creates curved result rather than a linear one giving a more organic speed change
                // note T in Lerp is clamped, so we don't need to clamp our speed
                _speed = Mathf.Lerp(currentHorizontalSpeed, targetSpeed * inputMagnitude, Time.deltaTime * SpeedChangeRate);

                // round speed to 3 decimal places
                _speed = Mathf.Round(_speed * 1000f) / 1000f;
            } 
            else {
                _speed = targetSpeed;
            }
            
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);

            // normalise input direction
            var inputDirection = new Vector3(move.x, 0.0f, move.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (move != Vector3.zero) {
                // move
                inputDirection = transform.right * move.x + transform.forward * move.y;
            }

            // move the player
            _controller.Move(inputDirection.normalized * (_speed * Time.deltaTime) +
                            new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
            
            // update animator if using character
            if (_hasAnimator) {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax) {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected() {
            var transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            var transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }


        //singleplayer & hosts in multiplayer will be able to move the platform
        public void MovePlatform()
        {
            float speed;

            //not moving
            if (move == Vector3.zero) speed = 0f;

            //moving
            else
            {
                speed = SettingsController.PlatformSpeed();
                Vector3 inputDirection = transform.right * move.x + transform.forward * move.y + transform.up * move.z;
                
               // platform.transform.position = Vector3.MoveTowards(platform.transform.position, platform.transform.position + inputDirection.normalized * speed, speed * Time.deltaTime); 
                
                /*Vector3 cameraForward =
                    new Vector3((movePlatform.y == 0)? gameObject.GetComponentInChildren<Camera>().transform.forward.x : 0, movePlatform.y,
                        (movePlatform.y == 0) ? gameObject.GetComponentInChildren<Camera>().transform.forward.z : 0);*/
                
                //moving tiles
                terrainTiles.transform.position = Vector3.MoveTowards(terrainTiles.transform.position, terrainTiles.transform.position - inputDirection.normalized * speed, speed * Time.deltaTime);
            }

        }

        public float GetSpeed()
        {
            return _speed;
        }
    

        #endregion


        #region INPUT

        public void OnMove(InputAction.CallbackContext context) 
        { 
            if(cursorLocked) move = context.ReadValue<Vector3>();
        }
        public void OnTeleport(InputAction.CallbackContext context) { }

        public void OnLook(InputAction.CallbackContext context) { look = context.ReadValue<Vector2>(); }

        public void OnSelect(InputAction.CallbackContext context) { }

        public void OnHapticDevice(InputAction.CallbackContext context) { }

        public void OnRotateAnchor(InputAction.CallbackContext context) { }

        public void OnTranslateAnchor(InputAction.CallbackContext context) { }

        public void OnSprint(InputAction.CallbackContext context) { sprint = context.ReadValueAsButton(); }

        public void OnJump(InputAction.CallbackContext context) { jump = context.ReadValueAsButton(); }

        public void OnToggleMode(InputAction.CallbackContext context) {
            // This would lock the players movement if they pressed the F key.
            // Disabled as of 10/10/2022 as we instead lock the desktop player's movement
            // when they interact with an input field.
            /*
            if (context.performed) {
                cursorUnlocked = !cursorUnlocked;
                Cursor.lockState = cursorUnlocked ? CursorLockMode.None : CursorLockMode.Locked;
            }*/
        }

        public void OnRotateAnchorToggle(InputAction.CallbackContext context) {
/*            rotateAnchor = context.ReadValueAsButton();
            if (rotateAnchor) { _actionBasedController.rotateAnchorAction.EnableDirectAction(); }
            else { _actionBasedController.rotateAnchorAction.DisableDirectAction(); }*/
        }

        public void OnMovePlatform(InputAction.CallbackContext context)
        {
            if (context.performed && cursorLocked) // && InfoScreen.canInteract) -> turn off for conference build (for now)
            {
                movePlatform = !movePlatform;
                TerrainTools.Instance.ToggleShift(movePlatform);
            }
        }
        
        public void OnElevatePlatform(InputAction.CallbackContext context)
        {
            elevatePlatform = context.ReadValue<Vector2>();
        }

        public void OnToggleMenu(InputAction.CallbackContext context)
        {
            if (context.performed && GameState.InTerrain)
            {
                Menu.LockCursor.Invoke(!cursorLocked);
                TerrainTools.Instance.ToggleTab(!cursorLocked);
            }
        }

        public void OnEscape(InputAction.CallbackContext context)
        {
            if (context.performed && (!GameState.InMultiuser || NetworkManager.Singleton.IsHost))
            {
                MainMenu.OpenPrimaryMenus(true);
                MainMenu.Instance.CloseAllMenus();
                MainMenu.OpenMenu(true);
            }
        }


        #endregion

    }
}