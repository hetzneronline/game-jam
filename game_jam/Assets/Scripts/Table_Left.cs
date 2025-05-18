using UnityEngine;

/// <summary>
/// Checks if objects placed on the left table have '2' as the last character.
/// </summary>
public class Table_Left : TableChecker
{
    protected override void Awake()
    {
        // Set the expected character for this table
        expectedChar = '2';
        
        // Call the parent Awake method
        base.Awake();
    }
}
