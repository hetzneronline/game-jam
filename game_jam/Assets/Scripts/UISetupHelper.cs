using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// This class will help with setting up the UI for interactions in the scene
public class UISetupHelper : MonoBehaviour
{
    [SerializeField] private bool createUIAtStart = true;
    [SerializeField] private Font defaultFont;
    
    private void Start()
    {
        if (createUIAtStart && UIPromptController.Instance == null)
        {
            SetupInteractionUI();
        }
    }
    
    public void SetupInteractionUI()
    {
        // Check if UI already exists
        if (UIPromptController.Instance != null)
        {
            Debug.Log("UI Prompt system is already set up");
            return;
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("InteractionCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Check if there's already an EventSystem in the scene
        // If not, we'll need to create one
        SetupEventSystem();
        
        // Create prompt panel
        GameObject promptPanel = new GameObject("PromptPanel");
        promptPanel.transform.SetParent(canvasObj.transform, false);
        
        // Add Image background
        Image panelImage = promptPanel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.7f);
        
        // Set panel size and position (bottom center)
        RectTransform panelRect = promptPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.sizeDelta = new Vector2(400, 50);
        panelRect.anchoredPosition = new Vector2(0, 100);
        
        // Create prompt text
        GameObject textObj = new GameObject("PromptText");
        textObj.transform.SetParent(promptPanel.transform, false);
        
        // Set up text component
        Text promptText = textObj.AddComponent<Text>();
        promptText.text = "Press E to interact";
        promptText.alignment = TextAnchor.MiddleCenter;
        promptText.color = Color.white;
        
        if (defaultFont != null)
        {
            promptText.font = defaultFont;
        }
        
        // Set text size and position
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = Vector2.zero;
        textRect.offsetMin = new Vector2(10, 5);
        textRect.offsetMax = new Vector2(-10, -5);
        
        // Add UIPromptController component
        UIPromptController controller = canvasObj.AddComponent<UIPromptController>();
        controller.promptPanel = promptPanel;
        controller.promptText = promptText;
          // Hide panel at start
        promptPanel.SetActive(false);
        
        Debug.Log("Interaction UI system created successfully");
    }
      private void SetupEventSystem()
    {
        // Check if EventSystem already exists in scene
        EventSystem existingSystem = FindAnyObjectByType<EventSystem>();
        
        if (existingSystem != null)
        {
            Debug.Log("Using existing EventSystem in scene");
            return;
        }
        
        // Create a new EventSystem if needed
        GameObject eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<EventSystem>();
        eventSystemObj.AddComponent<StandaloneInputModule>();
        
        Debug.Log("Created new EventSystem");
    }
}
