using UnityEngine;

public class Cannon : InteractableObject
{
    [SerializeField] private float cannonDamage;
    public Canvas cannonView;
    
    public float strength => cannonDamage;
    public override void Interact()
    {
        Debug.Log("Interact was called");
        cannonView.enabled = !cannonView.enabled;
        if (cannonView.enabled)
        {
            RunManager.Instance.activeCannon = this;
        }
        else
        {
            RunManager.Instance.activeCannon = null;
        }
        GameState newState = (GameManager.Instance.currentState == GameState.Combat) ? GameState.Aiming : GameState.Combat;
        GameManager.Instance.ChangeState(newState);
    }
}
