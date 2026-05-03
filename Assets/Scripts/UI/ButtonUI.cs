using UnityEngine;

public class ButtonUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void DamageShipButton()
    {
        Debug.Log("Damage Button clicked");
        RunManager.Instance.DamageShip(10);
    }
    public void HealShipButton()
    {
        Debug.Log("Damage Button clicked");
        RunManager.Instance.AddHealth(10);
    }
}
