using Unity.Mathematics;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class PlayerController : MonoBehaviour
    {
    //Code adapted from "3rd Person Controller - Unity's New Input System" by One Wheel Studio found at "https://www.youtube.com/watch?v=WIl6ysorTE0"

        [Header("Input Field")]
        [SerializeField] private Inputs inputsAsset;
        [SerializeField] private InputAction move,run;
        

        [Header("RigidBody")] 
        [SerializeField] private Rigidbody rb;

        [Header("Forces & Speed")] [SerializeField]
        private float movementForce;
        [SerializeField] private float jumpForce;
        [SerializeField] private float maxSpeed;
        private Vector3 forceDirection = Vector3.zero;

        
        [Header("Camera")]
        [SerializeField]private Camera playerCam;

        [Header("Animator")] 
        [SerializeField] private Animator _animator;
        [SerializeField] private AnimationClip attackAnimation;

        [SerializeField] private bool canMove, canAttack, canUseStamina, canGatherWater, canEnterCar;

        [SerializeField] private BoxCollider attackCol;
        [SerializeField] private Inventory playerInventory;

        [SerializeField] private GameObject playerCamera, carCamera;

        [SerializeField] private CarController _carController;

        [SerializeField] private EscapeMenu _escapeMenu;
       
        
        private void Awake()
        { 
            playerInventory = GetComponent<Inventory>();
            rb = this.GetComponent<Rigidbody>();
            inputsAsset = new Inputs();
            _animator = this.GetComponent<Animator>();
            canMove = true;
            attackCol.enabled = false;
            canAttack = true;
            canEnterCar = false;
        }
        
        
        private void OnEnable()
        {
            EnablePlayerControl();
        }

        public void EnablePlayerControl()
        {
            inputsAsset.Player.Jump.started += DoJump;
            inputsAsset.Player.Attack.started += DoAttack;
            inputsAsset.Player.Pickup.started += PickUp;
            inputsAsset.Player.Use.started += Interact;
            inputsAsset.Player.Esc.started += OpenEscapeMenu;
            move = inputsAsset.Player.Move;
            run = inputsAsset.Player.Run;
            inputsAsset.Player.Enable();
        }

        private void OpenEscapeMenu(InputAction.CallbackContext obj)
        {
            _escapeMenu.ToggleEscapeMenu();
        }


        private void Interact(InputAction.CallbackContext obj)
        {
            if (canEnterCar)
            {
                playerCamera.SetActive(false);
                carCamera.SetActive(true);
                inputsAsset.Player.Disable();
                _carController.EnableControl();
                GetComponent<PlayerExitandEnterCar>().ToggleEnterExit();
            }
        }

        private void PickUp(InputAction.CallbackContext obj)
        {
            if (canGatherWater && playerInventory.GetWater() < 100)
            {
                _animator.SetTrigger("Pickup");
                playerInventory.SetWater(100 - playerInventory.GetWater());
            }
            else if(!canGatherWater)
            {
                PickUpItem();
            }

            if (canEnterCar)
            {
                _carController.StopCar();
                _carController.gameObject.transform.SetPositionAndRotation(_carController.gameObject.transform.position, Quaternion.Euler(Vector3.zero));
            }
        }

        void PickUpItem()
        {
            print("Item Pickup not implemented yet");
        }
        
        public void AtWaterSource(bool state)
        {
            canGatherWater = state;
            
        }

        private void DoAttack(InputAction.CallbackContext obj)
        {
            if(canAttack)
            {
                transform.rotation =Quaternion.Slerp(transform.rotation,quaternion.LookRotation(GetCameraRight(playerCam),Vector3.up), 2);
                _animator.SetTrigger("Attacking");
            }
        }


        private void OnDisable()
        {
            inputsAsset.Player.Jump.started -= DoJump;
            inputsAsset.Player.Attack.started -= DoAttack;
            inputsAsset.Player.Disable();
        }

        private void FixedUpdate()
        {
            if (run.IsPressed() && canMove && move.IsPressed() && canUseStamina)
            {
                _animator.SetBool("isRunning",true);
                forceDirection += move.ReadValue<Vector2>().y * GetCameraRight(playerCam) *movementForce * 2f;
                forceDirection += move.ReadValue<Vector2>().x * GetCameraForward(playerCam) * movementForce* 2f;
            }
            else if(canMove)
            {
                _animator.SetBool("isRunning",false);
                forceDirection += move.ReadValue<Vector2>().y * GetCameraRight(playerCam) *movementForce;
                forceDirection += move.ReadValue<Vector2>().x * GetCameraForward(playerCam)* movementForce;    
            }


            rb.AddForce(forceDirection,ForceMode.Impulse);
            forceDirection = Vector3.zero;

            
            if (move.IsPressed())
            {
                _animator.SetBool("isWalking",true);
            }
            else
            {
                _animator.SetBool("isWalking",false);
            }

            
            //fall faster to reduce "floating" effect
            if (rb.velocity.y < 0)
            {
                rb.velocity-= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime *5;
            }

            
            //lmit horizontal velocity
            Vector3 horizontalVelocity = rb.velocity;
            horizontalVelocity.y = 0;
            if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            {
                rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;
            }


            IsFalling();

            LookAt();
        }

        private void LookAt()
        {
            Vector3 dir = rb.velocity;
            dir.y = 0;
            
            if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && dir.sqrMagnitude > 0.1f)
            {
                this.rb.rotation = Quaternion.LookRotation(dir,Vector3.up);
            }
            else
            {
                rb.angularVelocity = Vector3.zero;
            }

        }



        private Vector3 GetCameraRight(Camera playerCamera)
        {
            Vector3 forward = playerCamera.transform.forward;
            forward.y = 0;
            return forward.normalized;
        }

        private Vector3 GetCameraForward(Camera playerCamera)
        {
            Vector3 right = playerCamera.transform.right;
            right.y = 0;
            return right.normalized;
        }

        private void DoJump(InputAction.CallbackContext context)
        {
            if (IsGrounded() && canMove)
            {
                forceDirection += Vector3.up * jumpForce;
                _animator.SetTrigger("Jump");
            }
        }

        private bool IsGrounded()
        {
            Ray ray = new Ray(this.transform.position + Vector3.up *0.25f, Vector3.down);

            if (Physics.Raycast(ray, out RaycastHit hit, 1f))
            {
                return true;
            }
            else
            {
                return false;
            }
            
        }
        private void IsFalling()
        {
            if (IsGrounded())
            {
                _animator.SetBool("isFalling", false);
            }
            else
            {
                _animator.SetBool("isFalling", true);
            }
        }

        //attack animation events
        public void EnableMovement()
        {
            canMove = true;
        }

        public void DisableAttack()
        {
            attackCol.enabled =false;
        }

        public void DisableMovement()
        {
            attackCol.enabled = true;
            canMove = false;
        }

        public void SetCanUseStamina(bool state)
        {
            canUseStamina = state;
        }

        public void CanAttackMethod(bool state)
        {
            canAttack = state;
        }

        public void CanEnterCar(bool state)
        {
            canEnterCar = state;
        }
        
        
    }


