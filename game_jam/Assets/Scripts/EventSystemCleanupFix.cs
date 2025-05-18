using UnityEngine;
using UnityEngine.EventSystems;

// This script is a temporary fix to help clean up duplicate EventSystem components
// Add it to any GameObject in your scene, run the game once, then you can remove it
public class EventSystemCleanupFix : MonoBehaviour
{
    private void Awake()
    {
        // Find all EventSystem components in the scene
        EventSystem[] eventSystems = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);
        
        // Log what we found
        Debug.Log($"Found {eventSystems.Length} EventSystem components in the scene");
        
        if (eventSystems.Length > 1)
        {
            Debug.LogWarning("Cleaning up duplicate EventSystem components");
            
            // Keep the first one and destroy the rest
            for (int i = 1; i < eventSystems.Length; i++)
            {
                Debug.Log($"Destroying duplicate EventSystem on GameObject: {eventSystems[i].gameObject.name}");
                Destroy(eventSystems[i].gameObject);
            }
            
            Debug.Log("Cleanup complete. You should no longer see the duplicate EventSystem error.");
        }
        else
        {
            Debug.Log("No duplicate EventSystem components found.");
        }
    }
}
