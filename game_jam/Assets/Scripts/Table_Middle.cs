using UnityEngine;

/// <summary>
/// Checks if objects placed on the middle table have '3' as the last character.
/// </summary>
public class Table_Middle : TableChecker
{
    protected override void Awake()
    {
        // Set the expected character for this table
        expectedChar = '3';
        
        // Call the parent Awake method
        base.Awake();
    }
}
