using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
using Photon.Pun;
using UnityEngine.UI;
using UnityEngine.Events;

public class PlayerMovementAdvanced : MonoBehaviourPunCallbacks, IPunObservable
{
    //[Header("Player")]
    //public GameObject playerPrefab;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float slideSpeed; //if u change slide speed keep and eye on overtimeslidespeed as that is hardcoded
    public float overtimeslideSpeed;
    public float wallrunSpeed;

    private float desiredMoveSpeed;
    private float lastDesiredMoveSpeed;

    public float speedIncreaseMultiplier;
    public float slopeIncreaseMultiplier;

    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds + Mouse")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;
    
    public Transform cameraTransform;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit;
    private bool exitingSlope;
    

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    public Vector3 moveDirection;

    public bool readyToSlide = true;

    Rigidbody rb;
    [Header("Tagging")]
    [SerializeField] private Material taggedColor;
    private Material initialColor; // maybe serialize?
    private float tagBackTimer; //tag back values
    [SerializeField] private float tagBackDuration;
    [SerializeField] private float tagRange;
    public AudioSource source;
    public AudioClip taggedSound;
    

    [Header("Radial Indicator")]
    [SerializeField] private float indicatorTimer = 100% 10;
    [SerializeField] private float maxIndicatorTimer = 100% 10;
    [SerializeField] public Image radialIndicatorUI = null;
    [SerializeField] private UnityEvent myEvent = null;
    private bool shouldUpdate = false;
    public float cooldownPercentage;


    [Header("Random")]
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private float timeSpentTagged;
    [SerializeField] private TMP_Text speedDisplay;
    [SerializeField] private TMP_Text tagDisplay;





    public MovementState state;
    public enum MovementState
    {
        freeze, //maybe don't use
        walking,
        sprinting,
        wallrunning,
        crouching,
        sliding,
        air
    }

    public bool isTagged;

    public bool freeze;

    public bool activeHook;

    public bool sliding;
    //public bool crouching; //worked without it, might break it
    public bool wallrunning;

    private void Awake()
    {
        initialColor = GetComponentInChildren<MeshRenderer>().material; // if serialize initial color - no need for this
    }

    private void Start()
    {

        //GameObject myPlayer = (GameObject)PhotonNetwork.Instantiate(this.playerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);

        //myPlayer.GetComponentInChildren<Camera>().enabled = true;

        //cc = GetComponent<CharacterController>(); using character controller

        cameraTransform = GetComponentInChildren<Camera>().transform;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<PlayerCam>().enabled = false;
            GetComponentInChildren<Camera>().enabled = false;
            GetComponentInChildren<AudioListener>().enabled = false;
            GetComponent<PlayerMovementAdvanced>().radialIndicatorUI.enabled = false;

            
            //GetComponentInChildren<FirstPersonController>().enabled = false;
            //GetComponentInChildren<Sliding>().enabled = false;
            //GetComponentInChildren<AudioListener>().enabled = false;
        }

        UpdateNameDisplay();
  

                rb = GetComponent<Rigidbody>();
                rb.freezeRotation = true;

                readyToJump = true;

