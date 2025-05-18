using UnityEngine;

public class CollectibleItem : InteractiveObject
{
    [Header("Collectible Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private bool destroyOnCollect = true;
    [SerializeField] private GameObject collectEffect;
    [SerializeField] private AudioClip collectSound;
    
    protected override void Interact()
    {
        // Add score using the ScoreManager
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }
        else
        {
            Debug.LogWarning("No ScoreManager found! Score not added.");
        }
        
        Debug.Log("Player collected " + gameObject.name + " worth " + scoreValue + " points");
        
        // Play sound effect if available
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // Spawn collection effect if available
        if (collectEffect != null)
        {
            Instantiate(collectEffect, transform.position, Quaternion.identity);
        }
        
        // Destroy the object if configured to do so
        if (destroyOnCollect)
        {
            Destroy(gameObject);
        }
    }
}
