
using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerMovement : MonoBehaviour
{
//*variables
    [Header("Player Movement")]
    private float moveSpeed;
    public float walkSpeed = 7f,sprintSpeed = 11f, groundDrag = 7f;
    float horizontalInput, verticalInput;
    Vector3 moveDirection;
    public Camera fpscam;

    [Header("Jumping")]
    [Range(7, 14)]public float jumpForce = 9f;
    
    public float jumpCooldown = 0.25f, airMultiplier = 0.3f, cayoteBase, cayoteTimer, jumpBuffer, jbTimer;
    bool readyToJump = true;

    [Header("Water")]
    public bool inWater;
    public float waterDrag;
    [Header("Ladder")]
    public float ladderSpeed;
    public float ladderDrag;
    bool onLadder;
    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    float startYScale;
    bool crouching;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask groundLayer, platformlayer;
    bool grounded, edgeGrounded;
    [Header("Slope Handling")]
    public float maxSlopeAngle;
    RaycastHit slopeHit;
    bool exitingSlope;

    public Rigidbody rb;
    public CapsuleCollider col;
    

    [Header("KeyBindings")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    public MovementState state;
    public enum MovementState{Sprint, Walking, Crouching, Air}
    int health;
    public bool reachedEnd;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        groundLayer = LayerMask.GetMask("Ground");
        playerHeight = GameObject.Find("PlayerBody").GetComponent<CapsuleCollider>().bounds.size.y;
        fpscam = Camera.main;
        startYScale = transform.localScale.y;

        health = GetComponent<Interactions>().health;

        
        
    }

    // Update is called once per frame
    void Update()
    {
        if(GetComponent<Interactions>().health > 0){
            PlayerInputs();
            StateHandler();
            SpeedController();

            //checks if player is on ground
            for(int i = 0; i < 4; ++i){
                var scale = col.radius * transform.localScale.x;
                Vector3 pos = new Vector3(0,0);
                switch (i)
                {
                    case 0:
                    pos = transform.position + new Vector3(1,0) * scale;
                    break;
                    case 1:
                    pos = transform.position + new Vector3(-1,0) * scale;
                    break;
                    case 2:
                    pos = transform.position + new Vector3(0,0, 1) * scale;
                    break;
                    case 3:
                    pos = transform.position + new Vector3(0,0,-1) * scale;
                    break;
                    
                }
                grounded = Physics.Raycast(pos, Vector3.down, playerHeight * 0.5f + 0.15f);
                if(grounded)break;
            }

            
            

            if(inWater) {
                rb.drag = waterDrag;
                cayoteTimer = cayoteBase;
            }
            else if(onLadder){
                rb.drag = ladderDrag;
                cayoteTimer = cayoteBase;
            } 
            else if(grounded){
                rb.drag = groundDrag;
                cayoteTimer = cayoteBase;
            }
            else if(!grounded) {
                rb.drag = 0;
                cayoteTimer -= Time.deltaTime;
            }

            if(Input.GetKeyDown(KeyCode.RightArrow) && SceneManager.GetActiveScene().buildIndex+1 <6){
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
            } else if(Input.GetKeyDown(KeyCode.LeftArrow) && SceneManager.GetActiveScene().buildIndex-1 >0){
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
            }
            reachedEnd = SceneManager.GetActiveScene().buildIndex == 5;


        }

    }
    void FixedUpdate()
    {
        MovePlayer();
    }
    void PlayerInputs(){

    //*X,Z input 
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        Jumping();
        if(!onLadder)
            Crouching();
        
    }
    void Jumping(){
        jbTimer = Input.GetKeyDown(jumpKey) ?   jbTimer = jumpBuffer : jbTimer - Time.deltaTime;
        //jumps when ready
        if( jbTimer > 0 && cayoteTimer > 0){
            jbTimer = 0;
            exitingSlope = true;
            //prefoms jump
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            if(onLadder) {
                rb.AddForce(-transform.forward  * 2, ForceMode.Impulse);
                rb.AddForce(transform.up * (jumpForce/2), ForceMode.Impulse);
            }else
                rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            
        }
        //reduces jump if space is not held
        if(Input.GetKeyUp(jumpKey) && rb.velocity.y > 0){
            rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y* 0.6f, rb.velocity.z);
            cayoteTimer = 0;
            exitingSlope = false;
        }
            
    }

    void Crouching(){
        if(Input.GetKeyDown(crouchKey)){
            crouching = true;
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }
        if(Input.GetKeyUp(crouchKey))
            crouching = false;
        //uncrouching if there isnt a celing
        if(!crouching && !Physics.Raycast(transform.position, Vector3.up, playerHeight))
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
    }

//*X,Z movement
    void MovePlayer(){
        if(onLadder){
            if(Vector3.Angle(transform.forward, fpscam.transform.forward) > 45){
                moveDirection = fpscam.transform.forward * verticalInput + transform.right * horizontalInput;
                rb.AddForce(moveDirection.normalized * ladderSpeed * 10f, ForceMode.Force);
                
            }
            rb.useGravity = false;
            return;
        }
            //Calculate Movement
        moveDirection = transform.forward * verticalInput + transform.right * horizontalInput;
        
    //moves player
        if(OnSlope() && !exitingSlope){
            rb.AddForce(SlopeMoveDirection() * moveSpeed * 20f, ForceMode.Force);
            if(rb.velocity.y > 0) rb.AddForce(Vector3.down * 80f, ForceMode.Force);
        }
        else if(grounded){
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);

        rb.useGravity = !OnSlope();
        //makes sure the player doesnt get stuck to walls
        if(Physics.Raycast(transform.position, Vector3.right,0.7f)) 
            rb.AddForce(-Vector3.right, ForceMode.Impulse);
        if(Physics.Raycast(transform.position, -Vector3.right,0.7f)) 
            rb.AddForce(Vector3.right, ForceMode.Impulse);
    
    }
    void StateHandler(){
        if(Input.GetKey(crouchKey)){
            state = MovementState.Crouching;
            moveSpeed = crouchSpeed;
        } else if(grounded && Input.GetKey(sprintKey)){
            state = MovementState.Sprint;
            moveSpeed = sprintSpeed;
        } else if(grounded){
            state = MovementState.Walking;
            moveSpeed = walkSpeed;
        } else state = MovementState.Air;
    }
    void SpeedController(){
        //limits speed when it gets to high
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        if(OnSlope() && !exitingSlope){
            if(rb.velocity.magnitude > moveSpeed) 
                rb.velocity = rb.velocity.normalized * moveSpeed;
        }else{
            if(flatVel.magnitude > moveSpeed){
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
            }
        }
        
    }
    bool OnSlope(){
        //checks if on slope
        if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f)){
            float angle = Vector3.Angle(Vector3.up, slopeHit.normal);
            //checks if the angle is walkable
            return angle < maxSlopeAngle && angle != 0;
        }
        return false;
    }
    Vector3 SlopeMoveDirection() {return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;}


    void OnCollisionStay(Collision collision)
    {
        foreach (ContactPoint p in collision.contacts)
        {
            if(collision.gameObject.tag == "Ladder"){
                if(Mathf.Abs(p.normal.z) == 1 || Mathf.Abs(p.normal.x) == 1){
                    onLadder = true;
                }
                
            }
                
            
        }
        
        
    }
    void OnCollisionExit(Collision collision)
    {
        if(collision.gameObject.tag == "Ladder") onLadder = false;
    }
    void OnDrawGizmosSelected()
    {
        
    }
}