                startYScale = transform.localScale.y;
    }

    private void Update()
    {

        if (photonView.IsMine)
        {
            // ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            MyInput();
            SpeedControl();
            StateHandler();

            // handle drag
            if (grounded & !activeHook)
                rb.drag = groundDrag;
            else
                rb.drag = 0;


            if (tagBackTimer > 0f)
            {
                tagBackTimer -= Time.deltaTime;
            }

            if (Input.GetMouseButtonDown(1) && LocalPauseMenu.GameIsPaused == false)
            {
                RaycastHit hit;
                //var ray = Camera.main.ScreenPointToRay(Input.mousePosition); //sends a ray out from center of screen
                var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Physics.Raycast(ray, out hit, tagRange))
                {
                    if (hit.collider.tag == "Player" && isTagged && tagBackTimer <= 0f) // Hits player, while tagged, and tag back timer = 0
                    {
                        //child.GetComponent<blockCollide>().BroadcastMessage("updateLocalState", true);

                        // untag self
                        GetComponent<PlayerMovementAdvanced>().photonView.RPC("onUntagged", RpcTarget.AllBuffered);

                        // tag who raycast hits
                        //GetComponent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered);
                        //GameObject.Find("Overlap").tag = "tag";
                        //hit.transform.gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                        hit.transform.gameObject.GetComponent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered);
                    }
                }
            }

            if (PlayerR.GetComponent<MeshRenderer>().material.color == Color.red && isTagged == false)
            {
                Debug.Log("color red tag runs");
                GetComponent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered);
                PlayerR.GetComponentInParent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered);

                // music que yourself been tagged
                
            }

            if (Input.GetKeyDown(KeyCode.Semicolon) && isTagged == false) // Emergency tag self - only use when no-one in game is tagged
            {
                Debug.Log("Tagger reset");
                GetComponent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered);
                PlayerR.GetComponentInParent<PlayerMovementAdvanced>().photonView.RPC("onTagged", RpcTarget.AllBuffered);
            }

            // if tagged, increase time tracked as tagged
            if (isTagged)
            {
                timeSpentTagged += Time.deltaTime;
                UpdateNameDisplay(); // updates it every time
            }
            else
            {
                UpdateNameDisplay();
            }

            // ### Onscreen Speed Display

            //var kph = (moveDirection * Time.deltaTime).magnitude * 3.6;
            var kph = GetComponent<Rigidbody>().velocity.magnitude; //  * 3.6 - for kph

            speedDisplay.text = ($"{kph.ToString("F1")} ms");

            if(tagBackTimer > 0)
            {
                var tagTimeDisplay = tagBackTimer;

                tagDisplay.text = tagTimeDisplay.ToString("F1");
            }
            else
            {
                tagDisplay.text = "";
            }

            bool keepVisible = false;
            // grapplingCdTimer is variable I need from Hookgrapple.cs & maxGrappleDistance for the indicator to appear
            // yeet is like start of successful grapple so start point
            if (GetComponent<HookGrapple>().yeet == true || keepVisible == true)
            {
                shouldUpdate = false;
                keepVisible = true;

                cooldownPercentage = indicatorTimer / 9;

                indicatorTimer -= Time.deltaTime;
                radialIndicatorUI.enabled = true;
                radialIndicatorUI.fillAmount = cooldownPercentage;

                if(indicatorTimer <= 0)
                {
                    indicatorTimer = maxIndicatorTimer;
                    radialIndicatorUI.fillAmount = maxIndicatorTimer;
                    radialIndicatorUI.enabled = false;
                    keepVisible = false;
                    GetComponent<HookGrapple>().yeet = false;
                    myEvent.Invoke();
                }
            }
            else
            {
                if (shouldUpdate)
                {
                    indicatorTimer += Time.deltaTime;
                    radialIndicatorUI.fillAmount = indicatorTimer;

                    if(indicatorTimer >= maxIndicatorTimer)
                    {
                        indicatorTimer = maxIndicatorTimer;
                        radialIndicatorUI.fillAmount = maxIndicatorTimer;
                        radialIndicatorUI.enabled = false;
                        shouldUpdate = false;
                        keepVisible = false;
                        GetComponent<HookGrapple>().yeet = false;
                    }
                }
            }

            if(GetComponent<HookGrapple>().yeet == false)
            {
                shouldUpdate = true;
                radialIndicatorUI.enabled = false;
            }


        }// end of photonview.ismine    
    }// end of update

    [PunRPC]
    private void FixedUpdate()
    {
        if (photonView.IsMine)
        {
            MovePlayer();

            
        }

    }

    private void UpdateNameDisplay()
    {
        // if our player is winning
        var isWinning = FindObjectsOfType<PlayerMovementAdvanced>().OrderBy(p => p.timeSpentTagged).First() == this; // Orders player and grabs top one, if player is top then you're winning

        if (isWinning)
        {
            nameText.text = $"<color=green>{photonView.Owner.NickName}</color>\n<size=50%>{timeSpentTagged:F1}</size>"; // Green color for winning
        }
        else
        {
            nameText.text = $"{photonView.Owner.NickName}\n<size=50%>{timeSpentTagged:F1}</size>";
        }
    }

    [SerializeField] private GameObject PlayerR;

    [PunRPC]
    public void onTagged()
    {
        // flag player as tagged 
        isTagged = true;

        // start tagback timer
        tagBackTimer = tagBackDuration;

        // change color of player
        //GetComponentInChildren<MeshRenderer>().material.color = taggedColor; // got rid of taggedColor & initialColor as material/color issues
        PlayerR.GetComponent<MeshRenderer>().material.color = Color.red;

        // audio cue to all players
        taggedAudio();

        Debug.Log("onTagged() ran");
    }

    [PunRPC]
    public void onUntagged()
    {
        // flag player as untagged
        isTagged = false;
        // reset color of player
        //GetComponentInChildren<MeshRenderer>().material.color = initialColor;
        PlayerR.GetComponent<MeshRenderer>().material.color = Color.white;
        Debug.Log("onUntagged() ran");
    }

    public void taggedAudio()
    {
        source.PlayOneShot(taggedSound);
    }

    /*
    private void namen(Collision other, Collision collision)
    {
        var otherNamen = other.collider.gameObject;
        if (isTagged && collision.gameObject.CompareTag("Player"))
        {
            photonView.RPC("onUntagged", RpcTarget.AllBuffered);

            // tag other
            otherNamen.photonView.RPC("onTagged", RpcTarget.AllBuffered);
        }
    }
   
    */
    /* ### Attempt to collide and activate tagging but caused too many issues so just manual tagging
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log("onCollissionEnter runs"); // ### doesnt run

        // check for player collision
        var otherPlayer = other.collider.GetComponent<PlayerMovementAdvanced>();
        Debug.Log(otherPlayer);

        if (otherPlayer != null)
        {
            Debug.Log("otherPlayer != null");
            // if we're tagged and tagback is on
            if (isTagged && tagBackTimer <= 0f)
            {
                // untag user
                //photonView.RPC("onUntagged", RpcTarget.AllBuffered);

                // tag other
                //otherPlayer.photonView.RPC("onTagged", RpcTarget.AllBuffered);

            }
        }
    }
    */
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        transform.Rotate(0, horizontalInput, 0); //############################## might break it, attempt to update online orientation

        // when to jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && readyToSlide == true)
        {

            //transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            //change opacity here?
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    private void StateHandler()
    {
        overtimeslideSpeed = 22; //hardcoded for slideSpeed = 99

        // Mode - Freeze
        if (freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.velocity = Vector3.zero;
        }

        // Mode - Sliding
        else if (sliding)
        {
            state = MovementState.sliding;

            if (OnSlope() && rb.velocity.y < 0.1f)
                desiredMoveSpeed = slideSpeed;

            else
                desiredMoveSpeed = overtimeslideSpeed;


        }

        // Mode - Crouching
        else if (Input.GetKey(crouchKey))
        {

            state = MovementState.crouching;
            desiredMoveSpeed = crouchSpeed;
        }

        // Mode - Sprinting
        else if(grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            desiredMoveSpeed = sprintSpeed;
        }

        // Mode - Wallrunning
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            desiredMoveSpeed = wallrunSpeed;

        }

        // Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            desiredMoveSpeed = walkSpeed;
        }

        // Mode - Air
        else
        {
            state = MovementState.air;
        }

        // check if desiredMoveSpeed has changed drastically 
        if(Mathf.Abs(desiredMoveSpeed - lastDesiredMoveSpeed) > 5f && moveSpeed != 0)
        {
            StopAllCoroutines();
            StartCoroutine(SmoothlyLerpMoveSpeed());
        }
        else
        {
            moveSpeed = desiredMoveSpeed;
        }

        lastDesiredMoveSpeed = desiredMoveSpeed;
    }

    private IEnumerator SmoothlyLerpMoveSpeed()
    {
        // smothly lerp movementSpeed to desired value
        float time = 0;
        float difference = Mathf.Abs(desiredMoveSpeed - moveSpeed);
        float startValue = moveSpeed;

        while (time < difference)
        {
            moveSpeed = Mathf.Lerp(startValue, desiredMoveSpeed, time / difference);

            if (OnSlope())
            {
                float slopeAngle = Vector3.Angle(Vector3.up, slopeHit.normal);
                float slopeAngleIncrease = 1 + (slopeAngle / 90f);

                time += Time.deltaTime * speedIncreaseMultiplier * slopeIncreaseMultiplier * slopeAngleIncrease;
            }
            else
                time += Time.deltaTime * speedIncreaseMultiplier;

            yield return null;
            
            //time += Time.deltaTime;
            //yield return null;
        }

        moveSpeed = desiredMoveSpeed;
    }

    private void MovePlayer()
    {
        if (activeHook)
        {
            return;
        }

        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on slope
        if (OnSlope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            if (rb.velocity.y > 0)
                rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }

        // on ground
        else if(grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        // turn gravity off while on slope
        //if(!wallrunning) rb.useGravity = !OnSlope(); // MIGHT NEED THIS BUT KINDA BROKEN ##########################
    }
    
    private void SpeedControl()
    {
        if (activeHook)
        {
            return;
        }

        // limiting speed on slope
        if (OnSlope() && !exitingSlope)
        {
            if (rb.velocity.magnitude > moveSpeed)
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }

        // limiting speed on ground or in air
        else
        {
            if(!sliding)
            {
                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                // limit velocity if needed
                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
                }
            }
            else
            {
                //could try to reduce slow down while sliding

                Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

                // limit velocity if needed
                if (flatVel.magnitude > moveSpeed)
                {
                    Vector3 limitedVel = flatVel.normalized * moveSpeed;
                    rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
                }
            }
            
            
        }
    }

    public void Jump()
    {
        exitingSlope = true;

        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    public void ResetJump()
    {
        readyToJump = true;

        exitingSlope = false;
    }

    private bool enableMovementOnNextTouch;

    public void JumpToPosition(Vector3 targetposition, float trajectoryHeight)
    {
        activeHook = true;

        velocityToSet = CalculateJumpVelocity(transform.position, targetposition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);

    }

    private Vector3 velocityToSet;
    private void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.velocity = velocityToSet;
    }

    public void ResetRestrictions()
    {
        activeHook = false;
    }

    private void onCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<HookGrapple>().StopGrapple();
        }
    }

    public bool OnSlope()
    {
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
        {
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0;
        }

        return false;
    }
    
    public Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        return Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized;
    }

    public Vector3 CalculateJumpVelocity(Vector3 startPoint, Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY = endPoint.y - startPoint.y;
        Vector3 displacementXZ = new Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        Vector3 velocityY = Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        //Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) + Mathf.Sqrt(2 * (displacementY - trajectoryHeight) / gravity));
        Vector3 velocityXZ = displacementXZ / 2;

        return velocityXZ + velocityY;
    }

    // photon update time tagged
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isTagged);
            stream.SendNext(timeSpentTagged);
        }
        else
        {
            isTagged = (bool) stream.ReceiveNext();
            timeSpentTagged = (float) stream.ReceiveNext();

            UpdateNameDisplay();
        }
    }
}