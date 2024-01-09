using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    [SerializeField] private BoxCollider2D coll;
    private SpriteRenderer sprite;
    private Animator anim;

    [SerializeField] private LayerMask jumpableGround;

    [Header("Horizontal Movement")]
    [SerializeField] private float dirX = 0f;
    [SerializeField] private float moveSpeed = 7f;

    [SerializeField] private float teleportDistance = 5f;
    [SerializeField] private bool canTeleportLeft = false;
    [SerializeField] private bool canTeleportRight = false;
    private bool canTeleport = true;

    private float boostTimer;
    [SerializeField] private float boostTime = 0.4f;

    [Header("Vertical Movement")]   
    [SerializeField] private float jumpForce = 14f;    
    [SerializeField] private float jumpDelay = 0.2f;
    private float jumpTimer;

    [SerializeField] private bool canDoubleJump = true;
    private bool jumpButtonHeld = false;
    [SerializeField] private float couterJumpForce = 40f;
    [SerializeField] private float maxFallVelocity = -20f;

    [SerializeField] private float coyoteTime = 0.1f;
    private float coyoteTimer;

    [SerializeField] private float minWallFallVelocity = -5f;  
    [SerializeField] private bool isWallJumping = false;
    private float wallJumpingDirection;
    [SerializeField] private float wallJumpingTime = 0.2f;
    private float wallJumpingCounter;
    [SerializeField] private float wallJumpingDuration = 0.3f;
    [SerializeField] private Vector2 wallJumpingPower = new Vector2(8f, 14f);

    [Header("Collision")]
    [SerializeField] private bool onGround = false;
    [SerializeField] private bool onLeftWall = false;
    [SerializeField] private bool onRightWall = false;
    [SerializeField] private float groundCheckOffset = .1f;
    [SerializeField] private float wallCheckOffset = .2f;
    [SerializeField] private bool isWallSliding = false;
    public bool isInvincible = false;

    [Header("Audio")]
    [SerializeField] private AudioSource jumpSoundEffect;
    [SerializeField] private AudioSource doubleJumpSoundEffect;
    [SerializeField] private AudioSource teleportSoundEffect;

    private enum MovementState { idle, running, jumping, falling, double_jump, on_wall }    
    
    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<BoxCollider2D>();
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        onGround = IsGrounded(); // check if grounded
        onLeftWall = IsOnLeftWall(); // check if on left walls
        onRightWall = IsOnRightWall(); // check if on right walls
        canTeleportLeft = CanTeleportLeft(); // check if can teleport to the left
        canTeleportRight = CanTeleportRight(); // check if can teleport to the right
        dirX = Input.GetAxis("Horizontal"); // get horizontal movement input        

        if (onGround)
        {
            canDoubleJump = true;      // restore double jump when on ground
            isWallSliding = false;     // stop wall sliding
            coyoteTimer = Time.time + coyoteTime;  // set timer for coyote time
        }

        if ((onLeftWall && dirX < 0f && !onGround) || (onRightWall && dirX > 0f && !onGround))  // Wall slide check
        {
            isWallSliding = true;            
        } 
        else
        {
            isWallSliding = false;
        }

        if (onGround || onLeftWall || onRightWall)  // restore telport when on ground or on wall
        {
            canTeleport = true;     
            sprite.color = new Color(1, 1, 1, 1); 
        }

        if (!canTeleport)
        {
            sprite.color = new Color(1, 1, 1, 0.5f); // sprite opacity change to indicate no teleport left.
        }

        if (Input.GetButtonDown("Jump")) // register jump potentially before landing
        {
            jumpTimer = Time.time + jumpDelay; // timer for jump buffer
            jumpButtonHeld = true; 
        }   
        
        if (Input.GetButtonUp("Jump"))
        {
            jumpButtonHeld = false;
        }         
              
        if (Input.GetButtonDown("Teleport") && canTeleport) // register teleport input
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, 0f, float.MaxValue));
            isInvincible = true; // invincibility for the first half of the teleport animation
            teleportSoundEffect.Play();            
            anim.SetTrigger("teleport");
            canTeleport = false;            
        }

        if (boostTimer > Time.time)
        {
            jumpForce = 16f;
            moveSpeed = 10;
        }
        else
        {
            jumpForce = 14f;
            moveSpeed = 7f;
        }

        AnimationUpdate(); 
    }

    private void FixedUpdate()
    {
        if (!isWallJumping) // cannot move while wall jumping
        {
            rb.velocity = new Vector2(dirX * moveSpeed, Mathf.Clamp(rb.velocity.y, maxFallVelocity, float.MaxValue)); // make character move (snappy), impose max fall speed
        }        

        if (jumpTimer > Time.time && coyoteTimer > Time.time && !isWallSliding) // Jump with buffer and coyote time
        {
            jumpSoundEffect.Play();
            Jump();
        }

        if (!jumpButtonHeld && rb.velocity.y > 0)  // Make jump height depends on button press
        {
            rb.AddForce(Vector2.down * couterJumpForce);
        }

        if (jumpTimer > Time.time && isWallSliding && !onGround) // Wall jump, sound effect depends on double jump state
        {
            if (canDoubleJump)
            {
                jumpSoundEffect.Play();
            } 
            else 
            {
                doubleJumpSoundEffect.Play();
            }            
            WallJump();
        }        

        if (jumpTimer > Time.time && canDoubleJump && !isWallSliding && !isWallJumping) 
            // Double jump, prevent its use if wall sliding or wall jumping
        {
            doubleJumpSoundEffect.Play();
            Jump();
            canDoubleJump = false;
        }

        if (isWallSliding)  // Slow down wall side
        {            
            rb.velocity = new Vector2(rb.velocity.x , Mathf.Clamp(rb.velocity.y, minWallFallVelocity, float.MaxValue));
        }        
    }

    private void Jump()
    {        
        rb.velocity = new Vector2(rb.velocity.x, 0);                 //keep horizontal velocity
        rb.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse); //add impulse for snappy jump
        jumpTimer = 0;                                               //reset jump buffer timer
    }

   /* private void WallJump() // DOES NOT WoRK PROPERLY BUT REUSE FOR SLIDING RAMP
    {
        rb.velocity = new Vector2(rb.velocity.x, 0); //keep horizontal velocity
        rb.AddForce(new Vector2(0, verticalWallJumpForce), ForceMode2D.Impulse); //add impulse depending on the direction
        rb.AddForce(new Vector2(Mathf.Sign(-dirX) * horizontalWallJumpForce, 0));
        jumpTimer = 0; //reset jump buffer timer        
    }*/

    private void WallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            if (sprite.flipX)  // set wall jumping direction depending on the sprite orientation
            {
                wallJumpingDirection = 1f;
            }
            else
            {
                wallJumpingDirection = -1f;                    
            }
            wallJumpingCounter = wallJumpingTime;
            CancelInvoke(nameof(StopWallJumping)); // stop wall jumping state if touch another wall before the end of wall jumping time
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime; // allow a time window to wall jump after moving away from wall
        }

        if (wallJumpingCounter > 0)
        {            
            isWallJumping = true;
            rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y); // make wall jump
            wallJumpingCounter = 0f; // reset wall jumping counter
        }

        Invoke(nameof(StopWallJumping), wallJumpingDuration);
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }


    private void Teleport() // Teleport left or right depending on sprite orientation
    {
        if (sprite.flipX)
        {
            if (canTeleportLeft)
            {
                transform.position = transform.position - new Vector3(teleportDistance, 0, 0);
            } 
            else
            {
                transform.position = transform.position - new Vector3(PartialTeleportLeft(), 0, 0); // if cant teleport, move player near first collider on the way
            }
            
        } 
        else if (!sprite.flipX) 
        {
            if (canTeleportRight)
            {
                transform.position = transform.position + new Vector3(teleportDistance, 0, 0);
            }
            else
            {
                transform.position = transform.position + new Vector3(PartialTeleportRight(), 0, 0);
            }
        }

        rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, 0f, float.MaxValue)); // slow down fall after telport       
        isInvincible = false; // stop invincibility
        boostTimer = Time.time + boostTime;
    }

    private void AnimationUpdate()
    {
        MovementState state;

        if (dirX > 0f || rb.velocity.x > 0.1f)
        {
            sprite.flipX = false;
        } 
        else if (dirX < 0f || rb.velocity.x < -0.1f) 
        {
            sprite.flipX = true;
        }     
                
        if (onGround && dirX > 0f)
        {
            state = MovementState.running;
            sprite.flipX = false;
        }
        else if (onGround && dirX < 0f)
        {
            state = MovementState.running;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > .1f)
        {
            if (canDoubleJump)
            {
                state = MovementState.jumping;
            }
            else
            {
                state = MovementState.double_jump;
            }
        }
        else if (rb.velocity.y < -.1f)
        {
            state = MovementState.falling;
        }

        if (!onGround && onRightWall && dirX > 0f)
        {
            state = MovementState.on_wall;                
            sprite.flipX = false;
            
        } else if (!onGround && onLeftWall && dirX < 0f)            
        {                
            state = MovementState.on_wall;                
            sprite.flipX = true;
            
        }     
                
        anim.SetInteger("state", (int)state);
    }

    private bool IsGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size * .9f, 0f, Vector2.down, groundCheckOffset, jumpableGround);
    }
    
    private bool IsOnLeftWall()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, wallCheckOffset, jumpableGround);
    }

    private bool IsOnRightWall()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, wallCheckOffset, jumpableGround);

    }  
    
    private bool CanTeleportLeft()
    {
        return !Physics2D.OverlapBox(coll.bounds.center + Vector3.left * teleportDistance, coll.bounds.size + new Vector3(.2f, -.2f, 0), 0f, jumpableGround);
    }

    private bool CanTeleportRight()
    {
        return !Physics2D.OverlapBox(coll.bounds.center + Vector3.right * teleportDistance, coll.bounds.size + new Vector3(.2f, -.2f, 0), 0f, jumpableGround);
    }

    private float PartialTeleportLeft()
    {        
        RaycastHit2D m_Hit;
        m_Hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.left, teleportDistance, jumpableGround);
        return m_Hit.distance;
    }

    private float PartialTeleportRight()
    {
        RaycastHit2D m_Hit;
        m_Hit = Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.right, teleportDistance, jumpableGround);
        return m_Hit.distance;
    }

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(coll.bounds.center + Vector3.left * teleportDistance, coll.bounds.size + new Vector3(.2f, -.2f, 0));
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(coll.bounds.center + Vector3.right * teleportDistance, coll.bounds.size + new Vector3(.2f, -.2f, 0));
    }*/

    /*private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(coll.bounds.center + Vector3.left * wallCheckOffset, coll.bounds.size);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(coll.bounds.center + Vector3.right * wallCheckOffset, coll.bounds.size);
        Gizmos.color = Color.green;
        Gizmos.DrawCube(coll.bounds.center + Vector3.down * groundCheckOffset, coll.bounds.size * .9f);
    }*/
}
