using UnityEngine;

/// <summary>
/// Helper class to easily create weight objects with appropriate naming
/// </summary>
public class WeightObjectSetup : MonoBehaviour
{
    [Header("Weight Settings")]
    [SerializeField] private int weightValue = 1; // Weight value from 1-9
    [SerializeField] private Color weightColor = Color.gray;
    [SerializeField] private Vector3 weightSize = new Vector3(0.5f, 0.2f, 0.5f);
    [SerializeField] private bool showWeightNumber = true;
    
    private void Start()
    {
        // Make sure the object's name ends with the weight value
        if (!gameObject.name.EndsWith(weightValue.ToString()))
        {
            gameObject.name = "Weight" + weightValue;
        }
        
        // Check if the object has a CarriableObject component
        CarriableObject carriable = GetComponent<CarriableObject>();
        if (carriable == null)
        {
            carriable = gameObject.AddComponent<CarriableObject>();
        }
        
        // Check if the object has a renderer
        Renderer objRenderer = GetComponent<Renderer>();
        if (objRenderer != null)
        {
            objRenderer.material.color = weightColor;
        }
        
        // Check if the object has a sprite renderer
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = weightColor;
        }
        
        // Apply the size
        transform.localScale = weightSize;
        
        // Add a text label showing the weight, if desired
        if (showWeightNumber)
        {
            AddWeightLabel();
        }
    }
    
    /// <summary>
    /// Add a visible label above the weight showing its value
    /// </summary>
    private void AddWeightLabel()
    {
        // Create a child object for the weight label
        GameObject labelObj = new GameObject("WeightLabel");
        labelObj.transform.SetParent(transform);
        labelObj.transform.localPosition = new Vector3(0, 0.5f, 0);
        
        // Make the label face the camera
        labelObj.AddComponent<Billboard>();
        
        // Add a TextMesh component
        TextMesh textMesh = labelObj.AddComponent<TextMesh>();
        textMesh.text = weightValue.ToString();
        textMesh.fontSize = 48;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.color = Color.black;
        
        // Add a MeshRenderer for the text
        MeshRenderer meshRenderer = labelObj.GetComponent<MeshRenderer>();
        if (meshRenderer != null)
        {
            meshRenderer.sortingOrder = 1; // Make sure it's visible over other elements
        }
    }
    
    /// <summary>
    /// Create a new weight object with the specified weight value
    /// </summary>
    public static GameObject CreateWeight(Vector3 position, int weightValue)
    {
        // Clamp the weight value between 1 and 9
        weightValue = Mathf.Clamp(weightValue, 1, 9);
        
        // Create a new cube
        GameObject weightObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        weightObj.name = "Weight" + weightValue;
        weightObj.transform.position = position;
        weightObj.transform.localScale = new Vector3(0.5f, 0.2f, 0.5f);
        
        // Add the carriable component
        CarriableObject carriable = weightObj.AddComponent<CarriableObject>();
        
        // Add the setup helper
        WeightObjectSetup setup = weightObj.AddComponent<WeightObjectSetup>();
        setup.weightValue = weightValue;
        
        // Color based on weight value (darker = heavier)
        float brightness = 1f - ((weightValue - 1f) / 10f);
        setup.weightColor = new Color(brightness, brightness, brightness);
        
        return weightObj;
    }
    
    // Menu item to create a weight in the scene
    [ContextMenu("Create Weight Object")]
    public void CreateWeightAtPosition()
    {
        GameObject weight = CreateWeight(transform.position, weightValue);
        weight.transform.SetParent(transform.parent);
        
        // Destroy this setup object
        DestroyImmediate(gameObject);
    }
}
