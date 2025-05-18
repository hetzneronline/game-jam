using UnityEngine;

public class SpawnPoint : MonoBehaviour
{
    [SerializeField] private string pointId = "";
    
    // Change this property to keep the original ID format
    public string FullName => string.IsNullOrEmpty(pointId) ? "SpawnPoint" : $"SpawnPoint_{pointId}";
    
    private void Start()
    {
        // IMPORTANT: Comment out or remove this auto-renaming!
        // We want to keep the names like SpawnPoint_env1, not rename them
        /*
        // Auto-fix the name if it doesn't match the expected format
        if (name != FullName && name != "SpawnPoint")
        {
            // Extract ID from name if possible (for names like "Spawnpoint (1)")
            if (string.IsNullOrEmpty(pointId) && name.Contains("(") && name.Contains(")"))
            {
                try {
                    string idPart = name.Substring(name.IndexOf("(") + 1);
                    idPart = idPart.Substring(0, idPart.IndexOf(")"));
                    pointId = idPart;
                } catch {}
            }
            
            // Fix the name
            string oldName = name;
            name = FullName;
            Debug.Log($"Renamed spawn point from '{oldName}' to '{name}' for proper identification");
        }
        */
        
        // Instead, just extract the ID if needed but keep the original name
        if (string.IsNullOrEmpty(pointId) && name.StartsWith("SpawnPoint_"))
        {
            pointId = name.Substring("SpawnPoint_".Length);
            Debug.Log($"Extracted ID '{pointId}' from spawn point name '{name}'");
        }
    }
    
    private void OnDrawGizmos()
    {
        // Visualize spawn points in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
        
        #if UNITY_EDITOR
        // This will only compile in the editor
        UnityEditor.Handles.Label(transform.position + Vector3.up, name);
        #endif
    }
}