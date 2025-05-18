using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Text;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;
using System.Linq;

public class llm_integration : MonoBehaviour
{
    // UI Components
    [SerializeField] private TMP_InputField userInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private TextMeshProUGUI responseText;
    
    [SerializeField] private string pythonApiUrl = "http://127.0.0.1:5001/query";
    
    private bool isProcessing = false;
    private string logFilePath;
    
    private string[] conversationBuffers = new string[] 
    {
        "mhhh", "mh...", "mhhh...", "uhm", "hmmm", "huh", "uhm...", "huh...", 
        "mhhh?", "mh?", "hmmm?", "huh?", "uhm?", "ahm", "ahhm", "ahhm...", "...", "...", "...", "...", "...", "...", "...", "...", "...",
        "ahm...", "mhmm", "mhmm...", "mhmm?", "mhmmm", "mhmmm...", "mhmmm?", "...", "...", "...", "...", "...", "...", "...", "...", "...",
        "hmmm", "hmmm...", "hmmm?", "hmmmm", "hmmmm...", "hmmmm?", "...", "...", "...", "...", "...", "...", "...", "...", "...",
    };

    void Start()
    {
        // Setup logging
        string directory = Path.Combine(Application.persistentDataPath, "Logs");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        logFilePath = Path.Combine(directory, "llm_integration_log.txt");
        LogToFile("=== LLM Integration Started ===");
        
        // Log UI component status
        LogMessage($"InputField exists: {userInputField != null}");
        LogMessage($"SendButton exists: {sendButton != null}");
        LogMessage($"ResponseText exists: {responseText != null}");
        
        // Setup button click listener
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(SendMessage);
            LogMessage("Button click listener configured");
        }
        else
        {
            LogMessage("WARNING: Send button is null!", LogType.Warning);
        }
        
        // Setup input field to submit on Enter key
        if (userInputField != null)
        {
            // Add character validation to prevent special characters
            userInputField.onValidateInput += ValidateCharacterInput;
            userInputField.onEndEdit.AddListener(OnEndEdit);
            LogMessage("Input field end edit listener configured");
            LogMessage("Input field validation configured");
        }
        else
        {
            LogMessage("WARNING: User input field is null!", LogType.Warning);
        }
        
        // Check response text
        if (responseText == null)
        {
            LogMessage("WARNING: Response text field is null!", LogType.Warning);
        }
        
