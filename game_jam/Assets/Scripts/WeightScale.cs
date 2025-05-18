using UnityEngine;
using TMPro;

/// <summary>
/// A script that detects objects placed on a platform and displays their weight value.
/// The weight is determined by the last character of the object's name (e.g., "Weight3" has weight 3).
/// </summary>
public class WeightScale : MonoBehaviour
{
    [Header("Scale Settings")]
    [SerializeField] private TMP_Text weightDisplay; // TextMeshPro component to display the weight
    [SerializeField] private string defaultText = "0"; // Text to show when nothing is on the scale
    [SerializeField] private bool hideWhenEmpty = true; // Hide the text when nothing is on the scale
    
    [Header("Font Settings")]
    [SerializeField] private TMP_FontAsset customFont; // Custom font to use for the weight display
    [SerializeField] private bool useCustomFont = false; // Whether to use the custom font
    
    // Reference to the object platform this script is attached to
    private ObjectPlatform platform;
      private void Awake()
    {
        // Get the ObjectPlatform component
        platform = GetComponent<ObjectPlatform>();
        if (platform == null)
        {
            Debug.LogError("WeightScale script requires an ObjectPlatform component on the same GameObject.");
            enabled = false;
            return;
        }
        
        // Try to find the weight_watcher TMP component if not assigned
        if (weightDisplay == null)
        {
            weightDisplay = GameObject.Find("weight_watcher")?.GetComponent<TMP_Text>();
            if (weightDisplay == null)
            {
                Debug.LogWarning("Weight display (weight_watcher) not found. Weight will not be displayed.");
            }
        }
        
        // Apply custom font if specified
        if (useCustomFont && customFont != null && weightDisplay != null)
        {
            weightDisplay.font = customFont;
        }
        
        // Initialize display
        UpdateWeightDisplay();
    }
    
    private void Update()
    {
        UpdateWeightDisplay();
    }
      /// <summary>
    /// Updates the weight display based on the current object on the platform
    /// </summary>
    private void UpdateWeightDisplay()
    {
        if (weightDisplay == null)
            return;
            
        // Get the current object on the platform
        CarriableObject obj = platform.GetPlacedObject();
        
        if (obj == null)
        {
            // No object on the scale
            if (hideWhenEmpty)
            {
                weightDisplay.gameObject.SetActive(false);
            }
            else
            {
                weightDisplay.gameObject.SetActive(true);
                weightDisplay.text = defaultText;
            }
            return;
        }
        
        // Make sure the display is active when we have an object
        weightDisplay.gameObject.SetActive(true);
        
        // Get the name of the object
        string objName = obj.name;
        
        // Extract the last character
        if (objName.Length > 0)
        {
            string lastChar = objName[objName.Length - 1].ToString();
            
            // Check if the last character is a number
            int weight;
            if (int.TryParse(lastChar, out weight))
            {
                // Show the weight value
                weightDisplay.text = weight.ToString();
            }
            else
            {
                // Last character isn't a number, show 0 or some default
                weightDisplay.text = defaultText;
            }
        }
        else
        {
            // Empty name, show default
            weightDisplay.text = defaultText;
        }
    }
    
    /// <summary>
    /// Get the current weight value (useful for other scripts)
    /// </summary>
    /// <returns>The current weight value or 0 if nothing on the scale</returns>
    public int GetCurrentWeight()
    {
        CarriableObject obj = platform.GetPlacedObject();
        
        if (obj == null)
            return 0;
            
        string objName = obj.name;
        
        if (objName.Length > 0)
        {
            string lastChar = objName[objName.Length - 1].ToString();
            
            int weight;
            if (int.TryParse(lastChar, out weight))
            {
                return weight;
            }
        }
        
        return 0;
    }
}
