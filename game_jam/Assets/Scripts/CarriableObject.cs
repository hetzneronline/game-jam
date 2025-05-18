using UnityEngine;

/// <summary>
/// Represents a small object that can be picked up, carried, and dropped into designated receivers (like air vents).
/// Inherits from InteractiveObject to leverage the existing interaction system.
/// </summary>
public class CarriableObject : InteractiveObject
{
    [Header("Carriable Object Settings")]
    [SerializeField] private float pickupDistance = 1.5f;
    [SerializeField] private float carryHeight = 0.5f;
    [SerializeField] private float carryDistance = 1.0f;
    [SerializeField] private bool usePhysics = true;
    [SerializeField] private string objectType = "Apple"; // Type identifier used by receivers
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private AudioClip dropSound;
    
    // References
    private Rigidbody2D rb;
    private Collider2D objectCollider;
    private Transform playerTransform;
    private bool isBeingCarried = false;
    private Vector3 originalScale;
    private AudioSource audioSource;
    private Vector2 lastDirection = Vector2.down; // Default direction
    
    // Events
    public delegate void ObjectActionHandler(CarriableObject obj);
    public static event ObjectActionHandler OnObjectPickedUp;
    public static event ObjectActionHandler OnObjectDropped;
      // Property to access object type from receivers
    public string ObjectType => objectType;
    
    // Property to check if object is being carried
    public bool IsBeingCarried => isBeingCarried;
    
    protected override void Start()
    {
        base.Start(); // Call base Start method to set up renderer and collider
        
        // Get references
        rb = GetComponent<Rigidbody2D>();
        if (rb == null && usePhysics)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        
        objectCollider = GetComponent<Collider2D>();
        if (objectCollider == null)
        {
            // Add a default circle collider if none exists
            objectCollider = gameObject.AddComponent<CircleCollider2D>();
            ((CircleCollider2D)objectCollider).radius = 0.5f;
        }
        
        // Find player
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        // Store original properties
        originalScale = transform.localScale;
        
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (pickupSound != null || dropSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f; // Make sound 3D
            audioSource.maxDistance = 20f;
            audioSource.rolloffMode = AudioRolloffMode.Linear;
        }
        
        // Set interaction prompt
        interactionPrompt = "Press E to pick up";
    }
    
    protected override void Update()
    {
        if (!isBeingCarried)
        {
            base.Update(); // Use the base class behavior when not carrying
        }
        else
        {
            // When being carried, update position
            UpdateCarriedPosition();
            
            // Check for drop input
            if (Input.GetKeyDown(KeyCode.E))
            {
                DropObject();
            }
            
            // Track player direction for proper positioning
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float verticalInput = Input.GetAxisRaw("Vertical");
            
            if (horizontalInput != 0 || verticalInput != 0)
            {
                lastDirection = new Vector2(horizontalInput, verticalInput).normalized;
            }
        }
    }
    
    protected override void Interact()
    {
        if (!isBeingCarried && playerTransform != null)
        {
            PickupObject();
        }
    }
    
    private void PickupObject()
    {
        if (Vector3.Distance(transform.position, playerTransform.position) <= pickupDistance)
        {
            isBeingCarried = true;
            
            // Disable physics while being carried
            if (rb != null)
            {
                rb.simulated = false;
            }
            
            // Make collider a trigger to avoid physics interactions while carrying
            if (objectCollider != null)
            {
                objectCollider.isTrigger = true;
            }
            
            // Play pickup sound
            if (audioSource != null && pickupSound != null)
            {
                audioSource.PlayOneShot(pickupSound);
            }
            
            // Invoke event
            OnObjectPickedUp?.Invoke(this);
            
            Debug.Log($"Picked up {objectType}");
        }
    }
    
    private void UpdateCarriedPosition()
    {
        if (playerTransform != null)
        {
            // Position the object in front of the player based on last movement direction
            Vector3 carryPosition = playerTransform.position + new Vector3(
                lastDirection.x * carryDistance,
                lastDirection.y * carryDistance + carryHeight,
                0);
            
            // Smoothly move to the carry position
            transform.position = Vector3.Lerp(transform.position, carryPosition, Time.deltaTime * 10f);
        }
    }
    
    private void DropObject()
    {
        isBeingCarried = false;
        
        // Re-enable physics
        if (rb != null)
        {
            rb.simulated = true;
            rb.linearVelocity = Vector2.zero; // Reset velocity to prevent flying objects
        }
        
        // Re-enable collisions
        if (objectCollider != null)
        {
            objectCollider.isTrigger = false;
        }
        
        // Play drop sound
        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }
        
        // Invoke event
        OnObjectDropped?.Invoke(this);
        
        Debug.Log($"Dropped {objectType}");
        
        // Check if the object is over a receiver when dropped
        CheckForReceivers();
    }
    
    // Check if the object is dropped into a valid receiver (like an air vent)
    private void CheckForReceivers()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, 0.5f);
        foreach (Collider2D collider in colliders)
        {
            ObjectReceiver receiver = collider.GetComponent<ObjectReceiver>();
            if (receiver != null)
            {
                receiver.ReceiveObject(this);
                break; // Only allow one receiver to process the object
            }
        }
    }
    
    // Called when the player moves too far away - force drop the object
    private void DropIfTooFar()
    {
        if (isBeingCarried && playerTransform != null)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > pickupDistance * 2f)
            {
                DropObject();
            }
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isBeingCarried)
        {
            ObjectReceiver receiver = collision.GetComponent<ObjectReceiver>();
            if (receiver != null)
            {
                // Highlight the receiver when hovering over it with a carried object
                receiver.ShowHoverState(true, objectType);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (isBeingCarried)
        {
            ObjectReceiver receiver = collision.GetComponent<ObjectReceiver>();
            if (receiver != null)
            {
                // Remove highlight when moving away from a receiver
                receiver.ShowHoverState(false, objectType);
            }
        }
    }
    
    // Allow external scripts to force-drop the object (e.g., when player dies)
    public void ForceDropObject()
    {
        if (isBeingCarried)
        {
            DropObject();
        }
    }
}
