using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// This class ensures there's only one EventSystem in the scene
public class EventSystemManager : MonoBehaviour
{
    private static EventSystemManager _instance;
    private static bool _initialized = false;
    
    public static EventSystemManager Instance
    {
        get
        {
            if (_instance == null && !_initialized)
            {
                _initialized = true;
                
                EventSystemManager[] instances = FindObjectsOfType<EventSystemManager>();
                if (instances.Length > 0)
                {
                    _instance = instances[0];
                    
                    // Clean up any duplicates
                    if (instances.Length > 1)
                    {
                        Debug.LogWarning("Multiple EventSystemManager instances detected. Cleaning up duplicates.");
                        for (int i = 1; i < instances.Length; i++)
                        {
                            Destroy(instances[i].gameObject);
                        }
                    }
                }
                else
                {
                    // Create a new instance
                    GameObject managerObj = new GameObject("EventSystemManager");
                    _instance = managerObj.AddComponent<EventSystemManager>();
                    DontDestroyOnLoad(managerObj);
                }
            }
            
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Enforce singleton pattern
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize by cleaning up event systems
        CleanupDuplicateEventSystems();
    }
    
    // Ensure there's exactly one EventSystem in the scene
    public void CleanupDuplicateEventSystems()
    {
        EventSystem[] eventSystems = FindObjectsOfType<EventSystem>();
        
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
        else if (eventSystems.Length == 0)
        {
            // No EventSystem found, create one
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("Created new EventSystem");
        }
        else
        {
            Debug.Log("Found existing EventSystem: " + eventSystems[0].gameObject.name);
        }
    }
    
    // Call this method to ensure there's only one EventSystem
    public static void EnsureSingleEventSystem()
    {
        Instance.CleanupDuplicateEventSystems();
    }
}
