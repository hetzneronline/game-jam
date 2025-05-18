using UnityEngine;

// Helper class to create a collectible item in the editor
public class CollectibleSetupHelper : MonoBehaviour
{
    [SerializeField] private Color itemColor = new Color(1f, 0.8f, 0.1f, 1f); // Golden
    [SerializeField] private bool useRotation = true;
    [SerializeField] private float rotationSpeed = 50f;
    
    private void Start()
    {
        // Set initial appearance
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = itemColor;
        }
    }
    
    private void Update()
    {
        // Add a simple rotation animation
        if (useRotation)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }
    
    // This function can be used to create a collectible at runtime
    public static GameObject CreateDefaultCollectible()
    {
        // Create primitive object
        GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        collectible.name = "Collectible";
        collectible.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // Add CollectibleItem component
        CollectibleItem item = collectible.AddComponent<CollectibleItem>();
        
        // Add this helper component
        collectible.AddComponent<CollectibleSetupHelper>();
        
        return collectible;
    }
    
    // Menu item to create collectible in the editor
    [ContextMenu("Create Default Collectible")]
    public void CreateCollectibleInEditor()
    {
        GameObject collectible = CreateDefaultCollectible();
        collectible.transform.position = transform.position;
    }
}
