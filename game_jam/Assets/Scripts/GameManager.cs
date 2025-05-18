using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq; // Add this for Select extension method

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [HideInInspector]
    public string targetSpawnPoint = "SpawnPoint";
    
    private Vector3 playerPosition;
    
    // Add this field
    private bool isFirstSceneLoaded = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            
            // Ensure we're starting in the correct scene
            // If the game is started from a scene that's not scene 0, load scene 0
            if (SceneManager.GetActiveScene().buildIndex != 0 && !isFirstSceneLoaded)
            {
                isFirstSceneLoaded = true;
                SceneManager.LoadScene(0);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void SavePlayerState()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerPosition = player.transform.position;
            Debug.Log("Saved player position: " + playerPosition);
        }
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Find the player in the new scene
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            // Find the spawn point - first try exact match
            GameObject spawnPoint = GameObject.Find(targetSpawnPoint);
            
            // If not found, try case-insensitive search
            if (spawnPoint == null)
            {
                Debug.Log($"Searching for spawn point '{targetSpawnPoint}' with case-insensitive match...");
                foreach (SpawnPoint sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
                {
                    if (sp.name.Equals(targetSpawnPoint, System.StringComparison.OrdinalIgnoreCase))
                    {
                        spawnPoint = sp.gameObject;
                        Debug.Log($"Found spawn point with case-insensitive match: {sp.name}");
                        break;
                    }
                }
            }
            
            if (spawnPoint != null)
            {
                // Position the player at the spawn point
                player.transform.position = spawnPoint.transform.position;
                Debug.Log($"Positioned player at spawn point: {spawnPoint.name}");
            }
            else
            {
                Debug.LogWarning($"Spawn point '{targetSpawnPoint}' not found in scene {scene.name}");
                Debug.Log("Available spawn points: " + 
                    string.Join(", ", FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None).Select(sp => sp.name).ToArray()));
            }
        }
        else
        {
            Debug.LogWarning("Player not found in the loaded scene");
        }
        
        // Trigger a fade-in effect when a new scene loads
        if (FadeManager.Instance != null)
        {
            FadeManager.Instance.FadeIn();
        }
    }
    
    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Add this method to allow repositioning the player at any time
    public void RepositionPlayerAtSpawnPoint()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        
        GameObject spawnPoint = GameObject.Find(targetSpawnPoint);
        if (spawnPoint == null)
        {
            foreach (SpawnPoint sp in FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None))
            {
                if (sp.name.Equals(targetSpawnPoint, System.StringComparison.OrdinalIgnoreCase))
                {
                    spawnPoint = sp.gameObject;
                    break;
                }
            }
        }
        
        if (spawnPoint != null)
        {
            player.transform.position = spawnPoint.transform.position;
            Debug.Log($"Repositioned player at spawn point: {spawnPoint.name}");
        }
    }
}