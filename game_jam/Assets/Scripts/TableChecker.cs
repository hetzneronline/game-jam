using UnityEngine;

/// <summary>
/// Base script that checks if objects placed on a table have the correct last character in their name.
/// </summary>
public class TableChecker : MonoBehaviour
{
    [SerializeField] protected char expectedChar; // The character this table expects
    protected ObjectPlatform platform;
    
    protected virtual void Awake()
    {
        // Get the ObjectPlatform component
        platform = GetComponent<ObjectPlatform>();
        if (platform == null)
        {
            Debug.LogError("TableChecker requires an ObjectPlatform component on the same GameObject.");
            enabled = false;
            return;
        }
    }
    
    /// <summary>
    /// Checks if the current placed object has the expected last character
    /// </summary>
    /// <returns>True if the object matches the expected character, false otherwise</returns>
    public virtual bool CheckObject()
    {
        CarriableObject obj = platform.GetPlacedObject();
        
        if (obj == null)
            return false;
            
        string objName = obj.name;
        
        if (objName.Length > 0)
        {
            char lastChar = objName[objName.Length - 1];
            return lastChar == expectedChar;
        }
        
        return false;
    }
    
    /// <summary>
    /// Gets the expected character for this table
    /// </summary>
    /// <returns>The character this table is checking for</returns>
    public char GetExpectedChar()
    {
        return expectedChar;
    }
}
