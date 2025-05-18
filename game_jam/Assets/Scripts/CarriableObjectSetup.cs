using UnityEngine;

/// <summary>
/// Helper class to easily create and set up carriable objects like apples in your scene
/// </summary>
public class CarriableObjectSetup : MonoBehaviour
{
    [Header("Object Settings")]
    [SerializeField] private string objectType = "Apple";
    [SerializeField] private Color objectColor = Color.red;
    [SerializeField] private float objectRadius = 0.25f;
    [SerializeField] private bool usePhysics = true;
    
    [Header("Visual Effects")]
    [SerializeField] private bool addVisualEffects = true;
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotationSpeed = 45f;
    [SerializeField] private bool bobUpDown = true;
    [SerializeField] private float bobHeight = 0.1f;
    [SerializeField] private float bobSpeed = 1f;
    
    // Runtime variables
    private Vector3 startPosition;
    private float timer;
    
    private void Start()
    {
        startPosition = transform.position;
        timer = Random.value * 6.28f; // Randomize starting phase
        
        // Make sure we have a CarriableObject component
        CarriableObject carriable = GetComponent<CarriableObject>();
        if (carriable == null)
        {
            carriable = gameObject.AddComponent<CarriableObject>();
        }
        
        // Make sure we have a renderer
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = objectColor;
        }
        
        // Add a sprite renderer if this is a 2D object
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = objectColor;
        }
        
        // Add a rigidbody if using physics
        if (usePhysics && GetComponent<Rigidbody2D>() == null)
        {
            Rigidbody2D rb = gameObject.AddComponent<Rigidbody2D>();
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.mass = 0.5f;
            rb.linearDamping = 1f;
        }
        
        // Add a collider if needed
        if (GetComponent<Collider2D>() == null)
        {
            CircleCollider2D collider = gameObject.AddComponent<CircleCollider2D>();
            collider.radius = objectRadius;
        }
    }
    
    private void Update()
    {
        if (!addVisualEffects) return;
        
        // Only apply visual effects if not being carried
        CarriableObject carriable = GetComponent<CarriableObject>();
        if (carriable != null && carriable.enabled)
        {
            timer += Time.deltaTime * bobSpeed;
            
            // Apply bobbing effect
            if (bobUpDown)
            {
                float yOffset = Mathf.Sin(timer) * bobHeight;
                transform.position = new Vector3(
                    startPosition.x, 
                    startPosition.y + yOffset, 
                    startPosition.z);
            }
            
            // Apply rotation effect
            if (rotateObject)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
    }
    
    /// <summary>
    /// Create a default apple object at the specified position
    /// </summary>
    public static GameObject CreateApple(Vector3 position)
    {
        // Create a new sphere
        GameObject apple = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        apple.name = "Apple";
        apple.transform.position = position;
        apple.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Add the carriable component
        CarriableObject carriable = apple.AddComponent<CarriableObject>();
        
        // Add the setup helper
        CarriableObjectSetup setup = apple.AddComponent<CarriableObjectSetup>();
        setup.objectType = "Apple";
        setup.objectColor = Color.red;
        
        return apple;
    }
    
    // Menu item to create an apple in the scene view
    [ContextMenu("Create Apple")]
    public void CreateAppleAtPosition()
    {
        GameObject apple = CreateApple(transform.position);
        apple.transform.SetParent(transform.parent);
        
        // Destroy the current setup object since it's just a placeholder
        DestroyImmediate(gameObject);
    }
}
