using UnityEngine;

public class GroundSetup : MonoBehaviour
{
    void Start()
    {
        // Make sure the ground has the "Ground" tag
        if (tag != "Ground")
        {
            tag = "Ground";
            Debug.Log("Ground tag applied to " + gameObject.name);
        }
        
        // Make sure it has a collider for the player to land on
        if (GetComponent<BoxCollider2D>() == null)
        {
            gameObject.AddComponent<BoxCollider2D>();
            Debug.Log("Added BoxCollider2D to " + gameObject.name);
        }
    }
}
