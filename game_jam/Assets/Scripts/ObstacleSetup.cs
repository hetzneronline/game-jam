using UnityEngine;

public class ObstacleSetup : MonoBehaviour
{
    void Start()
    {
        // Make sure the obstacle has the "Obstacle" tag
        if (tag != "Obstacle")
        {
            tag = "Obstacle";
            Debug.Log("Obstacle tag applied to " + gameObject.name);
        }
        
        // Make sure it has a collider for collision detection
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("Added BoxCollider2D to " + gameObject.name);
        }
    }
}
