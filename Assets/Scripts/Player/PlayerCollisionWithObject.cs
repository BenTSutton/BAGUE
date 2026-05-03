using UnityEngine;

public class PlayerCollisionWithObject : MonoBehaviour
{
    // Look to see if object you are colliding with uses the IInteractableObject interface and if so do an action
    private IInteractableObject currentTarget;

    private void Update()
    {
        // 1. Check if we have a target and if the player presses 'E'
        if (currentTarget != null && Input.GetKeyDown(KeyCode.E))
        {
            // 2. Trigger the function on the object
            currentTarget.Interact();
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractableObject>();
        if (interactable != null)
        {
            currentTarget = interactable;
            Debug.Log("Entered: " + other.name);
            interactable.ShowInteractPrompt();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other.GetComponent<IInteractableObject>();
        // Check if the object we are leaving is the one we currently care about
        if (interactable != null && interactable == currentTarget)
        {
            currentTarget = null;
            Debug.Log("Exited: " + other.name);
            interactable.HideInteractPrompt();
        }
    }
}
