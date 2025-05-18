using UnityEngine;

/// <summary>
/// Checks if objects placed on the far right table have '5' as the last character.
/// </summary>
public class Table_FarRight : TableChecker
{
    protected override void Awake()
    {
        // Set the expected character for this table
        expectedChar = '5';
        
        // Call the parent Awake method
        base.Awake();
    }
}
