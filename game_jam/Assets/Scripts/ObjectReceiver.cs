using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Base class for objects that can receive carriable objects (like air vents, trash bins, etc.)
/// Handles the logic of receiving objects and triggering appropriate responses.
/// </summary>
public class ObjectReceiver : MonoBehaviour
{
    [Header("Receiver Settings")]
    [SerializeField] protected string receiverName = "Generic Receiver";
    [SerializeField] protected List<string> acceptedObjectTypes = new List<string>(); // Empty means accept all
    [SerializeField] protected bool destroyObjectsOnReceive = true;
    [SerializeField] protected float absorptionDelay = 0.5f;
    [SerializeField] protected Color highlightColor = new Color(0.5f, 1f, 0.5f, 1f);
    
    [Header("Visual Feedback")]
    [SerializeField] protected GameObject successEffect;
    [SerializeField] protected GameObject rejectEffect;
    [SerializeField] protected AudioClip successSound;
    [SerializeField] protected AudioClip rejectSound;
    
    // Component references
    protected Renderer receiverRenderer;
    protected SpriteRenderer spriteRenderer;
    protected Color originalColor;
    protected AudioSource audioSource;
    protected bool isHighlighted = false;
    
    // Events
    public delegate void ObjectReceivedHandler(CarriableObject obj, ObjectReceiver receiver);
    public static event ObjectReceivedHandler OnObjectReceived;
    public static event ObjectReceivedHandler OnObjectRejected;
    
    protected virtual void Awake()
    {
        // Get references to components
        receiverRenderer = GetComponent<Renderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        // Store original color
        if (receiverRenderer != null)
        {
            originalColor = receiverRenderer.material.color;
        }
        else if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
        
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (successSound != null || rejectSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Make sure we have a collider
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector2(1f, 1f);
        }
        else
        {
            collider.isTrigger = true;
        }
    }
    
    /// <summary>
    /// Process a carriable object that has been dropped onto this receiver
    /// </summary>
    public virtual void ReceiveObject(CarriableObject obj)
    {
        if (obj == null) return;
        
        bool acceptObject = IsObjectAccepted(obj.ObjectType);
        
        if (acceptObject)
        {
            HandleAcceptedObject(obj);
        }
        else
        {
            HandleRejectedObject(obj);
        }
    }
    
    /// <summary>
    /// Check if the receiver accepts this type of object
    /// </summary>
    protected virtual bool IsObjectAccepted(string objectType)
    {
        // If the acceptedObjectTypes list is empty, accept all objects
        if (acceptedObjectTypes.Count == 0) return true;
        
        // Otherwise, check if the object type is in the accepted list
        return acceptedObjectTypes.Contains(objectType);
    }
    
    /// <summary>
    /// Handle an object that's been accepted by this receiver
    /// </summary>
    protected virtual void HandleAcceptedObject(CarriableObject obj)
    {
        Debug.Log($"{receiverName} accepted {obj.ObjectType}");
        
        // Play success sound
        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }
        
        // Show success effect
        if (successEffect != null)
        {
            Instantiate(successEffect, obj.transform.position, Quaternion.identity);
        }
        
        // Trigger event
        OnObjectReceived?.Invoke(obj, this);
        
        // Destroy the object if set to do so
        if (destroyObjectsOnReceive)
        {
            StartCoroutine(AbsorbAndDestroyObject(obj));
        }
    }
    
    /// <summary>
    /// Handle an object that's been rejected by this receiver
    /// </summary>
    protected virtual void HandleRejectedObject(CarriableObject obj)
    {
        Debug.Log($"{receiverName} rejected {obj.ObjectType}");
        
        // Play reject sound
        if (audioSource != null && rejectSound != null)
        {
            audioSource.PlayOneShot(rejectSound);
        }
        
        // Show reject effect
        if (rejectEffect != null)
        {
            Instantiate(rejectEffect, obj.transform.position, Quaternion.identity);
        }
        
        // Trigger event
        OnObjectRejected?.Invoke(obj, this);
    }
    
    /// <summary>
    /// Coroutine to animate the object being absorbed before destroying it
    /// </summary>
    protected virtual System.Collections.IEnumerator AbsorbAndDestroyObject(CarriableObject obj)
    {
        if (obj == null) yield break;
        
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = transform.position;
        float startTime = Time.time;
        
        // Disable the object's collider to prevent further interactions
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }
        
        // Animate the object moving into the receiver
        while (Time.time < startTime + absorptionDelay)
        {
            float t = (Time.time - startTime) / absorptionDelay;
            
            // Move toward receiver
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t);
            
            // Scale down for a shrink effect
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, Vector3.zero, t);
            
            yield return null;
        }
        
        // Destroy the object
        Destroy(obj.gameObject);
    }
    
    /// <summary>
    /// Show or hide highlight when hovering with compatible objects
    /// </summary>
    public virtual void ShowHoverState(bool hovering, string objectType)
    {
        // Only highlight if the object type is accepted
        if (hovering && IsObjectAccepted(objectType))
        {
            isHighlighted = true;
            
            // Change color to indicate valid target
            if (receiverRenderer != null)
            {
                receiverRenderer.material.color = highlightColor;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = highlightColor;
            }
        }
        else if (!hovering && isHighlighted)
        {
            isHighlighted = false;
            
            // Restore original color
            if (receiverRenderer != null)
            {
                receiverRenderer.material.color = originalColor;
            }
            else if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }
}
