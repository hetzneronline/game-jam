using UnityEngine;

/// <summary>
/// Checks if objects placed on the right table have '4' as the last character.
/// </summary>
public class Table_Right : TableChecker
{
    protected override void Awake()
    {
        // Set the expected character for this table
        expectedChar = '4';
        
        // Call the parent Awake method
        base.Awake();
    }
}
