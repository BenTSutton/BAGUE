using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutpostDialogPanel : MonoBehaviour
{
    public static OutpostDialogPanel Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text creditText;

    public Image creditItem1Image;
    public Image creditItem2Image;
    public Image crewItem1Image;
    public Image crewItem2Image;
    public TMP_Text creditItem1Text;
    public TMP_Text creditItem2Text;
    public TMP_Text crewItem1Text;
    public TMP_Text crewItem2Text;
    public TMP_Text creditItem1CostText;
    public TMP_Text creditItem2CostText;
    public TMP_Text crewItem1CostText;
    public TMP_Text crewItem2CostText;
    public TMP_Text crewItem1NameText;
    public TMP_Text crewItem2NameText;
    public TMP_Text item1ButtonText;
    public TMP_Text item2ButtonText;
    public TMP_Text item3ButtonText;
    public TMP_Text item4ButtonText;
    public Button item1Button;
    public Button item2Button;
    public Button item3Button;
    public Button item4Button;

    [Header("Optional Service UI")]
    public Button repairServiceButton;
    public TMP_Text repairServiceButtonText;
    public Button refuelServiceButton;
    public TMP_Text refuelServiceButtonText;
    public Button rerollButton;
    public TMP_Text rerollButtonText;

    public TreasureDatabase treasureDatabase;
    public CrewDatabase crewDatabase;

    private NodeState currentState;
    private OutpostDefinition currentOutpost;

    private Treasure firstTreasure;
    private Treasure secondTreasure;
    private CrewMember firstCrew;
    private CrewMember secondCrew;

    private bool item1Purchased;
    private bool item2Purchased;
    private bool item3Purchased;
    private bool item4Purchased;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(OutpostDefinition outpostDefinition, NodeState state)
    {
        SFXManager.Instance?.PlayNotice();
        currentOutpost = outpostDefinition;
        currentState = state;

        currentOutpost?.EnsureFrameworkDefaults();

        PanelAnimation.Open(panel);
        MusicManager.Instance.PlayOutpostMusic();
        PopulateItems();
    }

    public void Close()
    {
        SFXManager.Instance?.PlayCancel();
        PanelAnimation.Close(panel);
        MusicManager.Instance.PlayMapMusic();
        MapRunState.Instance.CompleteCurrentNodeAfterEvent(currentState.node);
    }

    void RefreshCurrency()
    {
        creditText.text = RunManager.Instance.money.ToString();
        RefreshOfferCostLabels();
        RefreshPurchaseButtons();
        RefreshServiceButtons();
    }

    void RefreshOfferCostLabels()
    {
        UpdateCostLabel(creditItem1CostText, firstTreasure?.price);
        UpdateCostLabel(creditItem2CostText, secondTreasure?.price);
        UpdateCostLabel(crewItem1CostText, firstCrew?.price);
        UpdateCostLabel(crewItem2CostText, secondCrew?.price);
    }

    private void UpdateCostLabel(TMP_Text label, int? basePrice)
    {
        if (label != null && basePrice.HasValue)
            label.text = $"Buy: {GetOfferPrice(basePrice.Value)} Credits";
    }

    void PopulateItems()
    {
        firstTreasure = treasureDatabase != null
            ? treasureDatabase.GetRandomPurchasableCommonTreasure(null)
            : null;
        secondTreasure = treasureDatabase != null
            ? treasureDatabase.GetRandomPurchasableCommonTreasure(
                new[] { firstTreasure },
                firstTreasure != null ? new[] { firstTreasure.type } : null)
            : null;

        List<CrewMember> unavailableCrew = new List<CrewMember>(RunManager.Instance.activeCrew);
        firstCrew = crewDatabase != null
            ? crewDatabase.GetRandomPurchasableCrew(unavailableCrew)
            : null;
        if (firstCrew != null)
            unavailableCrew.Add(firstCrew);
        secondCrew = crewDatabase != null
            ? crewDatabase.GetRandomPurchasableCrew(unavailableCrew)
            : null;

        item1Purchased = false;
        item2Purchased = false;
        item3Purchased = false;
        item4Purchased = false;

        ShowTreasureOffer(firstTreasure, creditItem1Image, creditItem1Text, creditItem1CostText);
        ShowTreasureOffer(secondTreasure, creditItem2Image, creditItem2Text, creditItem2CostText);
        ShowCrewOffer(firstCrew, crewItem1Image, crewItem1Text, crewItem1CostText, crewItem1NameText);
        ShowCrewOffer(secondCrew, crewItem2Image, crewItem2Text, crewItem2CostText, crewItem2NameText);
        RefreshCurrency();
    }

    private void ShowTreasureOffer(
        Treasure treasure,
        Image image,
        TMP_Text description,
        TMP_Text cost)
    {
        if (treasure == null)
        {
            ShowUnavailableOffer(image, description, cost);
            return;
        }

        image.sprite = treasure.icon;
        description.text = treasure.description;
        cost.text = $"Buy: {GetOfferPrice(treasure.price)} Credits";
    }

    private void ShowCrewOffer(
        CrewMember crew,
        Image image,
        TMP_Text description,
        TMP_Text cost,
        TMP_Text name)
    {
        if (crew == null)
        {
            ShowUnavailableOffer(image, description, cost, name);
            return;
        }

        image.sprite = crew.icon;
        description.text = crew.description;
        name.text = crew.crewName;
        cost.text = $"Buy: {GetOfferPrice(crew.price)} Credits";
    }

    void RefreshPurchaseButtons()
    {
        int credits = RunManager.Instance != null ? RunManager.Instance.money : 0;

        UpdatePurchaseButton(item1Button, item1ButtonText, firstTreasure, item1Purchased, credits);
        UpdatePurchaseButton(item2Button, item2ButtonText, secondTreasure, item2Purchased, credits);
        UpdateCrewPurchaseButton(item3Button, item3ButtonText, firstCrew, item3Purchased, credits);
        UpdateCrewPurchaseButton(item4Button, item4ButtonText, secondCrew, item4Purchased, credits);
    }

    void UpdateCrewPurchaseButton(
        Button button,
        TMP_Text buttonText,
        CrewMember crew,
        bool purchased,
        int credits)
    {
        if (button == null || buttonText == null)
            return;

        if (crew == null)
        {
            button.interactable = false;
            buttonText.text = "Unavailable";
            return;
        }

        if (purchased)
        {
            button.interactable = false;
            buttonText.text = "Purchased!";
            return;
        }

        CrewAcquisitionResult availability =
            RunManager.Instance.GetCrewAcquisitionAvailability(crew);
        if (availability != CrewAcquisitionResult.Success)
        {
            button.interactable = false;
            buttonText.text = RunManager.Instance.GetCrewAcquisitionMessage(availability, crew);
            return;
        }

        int price = GetOfferPrice(crew.price);
        bool canAfford = credits >= price;
        button.interactable = canAfford;
        buttonText.text = canAfford
            ? $"Buy: {price} Credits"
            : $"Need {price} Credits";
    }

    int GetOfferPrice(int basePrice)
    {
        int outpostPrice = GetOutpostBasePrice(basePrice);
        return RunManager.Instance != null
            ? RunManager.Instance.GetPurchasePrice(outpostPrice)
            : outpostPrice;
    }

    int GetOutpostBasePrice(int basePrice)
    {
        float multiplier = currentOutpost != null
            ? Mathf.Max(0.01f, currentOutpost.priceMultiplier)
            : 1f;
        return Mathf.Max(0, Mathf.CeilToInt(basePrice * multiplier));
    }

    void UpdatePurchaseButton(
        Button button,
        TMP_Text buttonText,
        Treasure treasure,
        bool purchased,
        int credits)
    {
        if (button == null || buttonText == null)
            return;

        if (treasure == null)
        {
            button.interactable = false;
            buttonText.text = "Unavailable";
            return;
        }

        if (purchased)
        {
            button.interactable = false;
            buttonText.text = "Purchased!";
            return;
        }

        int price = GetOfferPrice(treasure.price);
        bool canAfford = credits >= price;
        button.interactable = canAfford;
        buttonText.text = canAfford
            ? $"Buy: {price} Credits"
            : $"Need {price} Credits";
    }

    void ShowUnavailableOffer(Image image, TMP_Text description, TMP_Text cost)
    {
        image.sprite = null;
        description.text = "No item available";
        cost.text = string.Empty;
    }

    void ShowUnavailableOffer(Image image, TMP_Text description, TMP_Text cost, TMP_Text name)
    {
        image.sprite = null;
        description.text = "No item available";
        cost.text = string.Empty;
        name.text = string.Empty;
    }

    public void BuyItem1()
    {
        BuyTreasure(firstTreasure, () => item1Purchased = true);
    }

    public void BuyItem2()
    {
        BuyTreasure(secondTreasure, () => item2Purchased = true);
    }

    public void BuyItem3()
    {
        BuyCrew(firstCrew, () => item3Purchased = true);
    }

    public void BuyItem4()
    {
        BuyCrew(secondCrew, () => item4Purchased = true);
    }

    private void BuyTreasure(Treasure treasure, System.Action markPurchased)
    {
        if (treasure == null)
            return;

        int price = GetOfferPrice(treasure.price);
        if (RunManager.Instance.TrySpendMoney(price))
        {
            treasure.ApplyEffect();
            markPurchased();
            SFXManager.Instance?.PlayPositive();
        }
        else
            SFXManager.Instance?.PlayNegative();

        RefreshCurrency();
    }

    private void BuyCrew(CrewMember crew, System.Action markPurchased)
    {
        if (crew == null)
            return;

        CrewAcquisitionResult result = RunManager.Instance.TryPurchaseCrew(
            crew,
            GetOutpostBasePrice(crew.price));
        if (result == CrewAcquisitionResult.Success)
        {
            markPurchased();
            SFXManager.Instance?.PlayPositive();
        }
        else
        {
            SFXManager.Instance?.PlayNegative();
        }

        RefreshCurrency();
    }

    void RefreshServiceButtons()
    {
        if (currentOutpost == null || RunManager.Instance == null)
            return;

        UpdateServiceButton(
            repairServiceButton,
            repairServiceButtonText,
            currentOutpost.HasService(OutpostServiceType.Repair),
            currentOutpost.repairServiceCost,
            $"Repair {currentOutpost.repairServiceAmount} Hull",
            RunManager.Instance.currentShipHealth < RunManager.Instance.maxShipHealth);
        UpdateServiceButton(
            refuelServiceButton,
            refuelServiceButtonText,
            currentOutpost.HasService(OutpostServiceType.Refuel),
            currentOutpost.refuelServiceCost,
            $"Buy {currentOutpost.refuelServiceAmount} Fuel",
            true);
        UpdateServiceButton(
            rerollButton,
            rerollButtonText,
            currentOutpost.HasService(OutpostServiceType.Reroll),
            currentOutpost.rerollCost,
            "Reroll Stock",
            true);
    }

    void UpdateServiceButton(
        Button button,
        TMP_Text buttonText,
        bool serviceAvailable,
        int baseCost,
        string label,
        bool useful)
    {
        if (button == null || buttonText == null)
            return;

        button.gameObject.SetActive(serviceAvailable);
        if (!serviceAvailable)
            return;

        int price = GetOfferPrice(baseCost);
        bool canAfford = RunManager.Instance.money >= price;
        button.interactable = useful && canAfford;
        buttonText.text = !useful
            ? "Hull Already Full"
            : canAfford
                ? $"{label}: {price} Credits"
                : $"Need {price} Credits";
    }

    public void BuyRepairService()
    {
        if (currentOutpost == null
            || !currentOutpost.HasService(OutpostServiceType.Repair)
            || RunManager.Instance.currentShipHealth >= RunManager.Instance.maxShipHealth)
        {
            RefreshCurrency();
            return;
        }

        int price = GetOfferPrice(currentOutpost.repairServiceCost);
        if (!RunManager.Instance.TrySpendMoney(price))
        {
            SFXManager.Instance?.PlayNegative();
            RefreshCurrency();
            return;
        }

        RunManager.Instance.AddHealth(currentOutpost.repairServiceAmount);
        SFXManager.Instance?.PlayPositive();
        RefreshCurrency();
    }

    public void BuyRefuelService()
    {
        if (currentOutpost == null
            || !currentOutpost.HasService(OutpostServiceType.Refuel))
        {
            return;
        }

        int price = GetOfferPrice(currentOutpost.refuelServiceCost);
        if (!RunManager.Instance.TrySpendMoney(price))
        {
            SFXManager.Instance?.PlayNegative();
            RefreshCurrency();
            return;
        }

        RunManager.Instance.AddFuel(currentOutpost.refuelServiceAmount);
        SFXManager.Instance?.PlayPositive();
        RefreshCurrency();
    }

    public void RerollStock()
    {
        if (currentOutpost == null
            || !currentOutpost.HasService(OutpostServiceType.Reroll))
        {
            return;
        }

        int price = GetOfferPrice(currentOutpost.rerollCost);
        if (!RunManager.Instance.TrySpendMoney(price))
        {
            SFXManager.Instance?.PlayNegative();
            RefreshCurrency();
            return;
        }

        SFXManager.Instance?.PlayPositive();
        PopulateItems();
    }
    
}
