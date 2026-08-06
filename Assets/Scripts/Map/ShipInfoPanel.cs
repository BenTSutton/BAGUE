using UnityEngine;
using TMPro;

//Placeholder ship info panel
public class ShipInfoPanel : MonoBehaviour
{

    public TMP_Text healthText;
    public TMP_Text fuelText;
    public TMP_Text moneyText;
    public TMP_Text scrapText;
    public TMP_Text crewText;
    public Transform crewContainer;
    public CrewSlotUI crewSlotPrefab;

    public Transform passiveEffectContainer;
    public PassiveEffectDisplay passiveEffectSlotPrefab;

    private int displayedEffectCount = -1;
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
        
        if (displayedEffectCount != RunManager.Instance.activeTreasureEffects.Count)
            RefreshPassiveEffects();
    }

    void RefreshCrew()
    {
        foreach (Transform child in crewContainer)
            Destroy(child.gameObject);

        foreach (var crew in RunManager.Instance.activeCrew)
        {
            if (crew != null)
                Instantiate(crewSlotPrefab, crewContainer).Setup(crew);
        }

        displayedCrewCount = RunManager.Instance.activeCrew.Count;
        crewText.text = displayedCrewCount + " / " + RunManager.Instance.MaxCrewCapacity;
    }

    void RefreshPassiveEffects()
    {
        foreach (Transform child in passiveEffectContainer)
            Destroy(child.gameObject);

        foreach (PersistentTreasureEffect effect
                in RunManager.Instance.activeTreasureEffects)
        {
            if (effect != null)
            {
                Instantiate(passiveEffectSlotPrefab, passiveEffectContainer)
                    .Setup(effect);
            }
        }

        displayedEffectCount = RunManager.Instance.activeTreasureEffects.Count;
    }
}
