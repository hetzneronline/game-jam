using UnityEngine;

/// <summary>
/// A specialized version of the ObjectPlatform designed to showcase objects
/// with visual effects like rotation, floating, or light effects
/// </summary>
public class DisplayPedestal : ObjectPlatform
{
    [Header("Display Effects")]
    [SerializeField] private bool rotateObject = true;
    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private bool floatObject = true;
    [SerializeField] private float floatHeight = 0.2f;
    [SerializeField] private float floatSpeed = 1f;
    [SerializeField] private GameObject lightEffect;
    
    // Timer for animation
    private float timer = 0f;
    private Vector3 basePosition;
    
    private void Start()
    {
        // Store the base position for the float effect
        basePosition = transform.position + new Vector3(0f, 0.5f, 0f);
        
        // Randomize start time for variation when multiple pedestals are used
        timer = Random.Range(0f, 6.28f);
        
        // Create light effect if specified
        if (lightEffect != null)
        {
            Instantiate(lightEffect, transform.position, Quaternion.identity, transform);
        }
    }
    
    private void Update()
    {
        timer += Time.deltaTime * floatSpeed;
        
        // Get the placed object
        CarriableObject placedObject = GetPlacedObject();
        
        if (placedObject != null)
        {
            // Apply rotation effect
            if (rotateObject)
            {
                placedObject.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
            
            // Apply float effect
            if (floatObject)
            {
                float yOffset = Mathf.Sin(timer) * floatHeight;
                placedObject.transform.position = basePosition + new Vector3(0f, yOffset, 0f);
            }
        }
    }
}
