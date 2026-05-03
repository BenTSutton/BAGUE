using UnityEngine;

public class Cannon : MonoBehaviour, IInteractableObject
{
    [SerializeField] private GameObject interactionPrompt;
    public void Interact()
    {
        Debug.Log("Interact was called");
    }

    public void ShowInteractPrompt()
    {
        interactionPrompt.SetActive(true);
    }

    public void HideInteractPrompt()
    {
        interactionPrompt.SetActive(false);
    }

}
