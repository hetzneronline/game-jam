using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Needed for LINQ methods like Any

// Top-down 2D character controller using Unity's built-in Input system
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactionRadius = 2f;
    
    [Header("Collider Settings")]
    [SerializeField] private float feetColliderHeight = 0.2f; // Height of feet collider
    [SerializeField] private float feetColliderWidth = 0.5f;  // Width of feet collider
    [SerializeField] private float feetColliderYOffset = -0.4f; // Y-offset from center
    
    private Rigidbody2D rb;
    private BoxCollider2D feetCollider;
    private Animator animator; // Reference to the Animator component
    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    
    // Animation parameter names
    private readonly string isWalkingParam = "IsWalking";
    private readonly string horizontalParam = "Horizontal";
    private readonly string verticalParam = "Vertical";
    
    private Vector2 lastMovementDirection = Vector2.down; // Default facing down
    private float keyReleaseCheckDelay = 0.05f; // How long to wait after key release before stopping animation
    private float lastKeyPressTime = 0;
    
    private void Start()
    {
        // Get required components
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {            
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        
        feetCollider = GetComponent<BoxCollider2D>();
        if (feetCollider == null)
        {
            feetCollider = gameObject.AddComponent<BoxCollider2D>();
        }
        
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Adjust collider
        AdjustFeetCollider();
        
        // Ensure idle state at start
        StopWalkingAnimation();
    }
    
    private void AdjustFeetCollider()
    {
        if (feetCollider != null)
        {
            feetCollider.size = new Vector2(feetColliderWidth, feetColliderHeight);
            feetCollider.offset = new Vector2(0, feetColliderYOffset);
            Debug.Log($"Adjusted player collider to feet-only: Size={feetCollider.size}, Offset={feetCollider.offset}");
        }
    }
    
    private void Update()
    {
        // Check for input field focus - ADD THIS AT THE TOP OF UPDATE
        bool isAnyInputFieldFocused = FindObjectsByType<TMPro.TMP_InputField>(FindObjectsSortMode.None)
            .Any(field => field.isFocused);
        
        if (isAnyInputFieldFocused)
        {
            // Don't process movement when input field is focused
            StopWalkingAnimation();
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Direct key checking
        bool upKeyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool downKeyPressed = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool leftKeyPressed = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightKeyPressed = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        
        bool anyMovementKeyPressed = upKeyPressed || downKeyPressed || leftKeyPressed || rightKeyPressed;
        
        // IMPORTANT: Reset animation right away when keys are released
        if (!anyMovementKeyPressed)
        {
            // Force animation to stop IMMEDIATELY
            StopWalkingAnimation();
            rb.linearVelocity = Vector2.zero;
            return; // Skip the rest of the method
        }
        
        if (anyMovementKeyPressed)
        {
            lastKeyPressTime = Time.time;
            
            // Calculate movement direction
            Vector2 moveDir = Vector2.zero;
            
            if (upKeyPressed) moveDir.y += 1;
            if (downKeyPressed) moveDir.y -= 1;
            if (leftKeyPressed) moveDir.x -= 1;
            if (rightKeyPressed) moveDir.x += 1;
            
            // Normalize to prevent diagonal speed boost
            if (moveDir.magnitude > 0)
            {
                moveDir.Normalize();
                lastMovementDirection = moveDir;
            }
            
            UpdateFacingDirection(moveDir);
            
            // Start walking animation with CORRECTED animation parameters
            if (animator != null)
            {
                animator.SetBool(isWalkingParam, true);
                
                // Set parameters for animator transitions
                animator.SetFloat(horizontalParam, moveDir.x);
                animator.SetFloat(verticalParam, moveDir.y);
                
                // For horizontal movement, explicitly select the animation state
                if (moveDir.x < -0.1f) // Walking left
                {
                    animator.Play("WalkLeft");
                    spriteRenderer.flipX = false; // No flip for left animation
                }
                else if (moveDir.x > 0.1f) // Walking right
                {
                    animator.Play("WalkRight");
                    spriteRenderer.flipX = false; // No flip for right animation
                }
                else if (Mathf.Abs(moveDir.y) > 0.1f) // Vertical movement
                {
                    animator.Play("Idle");
                    
                    // No need to flip the sprite for vertical movement
                    // Keep the last horizontal direction for the sprite orientation
                    if (lastMovementDirection.x < -0.1f)
                    {
                        spriteRenderer.flipX = false; // Face left
                    }
                    else if (lastMovementDirection.x > 0.1f)
                    {
                        spriteRenderer.flipX = true; // Face right
                    }
                    // If no previous horizontal direction, default to facing right
                    else
                    {
                        spriteRenderer.flipX = false; // Default facing
                    }
                    
                    // IMPORTANT: Reset any scale modifications
                    transform.localScale = new Vector3(1, 1, 1); // Reset to normal scale
                }
                else
                {
                    // No movement or diagonal, use idle
                    animator.Play("Idle");
                }
            }
        }
        
        // Handle interaction
        if (Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Player pressed E to interact");
        }
    }
    
    // Make StopWalkingAnimation more aggressive
    private void StopWalkingAnimation()
    {
        if (animator != null)
        {
            // Force reset animation state
            animator.Rebind(); // This forces a full reset of the animator
            animator.SetBool(isWalkingParam, false);
            
            // Keep the last direction for idle facing
            animator.SetFloat(horizontalParam, lastMovementDirection.x);
            animator.SetFloat(verticalParam, lastMovementDirection.y);
            
            // IMPORTANT: Reset any scale modifications
            transform.localScale = new Vector3(1, 1, 1); // Reset to normal scale
        }
    }
    
    private void UpdateFacingDirection(Vector2 moveDir)
    {
        if (spriteRenderer != null)
        {
            // Only update flip if we have horizontal movement
            if (Mathf.Abs(moveDir.x) > 0.1f)
            {
                // CORRECTED: Fix the mirroring issue
                // If moving left (negative X), don't flip
                // If moving right (positive X), flip
                spriteRenderer.flipX = moveDir.x > 0;
            }
        }
    }
    
    // Enhanced FixedUpdate to handle input field focus
    private bool IsInputFieldFocused()
    {
        // More efficient to cache this result between Update and FixedUpdate
        return FindObjectsByType<TMPro.TMP_InputField>(FindObjectsSortMode.None)
            .Any(field => field.isFocused);
    }

    private void FixedUpdate()
    {
        // Check for input field focus
        if (IsInputFieldFocused())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        // Direct key checking for physics movement
        bool upKeyPressed = Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);
        bool downKeyPressed = Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);
        bool leftKeyPressed = Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);
        bool rightKeyPressed = Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);
        
        // IMMEDIATE animation reset if no keys pressed
        if (!upKeyPressed && !downKeyPressed && !leftKeyPressed && !rightKeyPressed)
        {
            StopWalkingAnimation();
            rb.linearVelocity = Vector2.zero;
            return;
        }
        
        Vector2 moveDir = Vector2.zero;
        
        if (upKeyPressed) moveDir.y += 1;
        if (downKeyPressed) moveDir.y -= 1;
        if (leftKeyPressed) moveDir.x -= 1;
        if (rightKeyPressed) moveDir.x += 1;
        
        // Apply movement
        if (moveDir.magnitude > 0)
        {
            moveDir.Normalize();
            rb.linearVelocity = moveDir * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
    }
    
    // You can use this to visualize the collider in the editor
    private void OnDrawGizmosSelected()
    {
        // Draw a wire cube representing the feet collider
        Gizmos.color = Color.green;
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            Matrix4x4 oldMatrix = Gizmos.matrix;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(col.offset, col.size);
            Gizmos.matrix = oldMatrix;
        }
        else
        {
            // If no collider exists yet, use the configured values
            Vector2 size = new Vector2(feetColliderWidth, feetColliderHeight);
            Vector3 center = new Vector3(0, feetColliderYOffset, 0);
            Gizmos.DrawWireCube(transform.position + center, size);
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Handle collisions if needed
    }
}
