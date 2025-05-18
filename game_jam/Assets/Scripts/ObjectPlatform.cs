using UnityEngine;

/// <summary>
/// Represents a platform or surface where CarriableObjects can be placed.
/// Provides visual feedback when objects are hovering over the platform.
/// </summary>
public class ObjectPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    [SerializeField] private string platformName = "Object Platform";
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private Vector3 objectPlacementOffset = new Vector3(0f, 0.5f, 0f);
    [SerializeField] private Color highlightColor = new Color(0.5f, 0.8f, 1f, 1f);
    [SerializeField] private string[] acceptedObjectTypes; // Empty array means accept all types
    
    [Header("Audio")]
    [SerializeField] private AudioClip placementSound;
    [SerializeField] private AudioClip removingSound;
    
    // References
    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private Renderer objectRenderer;
    private Color originalColor;
    private bool isHighlighted = false;
    
    // Currently placed object
    private CarriableObject placedObject = null;
    
    // Events
    public delegate void PlatformActionHandler(CarriableObject obj, ObjectPlatform platform);
    public static event PlatformActionHandler OnObjectPlaced;
    public static event PlatformActionHandler OnObjectRemoved;
    
    private void Awake()
    {
        // Get references
        spriteRenderer = GetComponent<SpriteRenderer>();
        objectRenderer = GetComponent<Renderer>();
        
        // Store original color
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        else if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (placementSound != null || removingSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Make sure we have a collider set to trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
        }
        else
        {
            collider.isTrigger = true;
        }
    }
    
    private void Update()
    {        // First check if player is close and wants to pick up the placed object
        if (placedObject != null)
        {
            float distanceToPlayer = 0f;
            if (CheckPlayerInRange(out distanceToPlayer))
            {
                // Show prompt to pick up the object
                if (UIPromptController.Instance != null)
                {
                    UIPromptController.Instance.ShowPrompt("Press E to pick up " + placedObject.ObjectType);
                }
                
                // If player presses E to pick up the object
                if (Input.GetKeyDown(KeyCode.E))
                {
                    // Allow the object to be picked up
                    EnableObjectPhysics(placedObject);
                    
                    // Clear the prompt
                    if (UIPromptController.Instance != null)
                    {
                        UIPromptController.Instance.HidePrompt();
                    }
                    
                    placedObject = null;
                    return;
                }
            }
            else
            {
                // Hide prompt when player walks away
                if (UIPromptController.Instance != null)
                {
                    UIPromptController.Instance.HidePrompt();
                }
            }
            return;
        }
            
        // Find all CarriableObjects in the scene
        CarriableObject[] carriableObjects = FindObjectsOfType<CarriableObject>();
        
        bool foundObjectInRange = false;
        CarriableObject closestObject = null;
        float closestDistance = float.MaxValue;
        
        foreach (CarriableObject obj in carriableObjects)
        {
            // Skip if this object is not being carried or inactive
            if (!obj.gameObject.activeInHierarchy || !obj.IsBeingCarried)
                continue;
                
            float distance = Vector2.Distance(transform.position, obj.transform.position);
            
            // Check if this object is within interaction radius and closer than any previous object
            if (distance <= interactionRadius && distance < closestDistance && IsObjectAccepted(obj.ObjectType))
            {
                closestDistance = distance;
                closestObject = obj;
                foundObjectInRange = true;
            }
        }
        
        // Update highlight state based on whether we found a valid object in range
        SetHighlight(foundObjectInRange);
        
        // If an object is in range and player presses Q, place it
        if (foundObjectInRange && Input.GetKeyUp(KeyCode.Q))
        {
            PlaceObject(closestObject);
        }
    }
    
    // Additional trigger detection as backup
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (placedObject != null)
            return;
            
        CarriableObject carriableObject = collision.GetComponent<CarriableObject>();
        if (carriableObject != null && carriableObject.IsBeingCarried && IsObjectAccepted(carriableObject.ObjectType))
        {
            SetHighlight(true);
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        CarriableObject carriableObject = collision.GetComponent<CarriableObject>();
        if (carriableObject != null)
        {
            // Only remove highlight if we don't detect any other objects in range
            // (Update method will handle this more accurately)
            if (!Physics2D.OverlapCircle(transform.position, interactionRadius, LayerMask.GetMask("Default")))
            {
                SetHighlight(false);
            }
        }
    }
    
    /// <summary>
    /// Places the carriable object on this platform
    /// </summary>
    private void PlaceObject(CarriableObject obj)
    {
        if (placedObject != null || !IsObjectAccepted(obj.ObjectType))
        {
            // Platform already has an object or doesn't accept this type
            return;
        }
        
        // Force the object to be dropped first
        if (obj.IsBeingCarried)
        {
            obj.ForceDropObject();
        }
        
        // Set the object's position on the platform
        obj.transform.position = transform.position + objectPlacementOffset;
        
        // Keep a reference to the placed object
        placedObject = obj;
        
        // Disable physics on the object to prevent it from falling off
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = false;
        }
        
        // Make sure the collider is a trigger to prevent pushing
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        
        // Play placement sound
        if (audioSource != null && placementSound != null)
        {
            audioSource.PlayOneShot(placementSound);
        }
        
        // Turn off highlight
        SetHighlight(false);
        
        // Invoke event
        OnObjectPlaced?.Invoke(obj, this);
        
        Debug.Log($"Placed {obj.ObjectType} on {platformName}");
    }
    
    /// <summary>
    /// Removes the currently placed object from the platform
    /// </summary>
    public CarriableObject RemoveObject()
    {
        if (placedObject == null)
        {
            return null;
        }
        
        CarriableObject removedObject = placedObject;
        placedObject = null;
        
        // Play removal sound
        if (audioSource != null && removingSound != null)
        {
            audioSource.PlayOneShot(removingSound);
        }
        
        // Invoke event
        OnObjectRemoved?.Invoke(removedObject, this);
        
        Debug.Log($"Removed {removedObject.ObjectType} from {platformName}");
        
        return removedObject;
    }
    
    /// <summary>
    /// Check if the platform accepts a specific object type
    /// </summary>
    private bool IsObjectAccepted(string objectType)
    {
        // If no types specified, accept all
        if (acceptedObjectTypes == null || acceptedObjectTypes.Length == 0)
        {
            return true;
        }
        
        // Check if the object type is in the accepted list
        foreach (string type in acceptedObjectTypes)
        {
            if (type == objectType)
            {
                return true;
            }
        }
        
        return false;
    }
    
    /// <summary>
    /// Set or remove highlight effect on the platform
    /// </summary>
    private void SetHighlight(bool highlighted)
    {
        if (isHighlighted == highlighted)
        {
            return; // No change needed
        }
        
        isHighlighted = highlighted;
        
        // Change color based on highlight state
        Color targetColor = highlighted ? highlightColor : originalColor;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
        else if (objectRenderer != null)
        {
            objectRenderer.material.color = targetColor;
        }
        
        // Show a helpful prompt to the player about the 'Q' key
        if (highlighted && UIPromptController.Instance != null)
        {
            UIPromptController.Instance.ShowPrompt("Press Q to place object");
        }
        else if (!highlighted && UIPromptController.Instance != null)
        {
            UIPromptController.Instance.HidePrompt();
        }
    }
    
    /// <summary>
    /// Check if the platform currently has an object placed on it
    /// </summary>
    public bool HasObject()
    {
        return placedObject != null;
    }
    
    /// <summary>
    /// Get the type of the currently placed object, or null if empty
    /// </summary>
    public string GetPlacedObjectType()
    {
        return placedObject != null ? placedObject.ObjectType : null;
    }
    
    /// <summary>
    /// Get the currently placed object
    /// </summary>
    public CarriableObject GetPlacedObject()
    {
        return placedObject;
    }
      /// <summary>
    /// Draw the interaction radius in the editor for easier setup
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
    
    /// <summary>
    /// Check if player is within interaction range
    /// </summary>
    private bool CheckPlayerInRange(out float distance)
    {
        distance = float.MaxValue;
        
        // Find player (uses the same method as in InteractiveObject)
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return false;
            
        distance = Vector2.Distance(transform.position, player.transform.position);
        return distance <= interactionRadius;
    }
    
    /// <summary>
    /// Enable physics and collider for the object so it can be picked up again
    /// </summary>
    private void EnableObjectPhysics(CarriableObject obj)
    {
        if (obj == null)
            return;
            
        // Re-enable physics
        Rigidbody2D rb = obj.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.simulated = true;
        }
        
        // Set collider back to non-trigger
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = false;
        }
    }
}

