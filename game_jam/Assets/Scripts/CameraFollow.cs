using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 5f; // Increased for better response
    
    [Header("Camera Position Settings")]
    [SerializeField] private Vector3 positionOffset = new Vector3(-5, -5, -8); // Offset for 2.5D view
    [SerializeField] private Vector3 rotationAngles = new Vector3(-20, 0, 0); // Rotation for 2.5D view
    
    [Header("Camera Boundaries")]
    [SerializeField] private bool useBoundaries = true; 
    [SerializeField] private float minX = -15f;
    [SerializeField] private float maxX = 8f;  
    [SerializeField] private float minZ = -10f;
    [SerializeField] private float maxZ = 10f;
    [SerializeField] private bool showDebugInfo = false;
      
    // Store the initial rotation to maintain consistency
    private Quaternion targetRotation;
    private Camera cam;
    private Vector3 initialPosition;
    private Vector3 previousTargetPosition;
    private Vector3 velocity = Vector3.zero;
    
    private void Awake()
    {
        // Get the camera component early
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            cam = Camera.main;
        }
    }
    
    private void Start()
    {
        // Set the initial rotation based on the specified angles
        targetRotation = Quaternion.Euler(rotationAngles);
        transform.rotation = targetRotation;
        
        initialPosition = transform.position;
        
        // Find target if not already set
        if (target == null)
        {
            GameObject player = GameObject.FindWithTag("Player");
            if (player != null)
            {
                target = player.transform;
                previousTargetPosition = target.position;
            }
        }
        else
        {
            previousTargetPosition = target.position;
        }
    }
      // Call this method to set the camera boundaries based on a game area
    public void SetBoundaries(float minXValue, float maxXValue, float minZValue, float maxZValue)
    {
        this.minX = minXValue;
        this.maxX = maxXValue;
        this.minZ = minZValue;
        this.maxZ = maxZValue;
        useBoundaries = true;
        
        Debug.Log($"Camera boundaries set: X({minXValue} to {maxXValue}), Z({minZValue} to {maxZValue})");
    }
    
    // Method to reset the camera position if it gets stuck
    [ContextMenu("Reset Camera Position")]
    public void ResetCameraPosition()
    {
        if (target != null)
        {
            // Calculate fresh position directly from target position
            Vector3 newPosition = new Vector3(
                target.position.x + positionOffset.x,
                target.position.y + positionOffset.y,
                target.position.z + positionOffset.z
            );
            
            transform.position = newPosition;
            transform.rotation = targetRotation;
            Debug.Log("Camera position has been reset");
        }
    }
      private void LateUpdate()
    {
        if (target == null)
        {
            // Try to find the player if target is not set
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                target = player.transform;
            }
            else
            {
                return;
            }
        }
        
        // Calculate the desired position based on target position
        Vector3 targetPosition = target.position;
        
        // We need to follow in X and Z axes (horizontal plane), Y will be based on fixed offset
        float targetX = targetPosition.x;
        float targetY = targetPosition.y; // Store Y for fixed offset
        float targetZ = targetPosition.z;
        
        // Apply desired offset to position
        Vector3 desiredPosition = new Vector3(
            targetX + positionOffset.x,
            targetY + positionOffset.y, 
            targetZ + positionOffset.z
        );
          // If boundaries are enabled, clamp the desired position
        if (useBoundaries && cam != null)
        {
            // Calculate camera's viewport frustum size at the target distance
            float camHalfHeight, camHalfWidth;
            
            if (cam.orthographic)
            {
                camHalfHeight = cam.orthographicSize;
                camHalfWidth = camHalfHeight * cam.aspect;
            }
            else
            {
                // For perspective camera, we need to consider the rotated view
                // Get the distance from camera to player along the camera's forward vector
                float distanceZ = Mathf.Abs(positionOffset.z);
                float angleInRadians = rotationAngles.x * Mathf.Deg2Rad;
                
                // Adjust distance based on angle (simple approximation)
                float effectiveDistance = distanceZ * Mathf.Cos(angleInRadians);
                
                // Calculate viewport size at the target distance
                camHalfHeight = effectiveDistance * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                camHalfWidth = camHalfHeight * cam.aspect;
            }
            
            // Calculate actual boundaries for the camera view to avoid seeing outside the tapete
            float actualMinX = minX + camHalfWidth;
            float actualMaxX = maxX - camHalfWidth;
            float actualMinZ = minZ + camHalfHeight;
            float actualMaxZ = maxZ - camHalfHeight;
            
            // Check if the boundaries are too small for the camera view
            if (actualMaxX <= actualMinX || actualMaxZ <= actualMinZ)
            {
                // The boundaries are smaller than the camera view - center the camera on the boundaries
                if (actualMaxX <= actualMinX)
                {
                    float centerX = (maxX + minX) * 0.5f;
                    desiredPosition.x = centerX + positionOffset.x;
                }
                else
                {
                    // Clamp target position to stay within actual boundaries
                    float clampedTargetX = Mathf.Clamp(targetX, actualMinX, actualMaxX);
                    desiredPosition.x = clampedTargetX + positionOffset.x;
                }
                
                if (actualMaxZ <= actualMinZ)
                {
                    float centerZ = (maxZ + minZ) * 0.5f;
                    desiredPosition.z = centerZ + positionOffset.z;
                }
                else
                {
                    // Clamp target position to stay within actual boundaries
                    float clampedTargetZ = Mathf.Clamp(targetZ, actualMinZ, actualMaxZ);
                    desiredPosition.z = clampedTargetZ + positionOffset.z;
                }
            }
            else
            {
                // Normal clamping - viewport is smaller than boundaries
                float clampedX = Mathf.Clamp(targetX, actualMinX, actualMaxX);
                float clampedZ = Mathf.Clamp(targetZ, actualMinZ, actualMaxZ);
                
                // Apply clamping while maintaining offset
                desiredPosition.x = clampedX + positionOffset.x;
                desiredPosition.z = clampedZ + positionOffset.z;
            }
            
            // Debug info
            if (showDebugInfo)
            {
                Debug.Log($"Camera view: width={camHalfWidth*2}, height={camHalfHeight*2}");
                Debug.Log($"Player pos: {targetPosition}, Camera desired: {desiredPosition}");
                Debug.Log($"Actual boundaries: X({actualMinX} to {actualMaxX}), Z({actualMinZ} to {actualMaxZ})");
            }
        }
          // Smoothly move the camera to the desired position
        Vector3 smoothedPosition = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref velocity, 
            1f / (smoothSpeed * 10f)
        );
        
        transform.position = smoothedPosition;
        
        // Ensure we maintain our target rotation
        transform.rotation = targetRotation;
    }
      // Draw the camera boundaries in the editor for easier setup
    private void OnDrawGizmosSelected()
    {
        if (!useBoundaries) return;
        
        // Draw the boundary rectangle on the ground plane
        Gizmos.color = Color.yellow;
        
        // Draw the corners of the boundary on ground plane (Y=0)
        Vector3 topLeft = new Vector3(minX, 0, maxZ);
        Vector3 topRight = new Vector3(maxX, 0, maxZ);
        Vector3 bottomLeft = new Vector3(minX, 0, minZ);
        Vector3 bottomRight = new Vector3(maxX, 0, minZ);
        
        // Draw the edges of the boundary
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
        Gizmos.DrawLine(bottomLeft, topLeft);
        
        // If we have a camera component, also visualize the actual view frustum
        Camera gizmoCam = GetComponent<Camera>();
        if (gizmoCam != null)
        {
            // Draw camera frustum with a different color
            Gizmos.color = new Color(0.2f, 1f, 0.3f, 0.3f);
            
            // Calculate camera's view boundaries at current position
            float camHalfHeight, camHalfWidth;
            
            if (gizmoCam.orthographic)
            {
                camHalfHeight = gizmoCam.orthographicSize;
                camHalfWidth = camHalfHeight * gizmoCam.aspect;
            }
            else
            {
                // For perspective camera, approximate the view area at y=0 plane
                float distanceToGround = Mathf.Abs(transform.position.y);
                camHalfHeight = distanceToGround * Mathf.Tan(gizmoCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
                camHalfWidth = camHalfHeight * gizmoCam.aspect;
                
                // Adjust for angled view
                if (rotationAngles.x != 0)
                {
                    float angleCorrection = Mathf.Cos(rotationAngles.x * Mathf.Deg2Rad);
                    camHalfHeight /= angleCorrection;
                }
            }
            
            // Draw camera's current view area as a semi-transparent cube
            Vector3 cameraPos = transform.position;
            Matrix4x4 originalMatrix = Gizmos.matrix;
            
            // Transform to camera's coordinate system
            Gizmos.matrix = Matrix4x4.TRS(
                cameraPos, 
                Quaternion.Euler(0, transform.eulerAngles.y, 0), 
                Vector3.one);
            
            // Draw the camera's view frustum at ground level
            Gizmos.DrawCube(new Vector3(0, -cameraPos.y, camHalfHeight), 
                new Vector3(camHalfWidth * 2, 0.1f, camHalfHeight * 2));
            
            // Restore the original matrix
            Gizmos.matrix = originalMatrix;
        }
    }
}
