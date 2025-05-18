using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EchoVisualizer : MonoBehaviour
{
    [Header("Echo Animation Settings")]
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 0.8f;
    [SerializeField] private float pulseDuration = 1.5f;
    [SerializeField] private bool startActive = false;
    [SerializeField] private bool randomizeStartPhase = true;
    
    [Header("Size Animation")]
    [SerializeField] private bool animateSize = true;
    [SerializeField] private float minScale = 0.9f;
    [SerializeField] private float maxScale = 1.1f;
    
    [Header("LLM Speaking Detection")]
    [SerializeField] private float activeTimeAfterMessage = 3f; // How long to stay active after a message
    [SerializeField] private string apiCheckEndpoint = "http://127.0.0.1:5001/status";
    [SerializeField] private float checkInterval = 0.5f; // How often to check if LLM is speaking
    
    private SpriteRenderer spriteRenderer;
    private Image imageComponent;
    private CanvasGroup canvasGroup;
    private float currentAlpha;
    private float pulseTimer;
    private bool isActive = false;
    private float lastMessageTime = 0;
    private Vector3 originalScale;
    private Coroutine checkSpeakingCoroutine;
    
    private void Awake()
    {
        // Get either SpriteRenderer (for world space) or Image (for UI)
        spriteRenderer = GetComponent<SpriteRenderer>();
        imageComponent = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (spriteRenderer == null && imageComponent == null && canvasGroup == null)
        {
            Debug.LogWarning("EchoVisualizer needs either a SpriteRenderer, Image component, or CanvasGroup to work!");
        }
        
        // Store original scale for size animation
        originalScale = transform.localScale;
        
        // Set initial state
        isActive = startActive;
        
        // Randomize starting phase if needed
        if (randomizeStartPhase)
        {
            pulseTimer = Random.Range(0f, pulseDuration);
        }
        
        // In the Awake or Start method, reduce the activeTimeAfterMessage value:
        activeTimeAfterMessage = 0.5f; // Much shorter fade-out (0.5 seconds)
    }

    private void Start()
    {
        // Initialize alpha based on active state
        SetAlpha(isActive ? maxAlpha : 0f);
        
        // Start the coroutine to check if LLM is speaking
        StartCheckingSpeakingStatus();
    }
    
    private void Update()
    {
        if (!isActive)
        {
            // Gradually fade out when not active
            if (currentAlpha > 0)
            {
                currentAlpha = Mathf.Max(0, currentAlpha - Time.deltaTime * 2);
                SetAlpha(currentAlpha);
            }
            return;
        }
        
        // Animate when active
        pulseTimer += Time.deltaTime;
        if (pulseTimer > pulseDuration)
        {
            pulseTimer = 0f;
        }
        
        // Calculate sine wave for smooth pulsing (0-1)
        float pulse = Mathf.Sin(pulseTimer / pulseDuration * Mathf.PI * 2) * 0.5f + 0.5f;
        
        // Apply alpha pulsing
        currentAlpha = Mathf.Lerp(minAlpha, maxAlpha, pulse);
        SetAlpha(currentAlpha);
        
        // Apply size pulsing if enabled
        if (animateSize)
        {
            float scale = Mathf.Lerp(minScale, maxScale, pulse);
            transform.localScale = originalScale * scale;
        }
        
        // Check if we should deactivate based on time since last message
        if (Time.time > lastMessageTime + activeTimeAfterMessage)
        {
            SetActive(false);
        }
    }
    
    // Set the alpha value using the appropriate component
    private void SetAlpha(float alpha)
    {
        currentAlpha = alpha;
        
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }
        
        if (imageComponent != null)
        {
            Color color = imageComponent.color;
            color.a = alpha;
            imageComponent.color = color;
        }
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = alpha;
        }
    }
    
    // Public method to set the active state
    public void SetActive(bool active)
    {
        isActive = active;
        
        if (active)
        {
            lastMessageTime = Time.time;
        }
    }
    
    // Call this when the LLM starts speaking
    public void OnLLMStartedSpeaking()
    {
        SetActive(true);
        lastMessageTime = Time.time;
    }
    
    // Call this when the LLM stops speaking
    public void OnLLMStoppedSpeaking()
    {
        // Set isActive to false IMMEDIATELY instead of waiting for the timer
        SetActive(false);
        
        // You can remove this line if you want an immediate stop with no fade-out
        // lastMessageTime = Time.time; // Set the time so it will fade out after the delay
    }
    
    private void StartCheckingSpeakingStatus()
    {
        // Stop existing coroutine if any
        if (checkSpeakingCoroutine != null)
        {
            StopCoroutine(checkSpeakingCoroutine);
        }
        
        // Start new coroutine
        checkSpeakingCoroutine = StartCoroutine(CheckSpeakingStatus());
    }
    
    private IEnumerator CheckSpeakingStatus()
    {
        // Wait for web requests module to be available
        yield return new WaitForEndOfFrame();
        
        while (true)
        {
            UnityEngine.Networking.UnityWebRequest request = null;
            bool requestFailed = false;
            
            try
            {
                request = UnityEngine.Networking.UnityWebRequest.Get(apiCheckEndpoint);
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to create web request: {e.Message}");
                requestFailed = true;
            }
            
            // Handle failed request outside the catch block
            if (requestFailed)
            {
                yield return new WaitForSeconds(checkInterval);
                continue;
            }
            
            // Normal flow continues here
            UnityEngine.Networking.UnityWebRequestAsyncOperation asyncOp = request.SendWebRequest();
            yield return asyncOp;
            
            // Process response
            try
            {
                if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    Debug.Log($"Status response: {response}"); // Add this debug line
                    
                    // More robust parsing
                    if (response.Contains("\"speaking\":true"))
                    {
                        OnLLMStartedSpeaking();
                        Debug.Log("LLM started speaking");
                    }
                    else if (response.Contains("\"speaking\":false"))
                    {
                        OnLLMStoppedSpeaking();
                        Debug.Log("LLM stopped speaking");
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"Failed to process response: {e.Message}");
            }
            finally
            {
                if (request != null)
                    request.Dispose();
            }
            
            yield return new WaitForSeconds(checkInterval);
        }
    }
    
    // Manual testing methods
    [ContextMenu("Start Speaking")]
    public void TestStartSpeaking()
    {
        OnLLMStartedSpeaking();
    }
    
    [ContextMenu("Stop Speaking")]
    public void TestStopSpeaking()
    {
        OnLLMStoppedSpeaking();
    }
    
    private void OnDestroy()
    {
        if (checkSpeakingCoroutine != null)
        {
            StopCoroutine(checkSpeakingCoroutine);
        }
    }

    private void OnEnable()
    {
        // Register with LLMManager
        if (LLMManager.Instance != null)
        {
            LLMManager.Instance.RegisterEchoVisualizer(this);
        }
    }

    private void OnDisable()
    {
        // Unregister from LLMManager
        if (LLMManager.Instance != null)
        {
            LLMManager.Instance.UnregisterEchoVisualizer(this);
        }
    }
}