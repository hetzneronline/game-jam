using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class FadeManager : MonoBehaviour
{
    public static FadeManager Instance { get; private set; }
    
    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Color fadeColor = Color.black;
    
    private Image fadeImage;
    private Canvas fadeCanvas;
    private bool isFading = false;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeFadeScreen();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeFadeScreen()
    {
        // Create Canvas for the fade effect
        fadeCanvas = gameObject.AddComponent<Canvas>();
        fadeCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        fadeCanvas.sortingOrder = 999; // Make sure it renders on top of everything
        
        // Add a CanvasScaler for proper scaling
        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add Canvas Group to control alpha
        CanvasGroup canvasGroup = gameObject.AddComponent<CanvasGroup>();
        
        // Create a full-screen black image
        GameObject imageObj = new GameObject("FadeImage");
        imageObj.transform.SetParent(transform);
        fadeImage = imageObj.AddComponent<Image>();
        fadeImage.color = fadeColor;
        
        // Set the image to cover the entire screen
        RectTransform rect = imageObj.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
        rect.anchoredPosition = Vector2.zero;
        
        // Start invisible
        fadeImage.enabled = true;
        fadeImage.canvasRenderer.SetAlpha(0f);
        
        Debug.Log("Fade screen initialized");
    }
    
    /// <summary>
    /// Fade to black, load a new scene, then fade back in
    /// </summary>
    public void FadeAndLoadScene(string sceneName, string spawnPointName = "SpawnPoint")
    {
        if (isFading)
        {
            Debug.LogWarning("Already fading, ignoring scene load request");
            return;
        }
        
        // Stop any existing coroutines just to be safe
        StopAllCoroutines();
        
        // Start the new transition
        StartCoroutine(FadeAndLoadSceneCoroutine(sceneName, spawnPointName));
    }
    
    private IEnumerator FadeAndLoadSceneCoroutine(string sceneName, string spawnPointName)
    {
        isFading = true;
        
        // Check if we're already in the target scene
        if (SceneManager.GetActiveScene().name == sceneName)
        {
            Debug.LogWarning($"Already in scene {sceneName}. Skipping scene load.");
            
            // Still fade out/in for visual consistency
            fadeImage.canvasRenderer.SetAlpha(0f);
            fadeImage.CrossFadeAlpha(1f, fadeDuration * 0.5f, true);
            yield return new WaitForSeconds(fadeDuration * 0.5f);
            fadeImage.CrossFadeAlpha(0f, fadeDuration * 0.5f, true);
            
            // Update spawn point anyway
            if (GameManager.Instance != null)
            {
                GameManager.Instance.targetSpawnPoint = spawnPointName;
                GameManager.Instance.RepositionPlayerAtSpawnPoint();
            }
            
            isFading = false;
            yield break;
        }
        
        // Store player data before transition
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SavePlayerState();
            GameManager.Instance.targetSpawnPoint = spawnPointName;
            Debug.Log($"Set target spawn point to '{spawnPointName}' for scene '{sceneName}'");
        }
        
        // Fade to black
        fadeImage.canvasRenderer.SetAlpha(0f);
        fadeImage.CrossFadeAlpha(1f, fadeDuration, true);
        
        // Wait for fade out to complete
        yield return new WaitForSeconds(fadeDuration);
        
        // Load the scene
        Debug.Log($"Loading scene: {sceneName}");
        Debug.Log($"Currently in scene: {SceneManager.GetActiveScene().name}");
        
        // Only load if we're not already in that scene
        if (SceneManager.GetActiveScene().name != sceneName)
        {
            SceneManager.LoadScene(sceneName);
        }
        
        // Wait a frame for the scene to load
        yield return null;
        
        // Fade back in
        fadeImage.canvasRenderer.SetAlpha(1f);
        fadeImage.CrossFadeAlpha(0f, fadeDuration, true);
        
        // Wait for fade in to complete
        yield return new WaitForSeconds(fadeDuration);
        
        isFading = false;
    }
    
    /// <summary>
    /// Simple fade in effect that can be used at the start of a scene
    /// </summary>
    public void FadeIn()
    {
        if (!isFading)
        {
            fadeImage.canvasRenderer.SetAlpha(1f);
            fadeImage.CrossFadeAlpha(0f, fadeDuration, true);
        }
    }
    
    /// <summary>
    /// Simple fade out effect
    /// </summary>
    public void FadeOut()
    {
        if (!isFading)
        {
            fadeImage.canvasRenderer.SetAlpha(0f);
            fadeImage.CrossFadeAlpha(1f, fadeDuration, true);
        }
    }
}