        LogMessage("LLM Integration initialized");
    }
    
    void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        // Find UI references in the new scene
        if (userInputField == null)
            userInputField = FindObjectsByType<TMP_InputField>(FindObjectsSortMode.None).FirstOrDefault();
        
        if (sendButton == null)
            sendButton = FindObjectsByType<Button>(FindObjectsSortMode.None).FirstOrDefault();
        
        // Log all text elements to see what's available
        var allTextElements = FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None);
        LogMessage($"Found {allTextElements.Length} TextMeshProUGUI elements:");
        foreach (var text in allTextElements) {
            LogMessage($"Text element: {text.name}");
        }
        
        if (responseText == null)
            responseText = allTextElements.FirstOrDefault(text => text.name.Contains("Response"));
                
        // Reconnect event listeners
        if (sendButton != null) {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(SendMessage);
        }
        
        if (userInputField != null) {
            userInputField.onEndEdit.RemoveAllListeners();
            userInputField.onEndEdit.AddListener(OnEndEdit);
        }
        
        LogMessage("Scene changed - reconnected UI references");
    }
    
    private void OnEndEdit(string text)
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            LogMessage("Enter key detected, sending message");
            SendMessage();
        }
    }
    
    public void SendMessage()
    {
        if (isProcessing)
        {
            LogMessage("Message processing already in progress, ignoring request");
            return;
        }
        if (string.IsNullOrEmpty(userInputField?.text) || userInputField.text.Trim().Length == 0)
        {
            LogMessage("Empty message, ignoring request");
            return;
        }
            
        string userMessage = userInputField.text;
        // Filter special characters from user message before sending
        userMessage = FilterSpecialCharacters(userMessage);
        LogMessage($"Processing user message: {userMessage}");
        
        // Immediately show a random thinking message
        if (responseText != null)
        {
            string bufferText = GetRandomBufferText();
            responseText.text = bufferText;
            LogMessage($"Showing buffer text: {bufferText}");
        }
        
        StartCoroutine(QueryLLM(userMessage));
        
        // Clear input field after sending
        userInputField.text = "";
    }
    
    private IEnumerator QueryLLM(string message)
    {
        isProcessing = true;
        LogMessage("API call started");
        
        // Create JSON payload
        string jsonPayload = JsonUtility.ToJson(new RequestData { message = message });
        LogMessage($"JSON payload created: {jsonPayload}");
        
        // Create web request
        using (UnityWebRequest request = new UnityWebRequest(pythonApiUrl, "POST"))
        {
            LogMessage($"Sending request to: {pythonApiUrl}");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            // Send the request
            LogMessage("Sending web request...");
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string responseJson = request.downloadHandler.text;
                LogMessage($"Received response: {responseJson}");
                
                try
                {
                    ResponseData responseData = JsonUtility.FromJson<ResponseData>(responseJson);
                    LogMessage($"Parsed response: {responseData.response}");
                    
                    // Update UI text gradually
                    if (responseText != null)
                    {
                        // Stop any previous display coroutines
                        StopAllCoroutines();
                        
                        // Start a new coroutine for this specific display
                        StartCoroutine(DisplayResponseGradually(responseData.response));
                    }
                    else
                    {
                        LogMessage("WARNING: ResponseText is null, can't update UI", LogType.Warning);
                    }
                }
                catch (System.Exception e)
                {
                    LogMessage($"ERROR parsing response JSON: {e.Message}", LogType.Error);
                }
            }
            else
            {
                LogMessage($"ERROR: Request failed with status {request.responseCode}", LogType.Error);
                LogMessage($"Error details: {request.error}", LogType.Error);
                LogMessage($"Response body: {request.downloadHandler.text}", LogType.Error);
            }
        }
        
        isProcessing = false;
        LogMessage("API call completed");
    }
    
    private IEnumerator DisplayResponseGradually(string fullResponse, float wordDelay = 0.1f)
    {
        if (responseText == null)
        {
            LogMessage("Cannot display response gradually - responseText is null", LogType.Warning);
            yield break;
        }
        
        // Filter special characters from the response
        fullResponse = FilterSpecialCharacters(fullResponse);
        
        // Split response into words
        string[] words = fullResponse.Split(' ');
        string currentText = "";
        
        // Display words one by one
        for (int i = 0; i < words.Length; i++)
        {
            // Add the next word
            currentText += words[i] + " ";
            responseText.text = currentText;
            
            // Wait for the specified delay
            yield return new WaitForSeconds(wordDelay);
        }
        
        LogMessage("Completed displaying response gradually");
    
    }
    
    // Logging functions
    private void LogMessage(string message, LogType logType = LogType.Log)
    {
        // Log to console
        switch (logType)
        {
            case LogType.Error:
                Debug.LogError($"[LLM] {message}");
                break;
            case LogType.Warning:
                Debug.LogWarning($"[LLM] {message}");
                break;
            default:
                Debug.Log($"[LLM] {message}");
                break;
        }
        
        // Also log to file
        LogToFile($"[{System.DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{logType}] {message}");
    }
    
    private void LogToFile(string message)
    {
        try
        {
            File.AppendAllText(logFilePath, message + System.Environment.NewLine);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to write to log file: {e.Message}");
        }
    }
    
    private string GetRandomBufferText()
    {
        int randomIndex = UnityEngine.Random.Range(0, conversationBuffers.Length);
        return conversationBuffers[randomIndex];
    }
    
    // Filter unwanted characters
    private string FilterSpecialCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        // Define characters to remove
        char[] charsToRemove = new char[] { 
            '&', '%', '$', '§', '(', ')', '/', '\\', '*', 
            '<', '>', '#', '@', '{', '}', '[', ']', '^', 
            '°', '=', '+', '~', '|', '-', '_' 
        };
        
        // Replace unwanted characters with empty string
        string result = text;
        foreach (char c in charsToRemove)
        {
            result = result.Replace(c.ToString(), "");
        }
        
        return result;
    }
    
    // Character validation callback for input field
    private char ValidateCharacterInput(string text, int charIndex, char addedChar)
    {
        // Define characters to block
        char[] disallowedChars = new char[] { 
            '&', '%', '$', '§', '(', ')', '/', '\\', '*', 
            '<', '>', '#', '@', '{', '}', '[', ']', '^', 
            '°', '=', '+', '~', '|', '-', '_', 
            'ä', 'ö', 'ü', 'Ä', 'Ö', 'Ü', 'ß',
            '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'
        };
        
        // Check if the character is in the disallowed list
        foreach (char c in disallowedChars)
        {
            if (addedChar == c)
            {
                // Return '\0' (null character) to reject this input
                return '\0';
            }
        }
        
        // Character is allowed, so return it unchanged
        return addedChar;
    }
    
    // Classes for JSON serialization
    [System.Serializable]
    private class RequestData
    {
        public string message;
    }
    
    [System.Serializable]
    private class ResponseData
    {
        public string response;
    }
}