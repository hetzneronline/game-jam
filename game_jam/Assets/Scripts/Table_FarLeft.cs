using UnityEngine;

/// <summary>
/// Checks if objects placed on the far left table have '1' as the last character.
/// </summary>
public class Table_FarLeft : TableChecker
{
    protected override void Awake()
    {
        // Set the expected character for this table
        expectedChar = '1';
        
        // Call the parent Awake method
        base.Awake();
    }
}
