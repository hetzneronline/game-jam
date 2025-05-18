using UnityEngine;
using UnityEngine.EventSystems;

// Helps set up the test scene with properly tagged objects
public class GameSceneSetup : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Vector3 playerStartPosition = Vector3.zero;
    
    [Header("Interactive Objects")]
    [SerializeField] private GameObject collectiblePrefab;
    [SerializeField] private int numberOfCollectibles = 5;
    [SerializeField] private float spawnRadius = 5f;
    
    [Header("UI Setup")]
    [SerializeField] private bool createUIAtStart = true;
    [SerializeField] private bool cleanupDuplicateEventSystems = true;
    
    private void Start()
    {
        if (cleanupDuplicateEventSystems)
        {
            CleanupEventSystems();
        }
        
        SetupScene();
    }
    
    public void SetupScene()
    {
        // Create player if not exists
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            if (playerPrefab != null)
            {
                player = Instantiate(playerPrefab, playerStartPosition, Quaternion.identity);
            }
            else
            {
                player = CreateDefaultPlayer();
            }
            
            player.tag = "Player";
        }
        
        // Create collectible items
        if (collectiblePrefab != null)
        {
            SpawnCollectibles();
        }
          // Setup UI if needed
        if (createUIAtStart)
        {
            UISetupHelper uiHelper = FindAnyObjectByType<UISetupHelper>();
            if (uiHelper == null)
            {
                GameObject uiHelperObj = new GameObject("UISetupHelper");
                uiHelper = uiHelperObj.AddComponent<UISetupHelper>();
            }
            
            uiHelper.SetupInteractionUI();
        }
        
        // Make sure we have a score manager
        if (ScoreManager.Instance == null)
        {
            GameObject scoreManagerObj = new GameObject("ScoreManager");
            scoreManagerObj.AddComponent<ScoreManager>();
            Debug.Log("Created ScoreManager");
        }
    }
    
    private GameObject CreateDefaultPlayer()
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Cube);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = playerStartPosition;
        player.transform.localScale = new Vector3(1f, 1f, 1f);
        
        // Add required components
        player.AddComponent<PlayerController>();
        
        // Add a nice material
        Renderer renderer = player.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = new Color(0.2f, 0.6f, 1f); // Blue color
        }
        
        Debug.Log("Created default player cube");
        return player;
    }
    
    private void SpawnCollectibles()
    {
        for (int i = 0; i < numberOfCollectibles; i++)
        {
            // Calculate random position within spawn radius
            float angle = i * (360f / numberOfCollectibles);
            float x = Mathf.Cos(angle * Mathf.Deg2Rad) * spawnRadius;
            float z = Mathf.Sin(angle * Mathf.Deg2Rad) * spawnRadius;
            Vector3 spawnPos = new Vector3(x, 0.5f, z);
            
            GameObject collectible = Instantiate(collectiblePrefab, spawnPos, Quaternion.identity);
            collectible.name = "Collectible_" + i;
        }
    }
      // This function can be called from the Unity Editor to set up the scene
    [ContextMenu("Setup Scene")]
    public void EditorSetupScene()
    {
        SetupScene();
    }
      // Clean up duplicate EventSystem components in the scene
    private void CleanupEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning("Multiple EventSystem components detected in scene. Keeping only one.");
            
            // Keep the first one we found and destroy the rest
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log("Removing duplicate EventSystem: " + eventSystems[i].gameObject.name);
                Destroy(eventSystems[i].gameObject);
            }
        }
    }
    
    // For manual cleanup of EventSystems from the Inspector
    [ContextMenu("Clean Up Event Systems")]
    public void CleanupEventSystemsManual()
    {
        CleanupEventSystems();
    }
}
