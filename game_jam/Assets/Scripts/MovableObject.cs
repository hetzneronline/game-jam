using UnityEngine;

public class MovableObject : InteractiveObject
{
    [Header("Movable Object Settings")]
    [SerializeField] private float pickupDistance = 1.5f;
    [SerializeField] private float carryHeight = 0f; // Reduced to 0 for top-down games
    [SerializeField] private float carryDistance = 1.0f;
    [SerializeField] private float dropCooldown = 0.5f;
    [SerializeField] private LayerMask obstacleLayers; // Layers that block placement
    [SerializeField] private bool maintainOrientation = true; // Whether to keep the object's original orientation

    private bool isBeingCarried = false;
    private Rigidbody2D objectRigidbody;
    private Collider2D objectCollider;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private Quaternion originalRotation; // Store the original rotation
    private Vector2 pickupDirection; // Direction relative to player when picked up
    private bool canDropObject = true;
    private float lastDropTime;

    // Override the parent Start method to initialize additional components
    protected override void Start()
    {
        base.Start(); // Call the parent Start method first

        // Get or add a Rigidbody2D
        objectRigidbody = GetComponent<Rigidbody2D>();
        if (objectRigidbody == null)
        {
            objectRigidbody = gameObject.AddComponent<Rigidbody2D>();
            objectRigidbody.gravityScale = 0f;
            objectRigidbody.freezeRotation = true;
            objectRigidbody.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        // Store reference to collider
        objectCollider = GetComponent<Collider2D>();
        
        // Store original position, scale, and rotation
        originalPosition = transform.position;
        originalScale = transform.localScale;
        originalRotation = transform.rotation;
        
        // Set interaction message
        interactionPrompt = "Press E to pick up";
        
        // Set proper physics settings
        objectRigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
        objectRigidbody.linearDamping = 10f; // High drag to prevent sliding
        objectRigidbody.angularDamping = 10f;
    }

    protected override void Update()
    {
        // Skip the parent Update if we're carrying the object
        if (!isBeingCarried)
        {
            base.Update();
        }
        else
        {
            // Implement carrying behavior
            UpdateCarryPosition();
            
            // Check for drop input
            if (Input.GetKeyDown(KeyCode.E) && canDropObject && Time.time - lastDropTime > dropCooldown)
            {
                DropObject();
            }
        }
    }

    // Override the Interact method to implement pickup behavior
    protected override void Interact()
    {
        if (!isBeingCarried)
        {
            PickupObject();
        }
    }

    private void PickupObject()
    {
        // Only pickup if the player is close enough
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= pickupDistance)
        {
            isBeingCarried = true;
            
            // Calculate pickup direction for later use
            Vector2 toObject = transform.position - playerTransform.position;
            pickupDirection = toObject.normalized;
            
            // Update physics
            objectRigidbody.isKinematic = true;
            objectRigidbody.linearVelocity = Vector2.zero; // Reset any velocity
            
            // Ensure scale is preserved
            transform.localScale = originalScale;
            
            if (objectCollider != null)
            {
                objectCollider.isTrigger = true;
            }
            
            // Hide prompt
            if (UIPromptController.Instance != null)
            {
                UIPromptController.Instance.HidePrompt();
                UIPromptController.Instance.ShowPrompt("Press E to place");
            }
            
            Debug.Log($"Player picked up {gameObject.name}");
        }
    }

    private void DropObject()
    {
        Vector2 dropPosition;
        
        if (maintainOrientation)
        {
            // Use the original pickup direction to determine drop position
            dropPosition = (Vector2)playerTransform.position + pickupDirection * carryDistance;
        }
        else
        {
            // Use current facing direction to determine drop position
            dropPosition = CalculateDropPosition();
        }
        
        // Raycast to check if there's an obstacle at the drop position
        Collider2D obstacle = Physics2D.OverlapCircle(dropPosition, 0.5f, obstacleLayers);
        
        if (obstacle == null)
        {
            // Place the object
            transform.position = new Vector3(dropPosition.x, dropPosition.y, transform.position.z);
            isBeingCarried = false;
            
            // Update physics
            objectRigidbody.isKinematic = false;
            objectRigidbody.linearVelocity = Vector2.zero; // Important: Reset velocity to prevent sliding
            
            // Restore original rotation if maintaining orientation
            if (maintainOrientation)
            {
                transform.rotation = originalRotation;
            }
            
            // Ensure scale is preserved
            transform.localScale = originalScale;
            
            if (objectCollider != null)
            {
                objectCollider.isTrigger = false;
            }
            
            // Update prompt
            if (UIPromptController.Instance != null)
            {
                UIPromptController.Instance.HidePrompt();
            }
            
            Debug.Log($"Player placed {gameObject.name}");
        }
        else
        {
            // Show message that there's no room
            if (UIPromptController.Instance != null)
            {
                UIPromptController.Instance.ShowPrompt("Can't place here - blocked by obstacle");
            }
            Debug.Log($"Can't place {gameObject.name} - blocked by {obstacle.name}");
        }
        
        lastDropTime = Time.time;
    }

    private void UpdateCarryPosition()
    {
        if (playerTransform != null)
        {
            Vector2 targetPosition;
            
            // Use the original direction when picked up
            if (maintainOrientation)
            {
                targetPosition = (Vector2)playerTransform.position + pickupDirection * carryDistance;
            }
            // Otherwise use player's facing direction
            else
            {
                // Calculate direction player is facing
                Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
                Vector2 carryDirection = playerInput.magnitude > 0.1f ? playerInput : Vector2.down;
                
                // Set position in front of player
                targetPosition = (Vector2)playerTransform.position + carryDirection * carryDistance;
            }
            
            // Add height if needed (reduced/removed for top-down games)
            if (carryHeight > 0)
            {
                targetPosition += Vector2.up * carryHeight;
            }
            
            // Keep the original z position to avoid depth issues
            float zPos = transform.position.z;
            
            // Smoothly move object
            transform.position = Vector3.Lerp(
                transform.position, 
                new Vector3(targetPosition.x, targetPosition.y, zPos), 
                Time.deltaTime * 10f
            );
            
            // Ensure scale is preserved
            transform.localScale = originalScale;
        }
    }

    private Vector2 CalculateDropPosition()
    {
        if (playerTransform != null)
        {
            // Calculate direction player is facing
            Vector2 playerInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            Vector2 dropDirection = playerInput.magnitude > 0.1f ? playerInput : Vector2.down;
            
            // Calculate position in front of player
            return (Vector2)playerTransform.position + dropDirection * carryDistance;
        }
        
        return transform.position;
    }
    
    // Reset the object to its original position (useful for debugging)
    public void ResetPosition()
    {
        transform.position = originalPosition;
        transform.localScale = originalScale;
        transform.rotation = originalRotation;
        isBeingCarried = false;
        objectRigidbody.isKinematic = false;
        objectRigidbody.linearVelocity = Vector2.zero;
        
        if (objectCollider != null)
        {
            objectCollider.isTrigger = false;
        }
    }
}