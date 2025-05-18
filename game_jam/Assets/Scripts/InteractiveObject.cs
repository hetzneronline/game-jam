using UnityEngine;

public class InteractiveObject : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] protected float interactionRadius = 2f;
    [SerializeField] protected string interactionPrompt = "Press E to interact";
    [SerializeField] protected Color highlightColor = new Color(1f, 0.92f, 0.016f, 1f); // Yellow highlight
    [SerializeField] protected float blinkSpeed = 2f; // Blinks per second
    
    protected Color originalColor;
    protected Renderer objectRenderer;
    protected SpriteRenderer spriteRenderer;
    protected bool playerInRange = false;
    protected float blinkTimer = 0f;
    protected Transform playerTransform;
    
    protected virtual void Start()
    {
        // Find the player
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogWarning("Player not found! Make sure player has 'Player' tag.");
        }
        
        // Get the renderer component (for 3D objects)
        objectRenderer = GetComponent<Renderer>();
        
        // Get sprite renderer (for 2D objects)
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
        
        // Store original color
        if (objectRenderer != null)
        {
            originalColor = objectRenderer.material.color;
        }
        else if (spriteRenderer != null) 
        {
            originalColor = spriteRenderer.color;
        }
        else
        {
            Debug.LogWarning("No Renderer found on " + gameObject.name);
        }
        
        // Make sure there's a collider for detection
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("Added BoxCollider2D to " + gameObject.name);
        }
    }
    
    protected virtual void Update()
    {
        if (playerTransform == null)
            return;
            
        // Check if player is in range
        float distanceToPlayer = Vector2.Distance(playerTransform.position, transform.position);
        bool wasInRange = playerInRange;
        playerInRange = distanceToPlayer <= interactionRadius;
        
        // State change detection
        if (wasInRange != playerInRange)
        {
            if (!playerInRange)
            {
                // Reset color when player leaves range
                ResetObjectColor();
            }
            
            // Show or hide interaction prompt
            if (playerInRange && UIPromptController.Instance != null)
            {
                UIPromptController.Instance.ShowPrompt(interactionPrompt);
            }
            else if (!playerInRange && UIPromptController.Instance != null)
            {
                UIPromptController.Instance.HidePrompt();
            }
        }
        
        // Handle highlighting/blinking effect
        if (playerInRange)
        {
            blinkTimer += Time.deltaTime * blinkSpeed;
            
            // Create pulsing effect using sine wave
            float pulseAmount = Mathf.Sin(blinkTimer * Mathf.PI) * 0.5f + 0.5f;
            
            // Apply color effect based on available renderer
            if (objectRenderer != null)
            {
                objectRenderer.material.color = Color.Lerp(originalColor, highlightColor, pulseAmount);
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(originalColor, highlightColor, pulseAmount);
            }
            
            // Check for interaction input
            if (Input.GetKeyDown(KeyCode.E))
            {
                Interact();
            }
        }
    }
    
    // Reset object color based on available renderer
    protected void ResetObjectColor()
    {
        if (objectRenderer != null)
        {
            objectRenderer.material.color = originalColor;
        }
        else if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
    
    // Method called when player interacts with this object
    protected virtual void Interact()
    {
        Debug.Log("Player interacted with " + gameObject.name);
        // Override this method in derived classes to implement specific interactions
    }
    
    // Show interaction radius in editor for debugging
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}