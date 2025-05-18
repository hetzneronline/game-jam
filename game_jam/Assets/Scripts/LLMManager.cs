using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class LLMManager : MonoBehaviour
{
    public static LLMManager Instance { get; private set; }
    
    [Header("API Configuration")]
    [SerializeField] private string apiUrl = "http://127.0.0.1:5001";
    [SerializeField] private float statusCheckInterval = 0.5f;
    
    [Header("Visual Indicators")]
    [SerializeField] private List<EchoVisualizer> echoVisualizers;
    
    // Events
    public delegate void LLMSpeakingHandler(bool isSpeaking);
    public static event LLMSpeakingHandler OnLLMSpeakingChanged;
    
    private bool isSpeaking = false;
    private Coroutine statusCheckCoroutine;
    
    private void Awake()
    {
        // Singleton pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        StartStatusChecking();
    }
    
    public void StartStatusChecking()
    {
        if (statusCheckCoroutine != null)
        {
            StopCoroutine(statusCheckCoroutine);
        }
        
        statusCheckCoroutine = StartCoroutine(CheckSpeakingStatus());
    }
    
    private IEnumerator CheckSpeakingStatus()
    {
        while (true)
        {
            yield return CheckIfLLMIsSpeaking();
            yield return new WaitForSeconds(statusCheckInterval);
        }
    }
    
    private IEnumerator CheckIfLLMIsSpeaking()
    {
        using (UnityWebRequest request = UnityWebRequest.Get($"{apiUrl}/status"))
        {
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                string response = request.downloadHandler.text;
                bool wasSpeaking = isSpeaking;
                
                // Parse JSON response
                var responseData = JsonUtility.FromJson<LLMStatusResponse>(response);
                isSpeaking = responseData.speaking;
                
                if (wasSpeaking != isSpeaking)
                {
                    // Update all echo visualizers
                    foreach (var visualizer in echoVisualizers)
                    {
                        if (visualizer != null)
                        {
                            if (isSpeaking)
                            {
                                visualizer.OnLLMStartedSpeaking();
                            }
                            else
                            {
                                visualizer.OnLLMStoppedSpeaking();
                            }
                        }
                    }
                    
                    // Trigger event for other scripts to use
                    OnLLMSpeakingChanged?.Invoke(isSpeaking);
                }
            }
        }
    }
    
    // Register a new echo visualizer
    public void RegisterEchoVisualizer(EchoVisualizer visualizer)
    {
        if (!echoVisualizers.Contains(visualizer))
        {
            echoVisualizers.Add(visualizer);
        }
    }
    
    // Unregister an echo visualizer
    public void UnregisterEchoVisualizer(EchoVisualizer visualizer)
    {
        if (echoVisualizers.Contains(visualizer))
        {
            echoVisualizers.Remove(visualizer);
        }
    }
    
    // Send a message to the LLM
    public IEnumerator SendMessage(string message, System.Action<string> onResponse)
    {
        using (UnityWebRequest request = new UnityWebRequest($"{apiUrl}/query", "POST"))
        {
            // Create JSON data
            LLMQueryRequest requestData = new LLMQueryRequest { message = message };
            string jsonData = JsonUtility.ToJson(requestData);
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            
            // Set up request
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            
            // Send request
            yield return request.SendWebRequest();
            
            if (request.result == UnityWebRequest.Result.Success)
            {
                // Parse response
                string responseJson = request.downloadHandler.text;
                LLMQueryResponse responseData = JsonUtility.FromJson<LLMQueryResponse>(responseJson);
                
                // Invoke callback with response
                onResponse?.Invoke(responseData.response);
            }
            else
            {
                Debug.LogError($"Error sending message: {request.error}");
                onResponse?.Invoke("Sorry, I couldn't process your request.");
            }
        }
    }
    
    private void OnDestroy()
    {
        if (statusCheckCoroutine != null)
        {
            StopCoroutine(statusCheckCoroutine);
        }
    }
    
    // Helper classes for JSON serialization
    [System.Serializable]
    private class LLMStatusResponse
    {
        public bool speaking;
        public long last_spoke;
    }
    
    [System.Serializable]
    private class LLMQueryRequest
    {
        public string message;
    }
    
    [System.Serializable]
    private class LLMQueryResponse
    {
        public string response;
    }
}