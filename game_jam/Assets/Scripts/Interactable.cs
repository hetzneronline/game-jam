using UnityEngine;
using TMPro; // <-- Wichtig für TextMeshPro

public class Interactable : MonoBehaviour
{
    public TMP_Text uiText; // <-- Der richtige Typ für TextMeshPro

    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            uiText.text = "Du hast interagiert!";
            uiText.gameObject.SetActive(true);
            Invoke(nameof(HideText), 2f);
        }
    }

    void HideText()
    {
        uiText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
