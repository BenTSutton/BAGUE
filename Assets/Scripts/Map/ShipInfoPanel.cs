using UnityEngine;
using TMPro;

//Placeholder ship info panel
public class ShipInfoPanel : MonoBehaviour
{

    public TMP_Text healthText;
    public TMP_Text fuelText;
    public TMP_Text moneyText;
    public TMP_Text scrapText;
    public Transform crewContainer;
    public CrewSlotUI crewSlotPrefab;

    private int displayedCrewCount = -1;

    // Update is called once per frame
    void Update()
    {
        healthText.text = RunManager.Instance.currentShipHealth.ToString() + " / " + RunManager.Instance.maxShipHealth.ToString();
        fuelText.text = RunManager.Instance.fuel.ToString();
        moneyText.text = RunManager.Instance.money.ToString();
        scrapText.text = RunManager.Instance.scrap.ToString();
        if (displayedCrewCount != RunManager.Instance.activeCrew.Count)
            RefreshCrew();
    }

    void RefreshCrew()
    {
        foreach (Transform child in crewContainer)
            Destroy(child.gameObject);

        foreach (var crew in RunManager.Instance.activeCrew)
            Instantiate(crewSlotPrefab, crewContainer).Setup(crew);

        displayedCrewCount = RunManager.Instance.activeCrew.Count;
    }
}
