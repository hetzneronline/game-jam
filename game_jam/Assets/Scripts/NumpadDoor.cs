using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NumpadDoor : InteractiveObject
{
    [Header("Numpad Settings")]
    [SerializeField] private string correctCode = "1234"; // The correct passcode
    [SerializeField] private int maxCodeLength = 4; // Maximum number of digits
    [SerializeField] private Sprite numpadImage; // The numpad background image
    [SerializeField] private Color backgroundDim = new Color(0, 0, 0, 0.7f); // Background dim color
    [SerializeField] private AudioClip keyPressSound; // Sound when pressing a key
    [SerializeField] private AudioClip correctCodeSound; // Sound when code is correct
    [SerializeField] private AudioClip incorrectCodeSound; // Sound when code is wrong
    
    [Header("Text Settings")]
    [SerializeField] private TMP_FontAsset codeFont; // Custom font for the code display
    [SerializeField] private TMP_FontAsset statusFont; // Custom font for the status messages
    [SerializeField] private Color codeFontColor = Color.green;
    [SerializeField] private Color statusFontColor = Color.white;
    
    [Header("Connected Scene Transition")]
    [SerializeField] private SceneTransition connectedTransition; // Reference to the scene transition component
    [SerializeField] private string accessDeniedMessage = "ACCESS DENIED";
    [SerializeField] private string accessGrantedMessage = "ACCESS GRANTED";
    
    // Door blocking collider
    private Collider2D doorBlocker;
    
    private GameObject numpadCanvas;
    private bool isUsingNumpad = false;
    private string currentCode = "";
    private TextMeshProUGUI codeDisplayText;
    private TextMeshProUGUI statusDisplayText;
    private bool doorUnlocked = false;
    private AudioSource audioSource;
    
    protected override void Start()
    {
        base.Start();
        
        // Change the interaction prompt
        interactionPrompt = "Press E to use keypad";
        
        // Set up audio source
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (keyPressSound != null || correctCodeSound != null || incorrectCodeSound != null))
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Find or create a door blocker collider
        doorBlocker = GetComponent<Collider2D>();
        if (doorBlocker == null)
        {
            // If there's no collider on this object, check for children
            doorBlocker = GetComponentInChildren<Collider2D>();
            if (doorBlocker == null)
            {
                // If still no collider found, add one
                doorBlocker = gameObject.AddComponent<BoxCollider2D>();
                ((BoxCollider2D)doorBlocker).size = new Vector2(1, 1); // Default size
                Debug.Log("Added door blocker collider");
            }
        }
        
        // Make sure it's NOT a trigger (to block the player)
        doorBlocker.isTrigger = false;
        
        // Find the linked scene transition if not set
        if (connectedTransition == null)
        {
            connectedTransition = GetComponent<SceneTransition>();
            if (connectedTransition == null)
            {
                connectedTransition = GetComponentInChildren<SceneTransition>();
                if (connectedTransition == null)
                {
                    Debug.LogWarning("No SceneTransition component found for NumpadDoor. Door will not work properly.");
                }
            }
        }
        
        // We'll use a flag to check if door is unlocked
        doorUnlocked = false;
        
        // Create the numpad UI (but keep it inactive)
        CreateNumpadDisplay();
    }
    
    protected override void Update()
    {
        // When using numpad, capture keyboard input
        if (isUsingNumpad)
        {
            // Check for escape key to close numpad
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseNumpadDisplay();
                return;
            }
            
            // Check for backspace/delete to remove last digit
            if (Input.GetKeyDown(KeyCode.Backspace) && currentCode.Length > 0)
            {
                PlayKeySound();
                currentCode = currentCode.Substring(0, currentCode.Length - 1);
                UpdateCodeDisplay();
                return;
            }
            
            // Check for enter/return key to submit code
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                PlayKeySound();
                CheckCode();
                return;
            }
            
            // Check for number keys (both top row and numpad)
            for (int i = 0; i <= 9; i++)
            {
                if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i))
                {
                    HandleNumberInput(i.ToString());
                    return;
                }
            }
            
            // Skip the base update that handles interaction while using numpad
            return;
        }
        
        // Normal interactive object behavior when not using numpad
        base.Update();
        
        // Handle door unlocked state (allow transition)
        if (doorUnlocked && connectedTransition != null && playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            // Trigger the transition directly
            connectedTransition.SendMessage("TransitionToScene", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    protected override void Interact()
    {
        if (!isUsingNumpad && !doorUnlocked)
        {
            OpenNumpadDisplay();
        }
        else if (doorUnlocked && connectedTransition != null)
        {
            // If door is unlocked, trigger the transition
            connectedTransition.SendMessage("TransitionToScene", SendMessageOptions.DontRequireReceiver);
        }
    }
    
    private void CreateNumpadDisplay()
    {
        // Create a canvas that will display the numpad
        numpadCanvas = new GameObject("NumpadDisplay");
        numpadCanvas.transform.SetParent(null); // Keep it at root level
        
        // Make it persist through scene loads if needed
        DontDestroyOnLoad(numpadCanvas);
        
        // Add canvas component
        Canvas canvas = numpadCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Make sure it's on top of everything
        
        // Add canvas scaler
        CanvasScaler scaler = numpadCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add the background dimming panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(numpadCanvas.transform, false);
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundDim;
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Create the numpad panel using the provided image
        GameObject numpadPanel = new GameObject("NumpadPanel");
        numpadPanel.transform.SetParent(numpadCanvas.transform, false);
        
        Image panelImage = numpadPanel.AddComponent<Image>();
        panelImage.sprite = numpadImage;
        panelImage.preserveAspect = true;
        
        RectTransform panelRect = numpadPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(400, 500); // Size can be adjusted based on your image
        panelRect.anchoredPosition = Vector2.zero;
        
        // Add text to display the entered code
        GameObject codeText = new GameObject("CodeText");
        codeText.transform.SetParent(numpadPanel.transform, false);
        
        codeDisplayText = codeText.AddComponent<TextMeshProUGUI>();
        codeDisplayText.alignment = TextAlignmentOptions.Center;
        codeDisplayText.fontSize = 48; // INCREASED from 36 to 48
        codeDisplayText.color = codeFontColor;
        codeDisplayText.text = "____";
        
        // Apply custom font if provided
        if (codeFont != null)
        {
            codeDisplayText.font = codeFont;
        }
        
        // Position the text field at the top of the numpad image
        // You'll need to adjust these values based on your PNG layout
        RectTransform codeTextRect = codeText.GetComponent<RectTransform>();
        codeTextRect.anchorMin = new Vector2(0.1f, 0.8f);
        codeTextRect.anchorMax = new Vector2(0.9f, 0.9f);
        codeTextRect.offsetMin = Vector2.zero;
        codeTextRect.offsetMax = Vector2.zero;
        
        // Add status display text
        GameObject statusDisplay = new GameObject("StatusDisplay");
        statusDisplay.transform.SetParent(numpadPanel.transform, false);
        
        statusDisplayText = statusDisplay.AddComponent<TextMeshProUGUI>();
        statusDisplayText.alignment = TextAlignmentOptions.Center;
        statusDisplayText.fontSize = 24;
        statusDisplayText.color = statusFontColor;
        statusDisplayText.text = "ENTER CODE";
        
        // Apply custom font if provided
        if (statusFont != null)
        {
            statusDisplayText.font = statusFont;
        }
        
        // Position the status text HIGHER UP
        RectTransform statusRect = statusDisplay.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0.1f, 0.9f); // Changed from 0.7f to 0.9f
        statusRect.anchorMax = new Vector2(0.9f, 0.98f); // Changed from 0.78f to 0.98f
        statusRect.offsetMin = Vector2.zero;
        statusRect.offsetMax = Vector2.zero;
        
        // Removed instructions text as requested
        
        // Start inactive
        numpadCanvas.SetActive(false);
    }
    
    private void HandleNumberInput(string number)
    {
        PlayKeySound();
        
        if (currentCode.Length < maxCodeLength)
        {
            currentCode += number;
            UpdateCodeDisplay();
        }
    }
    
    private void UpdateCodeDisplay()
    {
        // Show the entered code, padding with underscores as needed
        string displayText = currentCode;
        while (displayText.Length < maxCodeLength)
        {
            displayText += "_";
        }
        codeDisplayText.text = displayText;
    }
    
    // Modify the CheckCode method to directly trigger the scene transition

    private void CheckCode()
    {
        if (currentCode == correctCode)
        {
            // Code is correct
            statusDisplayText.text = accessGrantedMessage;
            statusDisplayText.color = Color.white;
            
            // Play correct code sound
            if (audioSource != null && correctCodeSound != null)
            {
                audioSource.PlayOneShot(correctCodeSound);
            }
            
            // Mark door as unlocked and disable the blocker collider
            doorUnlocked = true;
            if (doorBlocker != null)
            {
                doorBlocker.enabled = false;
            }
            
            Debug.Log("Door unlocked, triggering scene transition in 1.5 seconds");
            
            // Automatically transition after a short delay
            Invoke("TriggerSceneTransition", 1.5f);
            
            // Update the interaction prompt
            interactionPrompt = "Press E to open door";
        }
        else
        {
            // Code is incorrect
            statusDisplayText.text = accessDeniedMessage;
            statusDisplayText.color = Color.white;
            
            // Play incorrect code sound
            if (audioSource != null && incorrectCodeSound != null)
            {
                audioSource.PlayOneShot(incorrectCodeSound);
            }
            
            // Clear the code after a short delay
            Invoke("ClearCode", 1.0f);
        }
    }

    // Add this new method to handle the automatic transition
    private void TriggerSceneTransition()
    {
        // Close the numpad first
        CloseNumpadDisplay();
        
        // Check if the transition reference exists
        if (connectedTransition != null)
        {
            Debug.Log("Triggering scene transition directly");
            
            // Use SendMessage as we do elsewhere in this class
            connectedTransition.SendMessage("TransitionToScene", SendMessageOptions.DontRequireReceiver);
        }
        else
        {
            Debug.LogError("No scene transition component found - cannot transition to next scene");
        }
    }
    
    private void ClearCode()
    {
        currentCode = "";
        UpdateCodeDisplay();
        statusDisplayText.text = "ENTER CODE";
        statusDisplayText.color = statusFontColor;
    }
    
    private void PlayKeySound()
    {
        if (audioSource != null && keyPressSound != null)
        {
            audioSource.PlayOneShot(keyPressSound);
        }
    }
    
    private void OpenNumpadDisplay()
    {
        // Reset the code
        currentCode = "";
        UpdateCodeDisplay();
        statusDisplayText.text = "ENTER CODE";
        statusDisplayText.color = statusFontColor;
        
        // Show the numpad
        numpadCanvas.SetActive(true);
        isUsingNumpad = true;
        
        // Hide interaction prompt
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.HidePrompt();
        }
        
        // Pause the game while using numpad
        Time.timeScale = 0;
        
        Debug.Log("Opened numpad");
    }
    
    public void CloseNumpadDisplay()
    {
        numpadCanvas.SetActive(false);
        isUsingNumpad = false;
        
        // Resume game
        Time.timeScale = 1;
        
        Debug.Log("Closed numpad");
    }
    
    private void OnDestroy()
    {
        // Clean up the canvas when this object is destroyed
        if (numpadCanvas != null)
        {
            Destroy(numpadCanvas);
        }
        
        // Make sure time scale is reset if object is destroyed while numpad is open
        if (isUsingNumpad)
        {
            Time.timeScale = 1;
        }
    }
}