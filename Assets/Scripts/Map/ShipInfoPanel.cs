using UnityEngine;
using TMPro;
using System.Collections.Generic;

//Placeholder ship info panel
public class ShipInfoPanel : MonoBehaviour
{

    public TMP_Text healthText;
    public TMP_Text fuelText;
    public TMP_Text moneyText;
    public TMP_Text scrapText;
    public TMP_Text crewText;

    // Update is called once per frame
    void Update()
    {
        healthText.text = RunManager.Instance.currentShipHealth.ToString() + " / " + RunManager.Instance.maxShipHealth.ToString();
        fuelText.text = RunManager.Instance.fuel.ToString();
        moneyText.text = RunManager.Instance.money.ToString();
        scrapText.text = RunManager.Instance.scrap.ToString();
        ConstructCrewText();
    }

    void ConstructCrewText()
    {
        List<string> crewNames = new List<string>();

        foreach (var crew in RunManager.Instance.activeCrew)
        {
            crewNames.Add(crew.crewName);
        }

        crewText.text = string.Join("\n", crewNames);
    }
}
