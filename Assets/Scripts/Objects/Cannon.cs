using UnityEngine;

public class Cannon : MonoBehaviour, IInteractableObject
{
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private GameObject cannonView;
    public void Interact()
    {
        Debug.Log("Interact was called");
        cannonView.SetActive(!cannonView.activeSelf);
        GameState newState = (GameManager.Instance.currentState == GameState.Combat) ? GameState.Aiming : GameState.Combat;
        GameManager.Instance.ChangeState(newState);
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
