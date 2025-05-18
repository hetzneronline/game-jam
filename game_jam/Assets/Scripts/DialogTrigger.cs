using UnityEngine;

public class DialogTrigger : InteractiveObject
{
    [Header("Dialog Settings")]
    [SerializeField] private string[] dialogLines;
    [SerializeField] private string speakerName = "NPC";
    [SerializeField] private bool showOnlyOnce = false;
    
    private bool hasInteracted = false;
    
    protected override void Start()
    {
        // Call the base class Start method to set up the renderer and collider
        base.Start();
        
        // Set a custom prompt message for dialogs
        interactionPrompt = "Press E to talk";
    }
    
    protected override void Interact()
    {
        ShowDialog();
    }
    
    private void ShowDialog()
    {
        if (showOnlyOnce && hasInteracted)
        {
            Debug.Log("This dialog has already been shown.");
            return;
        }
        
        Debug.Log(speakerName + " says:");
        foreach (string line in dialogLines)
        {
            Debug.Log(line);
        }
        
        hasInteracted = true;
        
        // In a real game, you would connect this to your dialog UI system
    }
    
    // Visualization in editor
    protected override void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
