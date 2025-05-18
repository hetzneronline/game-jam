using UnityEngine;

/// <summary>
/// Locks a door until all tables have the correct objects placed on them.
/// </summary>
public class TableLockedDoor : MonoBehaviour
{
    [Header("Door Settings")]
    [SerializeField] private BoxCollider2D doorBlocker; // Collider that blocks the door
    [SerializeField] private string promptMessage = "All tables need correct objects to unlock the door"; // Message to show when door is locked
    [SerializeField] private string unlockedMessage = "Door unlocked!"; // Message when door is unlocked
    [SerializeField] private float checkInterval = 0.5f; // How often to check tables (seconds)
    [SerializeField] private bool showDebugMessages = true; // Whether to show debug messages
    [SerializeField] private AudioClip unlockSound; // Sound played when door unlocks
    
    [Header("Teleporter Settings")]
    [SerializeField] private SceneTransition connectedTransition; // Optional scene transition
    
    private TableChecker[] allTables; // All table checkers in the scene
    private bool isDoorUnlocked = false;
    private float checkTimer = 0f;
    private AudioSource audioSource;
      private void Start()
    {
        // Find all table checkers in the scene
        allTables = FindObjectsOfType<TableChecker>();
        
        if (allTables.Length == 0)
        {
            Debug.LogWarning("No TableChecker components found in the scene!");
        }
        else if (showDebugMessages)
        {
            Debug.Log($"TableLockedDoor: Found {allTables.Length} tables to check");
        }
        
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && unlockSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // If no door blocker assigned, try to find one or create one
        if (doorBlocker == null)
        {
            doorBlocker = GetComponent<BoxCollider2D>();
            
            if (doorBlocker == null)
            {
                // Create a new blocker
                doorBlocker = gameObject.AddComponent<BoxCollider2D>();
                doorBlocker.size = new Vector2(1, 1); // Default size, adjust in inspector
                doorBlocker.isTrigger = false; // Not a trigger, to block player
            }
        }
        
        // Find scene transition if not assigned
        if (connectedTransition == null)
        {
            connectedTransition = GetComponent<SceneTransition>();
            if (connectedTransition == null)
            {
                connectedTransition = GetComponentInChildren<SceneTransition>();
            }
        }
        
        // Disable teleporter initially if we found one
        if (connectedTransition != null)
        {
            connectedTransition.enabled = false;
            if (showDebugMessages)
            {
                Debug.Log("Found teleporter and disabled it initially");
            }
        }
        
        // Make sure the door is initially locked
        LockDoor();
        
        // Initial check
        CheckAllTables();
    }
    
    private void Update()
    {
        // Check tables periodically rather than every frame
        checkTimer += Time.deltaTime;
        if (checkTimer >= checkInterval)
        {
            checkTimer = 0f;
            CheckAllTables();
        }
    }
    
    /// <summary>
    /// Check if all tables have the correct objects
    /// </summary>
    private void CheckAllTables()
    {
        bool allTablesCorrect = true;
        
        foreach (TableChecker table in allTables)
        {
            if (!table.CheckObject())
            {
                allTablesCorrect = false;
                
                if (showDebugMessages)
                {
                    Debug.Log($"Table {table.gameObject.name} doesn't have the correct object (expecting '{table.GetExpectedChar()}')");
                }
                
                break;  // No need to check further if one is wrong
            }
        }
        
        // Update door state if it changed
        if (allTablesCorrect && !isDoorUnlocked)
        {
            UnlockDoor();
        }
        else if (!allTablesCorrect && isDoorUnlocked)
        {
            LockDoor();
        }
    }    /// <summary>
    /// Lock the door
    /// </summary>
    private void LockDoor()
    {
        isDoorUnlocked = false;
        
        // Enable or recreate the door blocker if needed
        if (doorBlocker == null)
        {
            // Recreate the blocker if it was destroyed
            doorBlocker = gameObject.AddComponent<BoxCollider2D>();
            doorBlocker.size = new Vector2(1, 1); // Default size
            doorBlocker.isTrigger = false; // Not a trigger, to block player
        }
        else
        {
            // Just enable it if it already exists
            doorBlocker.enabled = true;
        }
        
        // Show message to player
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.ShowPrompt(promptMessage);
        }
        
        // Disable teleporter if found
        if (connectedTransition != null)
        {
            connectedTransition.enabled = false;
            
            if (showDebugMessages)
            {
                Debug.Log("Scene transition disabled");
            }
        }
        
        if (showDebugMessages)
        {
            Debug.Log("Door locked: Not all tables have the correct objects");
        }
    }    /// <summary>
    /// Unlock the door
    /// </summary>
    private void UnlockDoor()
    {
        isDoorUnlocked = true;
        
        // Completely destroy the door blocker instead of just disabling it
        if (doorBlocker != null)
        {
            Destroy(doorBlocker);
            doorBlocker = null;
        }
        
        // Play unlock sound
        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
        
        // Show unlock message
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.ShowPrompt(unlockedMessage);
            
            // Hide the message after a short delay
            Invoke("HidePrompt", 2.0f);
        }
        
        // Enable the scene transition if it exists
        if (connectedTransition != null)
        {
            connectedTransition.enabled = true;
            
            if (showDebugMessages)
            {
                Debug.Log("Scene transition enabled for teleporter");
            }
        }
        
        if (showDebugMessages)
        {
            Debug.Log("Door unlocked: All tables have the correct objects!");
        }
    }
    
    /// <summary>
    /// Hide the UI prompt
    /// </summary>
    private void HidePrompt()
    {
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.HidePrompt();
        }
    }
    
    /// <summary>
    /// Check if the door is currently unlocked
    /// </summary>
    public bool IsDoorUnlocked()
    {
        return isDoorUnlocked;
    }
    
    /// <summary>
    /// Force a manual check of all tables
    /// </summary>
    public void ForceCheckTables()
    {
        CheckAllTables();
    }
    
    /// <summary>
    /// Display the door status in the editor for debugging
    /// </summary>
    private void OnDrawGizmos()
    {
        // Draw a locked or unlocked icon in the scene view
        Gizmos.color = isDoorUnlocked ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 0.1f));
    }
}
