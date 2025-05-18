using UnityEngine;
using UnityEngine.UI;

// This class manages the interaction prompt UI
public class UIPromptController : MonoBehaviour
{
    public GameObject promptPanel;
    public Text promptText;
    
    // Singleton instance
    private static UIPromptController _instance;
    
    public static UIPromptController Instance
    {
        get { return _instance; }
    }
    
    private void Awake()
    {
        // Set up singleton
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        
        // Hide prompt at start
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
    
    // Show a prompt with the given message
    public void ShowPrompt(string message)
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(true);
            
            if (promptText != null)
            {
                promptText.text = message;
            }
        }
    }
    
    // Hide the prompt
    public void HidePrompt()
    {
        if (promptPanel != null)
        {
            promptPanel.SetActive(false);
        }
    }
}
