using UnityEngine;
using System.Collections;

/// <summary>
/// An air vent that can receive small objects like apples.
/// Provides visual and audio feedback when objects are inserted.
/// </summary>
public class AirVent : ObjectReceiver
{
    [Header("Air Vent Specific Settings")]
    [SerializeField] private float suctionForce = 5f;
    [SerializeField] private float suctionRadius = 2f;
    [SerializeField] private bool pullNearbyObjects = true;
    [SerializeField] private ParticleSystem airParticles;
    [SerializeField] private AudioClip ventAmbientSound;
    [SerializeField] private AudioClip objectTravelingSound;
    
    [Header("Puzzle Settings")]
    [SerializeField] private int requiredObjectCount = 1;
    [SerializeField] private GameObject unlockEffect;
    [SerializeField] private string nextLevelName = "";
    [SerializeField] private GameObject objectToActivate;
    
    // State tracking
    private int currentObjectCount = 0;
    private bool isPuzzleSolved = false;
    private AudioSource ambientSource;
    
    protected override void Awake()
    {
        base.Awake();
        
        // Set the receiver name if it's not already set
        if (receiverName == "Generic Receiver")
        {
            receiverName = "Air Vent";
        }
        
        // Set up default accepted object types if none are specified
        if (acceptedObjectTypes.Count == 0)
        {
            acceptedObjectTypes.Add("Apple");
            acceptedObjectTypes.Add("Paper");
            acceptedObjectTypes.Add("Coin");
            acceptedObjectTypes.Add("Marble");
        }
        
        // Set up ambient sound
        if (ventAmbientSound != null)
        {
            ambientSource = gameObject.AddComponent<AudioSource>();
            ambientSource.clip = ventAmbientSound;
            ambientSource.loop = true;
            ambientSource.volume = 0.3f;
            ambientSource.spatialBlend = 1.0f;
            ambientSource.maxDistance = 10f;
            ambientSource.Play();
        }
        
        // Setup air particles if needed
        if (airParticles == null)
        {
            GameObject particleObj = new GameObject("VentParticles");
            particleObj.transform.parent = transform;
            particleObj.transform.localPosition = Vector3.zero;
            airParticles = particleObj.AddComponent<ParticleSystem>();
            
            // Configure a simple dust particle system
            var main = airParticles.main;
            main.startSize = 0.1f;
            main.startSpeed = 1f;
            main.startLifetime = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            
            var emission = airParticles.emission;
            emission.rateOverTime = 10f;
            
            var shape = airParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;
        }
    }
    
    private void Start()
    {
        // Initialize the air vent
        Debug.Log($"Air Vent initialized. Required objects: {requiredObjectCount}");
        
        // Start the suction effect
        if (pullNearbyObjects)
        {
            StartCoroutine(PullNearbyObjects());
        }
    }
    
    // Override the base method to track the number of received objects
    protected override void HandleAcceptedObject(CarriableObject obj)
    {
        base.HandleAcceptedObject(obj);
        
        // Increment object count
        currentObjectCount++;
        
        Debug.Log($"Air Vent received object {currentObjectCount}/{requiredObjectCount}");
        
        // Check if the puzzle is solved
        if (currentObjectCount >= requiredObjectCount && !isPuzzleSolved)
        {
            SolvePuzzle();
        }
    }
    
    // Handle the vent successfully receiving all required objects
    private void SolvePuzzle()
    {
        isPuzzleSolved = true;
        Debug.Log("Air Vent puzzle solved!");
        
        // Play a special effect
        if (unlockEffect != null)
        {
            Instantiate(unlockEffect, transform.position, Quaternion.identity);
        }
        
        // Activate the target object if specified
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }
        
        // Change the appearance to indicate completion
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.green;
        }
        else if (receiverRenderer != null)
        {
            receiverRenderer.material.color = Color.green;
        }
        
        // Load next level if specified
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            StartCoroutine(LoadNextLevelAfterDelay(2f));
        }
    }
    
    // Coroutine to pull nearby objects toward the vent
    private IEnumerator PullNearbyObjects()
    {
        while (true)
        {
            // Find all carriable objects in range
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, suctionRadius);
            
            foreach (Collider2D collider in colliders)
            {
                CarriableObject carriableObj = collider.GetComponent<CarriableObject>();
                
                // Only pull objects that aren't being carried and are of an accepted type
                if (carriableObj != null && 
                    IsObjectAccepted(carriableObj.ObjectType) &&
                    Vector2.Distance(transform.position, carriableObj.transform.position) <= suctionRadius)
                {
                    Rigidbody2D rb = carriableObj.GetComponent<Rigidbody2D>();
                    
                    if (rb != null && rb.simulated)
                    {
                        // Calculate direction to the vent
                        Vector2 direction = (Vector2)transform.position - rb.position;
                        
                        // Apply force scaled by distance (stronger when closer)
                        float distanceFactor = 1f - (direction.magnitude / suctionRadius);
                        float appliedForce = suctionForce * distanceFactor;
                        
                        rb.AddForce(direction.normalized * appliedForce);
                    }
                }
            }
            
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    // Handle loading the next level
    private IEnumerator LoadNextLevelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (!string.IsNullOrEmpty(nextLevelName))
        {
            Debug.Log($"Loading next level: {nextLevelName}");
            UnityEngine.SceneManagement.SceneManager.LoadScene(nextLevelName);
        }
    }
    
    // Override the absorption coroutine to add vent-specific effects
    protected override IEnumerator AbsorbAndDestroyObject(CarriableObject obj)
    {
        if (obj == null) yield break;
        
        Vector3 startPos = obj.transform.position;
        Vector3 targetPos = transform.position;
        float startTime = Time.time;
        
        // Play the traveling sound
        if (audioSource != null && objectTravelingSound != null)
        {
            audioSource.PlayOneShot(objectTravelingSound);
        }
        
        // Increase particle emission temporarily
        if (airParticles != null)
        {
            var emission = airParticles.emission;
            float originalRate = emission.rateOverTime.constant;
            emission.rateOverTime = originalRate * 3f;
            
            // Reset emission rate after a delay
            StartCoroutine(ResetParticleEmission(originalRate, absorptionDelay));
        }
        
        // Disable the object's collider to prevent further interactions
        Collider2D objCollider = obj.GetComponent<Collider2D>();
        if (objCollider != null)
        {
            objCollider.enabled = false;
        }
        
        // Animate the object moving into the receiver with a slight curve
        while (Time.time < startTime + absorptionDelay)
        {
            float t = (Time.time - startTime) / absorptionDelay;
            
            // Create a slight curved path for better visual effect
            Vector3 curveOffset = Vector3.up * Mathf.Sin(t * Mathf.PI) * 0.5f;
            
            // Move toward receiver
            obj.transform.position = Vector3.Lerp(startPos, targetPos, t) + curveOffset;
            
            // Scale down for a shrink effect
            obj.transform.localScale = Vector3.Lerp(obj.transform.localScale, Vector3.zero, t);
            
            // Add some rotation for style
            obj.transform.Rotate(0, 0, 720 * Time.deltaTime);
            
            yield return null;
        }
        
        // Destroy the object
        Destroy(obj.gameObject);
    }
    
    // Reset particle emission rate
    private IEnumerator ResetParticleEmission(float originalRate, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (airParticles != null)
        {
            var emission = airParticles.emission;
            emission.rateOverTime = originalRate;
        }
    }
    
    // Visualize the suction range in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.5f, 1f, 0.3f);
        Gizmos.DrawSphere(transform.position, suctionRadius);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, suctionRadius);
    }
}
