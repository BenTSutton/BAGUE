using UnityEngine;
using TMPro;

//Placeholder ship info panel
public class ShipInfoPanel : MonoBehaviour
{

    public TMP_Text healthText;
    public TMP_Text fuelText;
    public TMP_Text moneyText;

    // Update is called once per frame
    void Update()
    {
        healthText.text = "Health: " + RunManager.Instance.currentShipHealth.ToString() + " / " + RunManager.Instance.maxShipHealth.ToString();
        fuelText.text = "Fuel: " + RunManager.Instance.fuel.ToString();
        moneyText.text = "Money: " + RunManager.Instance.money.ToString();
    }
}
