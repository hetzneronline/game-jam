using UnityEngine;

public class InteractiveDoor : InteractiveObject
{
    [Header("Door Settings")]
    [SerializeField] private bool isLocked = false;
    [SerializeField] private bool autoClose = true;
    [SerializeField] private float closeDelay = 3f;
    
    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;
    private float doorMovement = 2f; // How far the door moves when opened
    
    protected override void Start()
    {
        base.Start(); // Call base Start method to set up renderer and collider
        
        // Store the closed position
        closedPosition = transform.position;
        
        // Calculate the open position (for a simple sliding door)
        // Here we slide the door up, but you could adjust this logic for different door movements
        openPosition = closedPosition + new Vector3(0, doorMovement, 0);
    }
    
    protected override void Interact()
    {
        if (isLocked)
        {
            Debug.Log("This door is locked!");
            return;
        }
        
        ToggleDoor();
    }
    
    void ToggleDoor()
    {
        isOpen = !isOpen;
        
        // Move the door to its open/closed position
        transform.position = isOpen ? openPosition : closedPosition;
        
        Debug.Log("Door is now " + (isOpen ? "open" : "closed"));
        
        // If auto-close is enabled and the door is open, start the close timer
        if (autoClose && isOpen)
        {
            Invoke("CloseDoor", closeDelay);
        }
    }
    
    void CloseDoor()
    {
        if (isOpen)
        {
            isOpen = false;
            transform.position = closedPosition;
            Debug.Log("Door auto-closed");
        }
    }
    
    // Method to lock/unlock the door from other scripts
    public void SetLockState(bool locked)
    {
        isLocked = locked;
        Debug.Log("Door is now " + (locked ? "locked" : "unlocked"));
    }
}
