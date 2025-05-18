using UnityEngine;

public class GameSetup : MonoBehaviour
{
    [Header("Player Setup")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    
    void Start()
    {
        // If no spawn point is specified, use this object's position
        if (playerSpawnPoint == null)
        {
            playerSpawnPoint = transform;
        }
        
        // Instantiate the player if a prefab is provided
        if (playerPrefab != null)
        {
            Instantiate(playerPrefab, playerSpawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("No player prefab assigned to GameSetup!");
        }
    }
}
