using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// A component that handles teleportation when a player enters its trigger volume.
/// Works with TableLockedDoor to ensure teleportation only happens when the door is unlocked.
/// </summary>
public class TableDoorTeleporter : MonoBehaviour
{
    [Header("Teleporter Settings")]
    [SerializeField] private string targetSceneName;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool showDebugMessages = false;
    [SerializeField] private AudioClip teleportSound;
    
    [Header("Dependencies")]
    [SerializeField] private TableLockedDoor linkedDoor;
    
    private AudioSource audioSource;
    private bool isTeleporting = false;
    
    private void Start()
    {
        // Set up audio
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && teleportSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Try to find the door if not linked
        if (linkedDoor == null)
        {
            linkedDoor = GetComponentInParent<TableLockedDoor>();
            if (linkedDoor == null)
            {
                linkedDoor = GetComponent<TableLockedDoor>();
            }
        }
        
        // Make sure we have a valid scene name
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogWarning("TableDoorTeleporter has no target scene specified!");
        }
        
        // Make sure this object has a collider set to trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
            ((BoxCollider2D)collider).size = new Vector2(1, 1);
            if (showDebugMessages)
            {
                Debug.Log("Added trigger collider to TableDoorTeleporter");
            }
        }
        
        // Make sure it's a trigger
        collider.isTrigger = true;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if this is the player
        if (collision.CompareTag(playerTag) && !isTeleporting)
        {
            // Check if door is unlocked before teleporting
            if (linkedDoor != null && !linkedDoor.IsDoorUnlocked())
            {
                if (showDebugMessages)
                {
                    Debug.Log("Player tried to use teleporter, but door is locked");
                }
                return;
            }
            
            // If we have no linked door, or the door is unlocked
            TriggerTeleport();
        }
    }
    
    /// <summary>
    /// Trigger the teleport to the target scene
    /// </summary>
    public void TriggerTeleport()
    {
        if (string.IsNullOrEmpty(targetSceneName) || isTeleporting)
        {
            return;
        }
        
        isTeleporting = true;
        
        if (showDebugMessages)
        {
            Debug.Log($"Teleporting to scene: {targetSceneName}");
        }
        
        // Play teleport sound if assigned
        if (audioSource != null && teleportSound != null)
        {
            audioSource.PlayOneShot(teleportSound);
        }
        
        // Load the target scene
        SceneManager.LoadScene(targetSceneName);
    }
}
