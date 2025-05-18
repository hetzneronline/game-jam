using UnityEngine;
using UnityEngine.UI;

public class ReadableLetter : InteractiveObject
{
    [Header("Letter Settings")]
    [SerializeField] private Sprite letterContent; // The image to show when reading
    [SerializeField] private float displayScale = 0.8f; // How large to display the letter (portion of screen)
    [SerializeField] private Color backgroundDim = new Color(0, 0, 0, 0.7f); // Background dim color
    
    private GameObject letterDisplayCanvas;
    private bool isReading = false;
    
    protected override void Start()
    {
        base.Start();
        
        // Change the interaction prompt
        interactionPrompt = "Press E to read";
        
        // Create the letter display canvas (but keep it inactive)
        CreateLetterDisplay();
    }
    
    protected override void Update()
    {
        // When reading, check for escape key to close the letter
        if (isReading)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                CloseLetterDisplay();
            }
            
            // Skip the base update that handles interaction while reading
            return;
        }
        
        // Normal interactive object behavior when not reading
        base.Update();
    }
    
    protected override void Interact()
    {
        if (!isReading)
        {
            OpenLetterDisplay();
        }
    }
    
    private void CreateLetterDisplay()
    {
        // Create a canvas that will display the letter
        letterDisplayCanvas = new GameObject("LetterDisplay");
        letterDisplayCanvas.transform.SetParent(null); // Keep it at root level
        
        // Make it persist through scene loads if needed
        DontDestroyOnLoad(letterDisplayCanvas);
        
        // Add canvas component
        Canvas canvas = letterDisplayCanvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Make sure it's on top of everything
        
        // Add canvas scaler
        CanvasScaler scaler = letterDisplayCanvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Make sure we have a GraphicRaycaster for the button to work
        letterDisplayCanvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // Add the background dimming panel
        GameObject background = new GameObject("Background");
        background.transform.SetParent(letterDisplayCanvas.transform, false); // false is important for correct positioning
        
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = backgroundDim;
        
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Add the letter image
        GameObject letterObj = new GameObject("LetterImage");
        letterObj.transform.SetParent(letterDisplayCanvas.transform, false); // false ensures it inherits correct position
        
        Image letterImage = letterObj.AddComponent<Image>();
        letterImage.sprite = letterContent;
        letterImage.preserveAspect = true;
        
        // Fix the positioning of the letter
        RectTransform letterRect = letterObj.GetComponent<RectTransform>();
        letterRect.anchorMin = new Vector2(0.5f, 0.5f); // Center anchor
        letterRect.anchorMax = new Vector2(0.5f, 0.5f); // Center anchor
        letterRect.pivot = new Vector2(0.5f, 0.5f); // Center pivot
        letterRect.anchoredPosition = Vector2.zero; // Position at center of screen
        
        // Set the size based on the display scale
        float width = Screen.width * displayScale;
        float height = Screen.height * displayScale;
        letterRect.sizeDelta = new Vector2(width, height);
        
        // Add a close button (optional)
        GameObject closeBtn = new GameObject("CloseButton");
        closeBtn.transform.SetParent(letterDisplayCanvas.transform, false); // false for correct positioning
        
        Button button = closeBtn.AddComponent<Button>();
        Image btnImage = closeBtn.AddComponent<Image>();
        btnImage.color = new Color(1, 1, 1, 0.8f);
        
        RectTransform btnRect = closeBtn.GetComponent<RectTransform>();
        btnRect.anchorMin = new Vector2(1, 1); // Top right corner
        btnRect.anchorMax = new Vector2(1, 1);
        btnRect.pivot = new Vector2(1, 1);
        btnRect.sizeDelta = new Vector2(50, 50);
        btnRect.anchoredPosition = new Vector2(-20, -20); // Offset from corner
        
        // Add X text to close button
        GameObject btnText = new GameObject("Text");
        btnText.transform.SetParent(closeBtn.transform, false);
        
        TMPro.TextMeshProUGUI text = btnText.AddComponent<TMPro.TextMeshProUGUI>();
        text.text = "X";
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.fontSize = 24;
        text.color = Color.black;
        
        RectTransform textRect = btnText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        // Add click handler to close the letter
        button.onClick.AddListener(CloseLetterDisplay);
        
        // Add "Press ESC to close" text at bottom
        GameObject escText = new GameObject("EscapeText");
        escText.transform.SetParent(letterDisplayCanvas.transform, false);
        
        TMPro.TextMeshProUGUI escapeText = escText.AddComponent<TMPro.TextMeshProUGUI>();
        escapeText.text = "Press ESC to close";
        escapeText.alignment = TMPro.TextAlignmentOptions.Center;
        escapeText.fontSize = 20;
        escapeText.color = Color.white;
        
        RectTransform escRect = escText.GetComponent<RectTransform>();
        escRect.anchorMin = new Vector2(0.5f, 0);
        escRect.anchorMax = new Vector2(0.5f, 0);
        escRect.pivot = new Vector2(0.5f, 0);
        escRect.sizeDelta = new Vector2(300, 30);
        escRect.anchoredPosition = new Vector2(0, 30);
        
        // Start inactive
        letterDisplayCanvas.SetActive(false);
    }
    
    private void OpenLetterDisplay()
    {
        // Show the letter display
        letterDisplayCanvas.SetActive(true);
        isReading = true;
        
        // Hide interaction prompt
        if (UIPromptController.Instance != null)
        {
            UIPromptController.Instance.HidePrompt();
        }
        
        // Optionally pause the game
        Time.timeScale = 0; // Comment this out if you don't want to pause
        
        Debug.Log("Player opened letter to read");
    }
    
    public void CloseLetterDisplay()
    {
        letterDisplayCanvas.SetActive(false);
        isReading = false;
        
        // Resume game if paused
        Time.timeScale = 1;
        
        Debug.Log("Player closed letter");
    }
    
    private void OnDestroy()
    {
        // Clean up the canvas when this object is destroyed
        if (letterDisplayCanvas != null)
        {
            Destroy(letterDisplayCanvas);
        }
        
        // Make sure time scale is reset if object is destroyed while letter is open
        if (isReading)
        {
            Time.timeScale = 1;
        }
    }
}