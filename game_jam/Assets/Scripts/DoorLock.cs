using UnityEngine;

/// <summary>
/// Locks a door until all tables have the correct objects placed on them.
/// </summary>
public class DoorLock : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject doorBlocker; // The collider or object that blocks the door
    [SerializeField] private float checkInterval = 0.5f; // How often to check tables (seconds)
    
    [Header("Settings")]
    [SerializeField] private bool showDebugMessages = true; // Whether to show debug messages
    [SerializeField] private string promptMessage = "All tables need correct objects to unlock the door"; // Message to show when door is locked
    
    private TableChecker[] allTables; // All table checkers in the scene
    private bool isDoorUnlocked = false;
    private float checkTimer = 0f;
    
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
            Debug.Log($"DoorLock: Found {allTables.Length} tables to check");
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
        
        // Update door state
        if (allTablesCorrect && !isDoorUnlocked)
        {
            UnlockDoor();
        }
        else if (!allTablesCorrect && isDoorUnlocked)
        {
            LockDoor();
        }
    }
    
    /// <summary>
    /// Lock the door
    /// </summary>
    private void LockDoor()
    {
        isDoorUnlocked = false;
        
        // Enable the door blocker
        if (doorBlocker != null)
        {
            doorBlocker.SetActive(true);
        }
        
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.ShowPrompt(promptMessage);
        }
        
        if (showDebugMessages)
        {
            Debug.Log("Door locked: Not all tables have the correct objects");
        }
    }
    
    /// <summary>
    /// Unlock the door
    /// </summary>
    private void UnlockDoor()
    {
        isDoorUnlocked = true;
        
        // Disable the door blocker
        if (doorBlocker != null)
        {
            doorBlocker.SetActive(false);
        }
        
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.ShowPrompt("Door unlocked!");
            
            // Hide the message after a short delay
            Invoke("HidePrompt", 2.0f);
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
    /// Force a check of all tables right now
    /// </summary>
    public void ForceCheckTables()
    {
        CheckAllTables();
    }
}
