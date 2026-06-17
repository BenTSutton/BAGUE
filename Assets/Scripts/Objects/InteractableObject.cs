using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] private GameObject interactionPrompt;

    public virtual void Interact()
    {
        throw new System.NotImplementedException();
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
