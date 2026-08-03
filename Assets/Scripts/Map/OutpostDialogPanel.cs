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
    public TMP_Text scrapText;

    public Image creditItem1Image;
    public Image creditItem2Image;
    public Image scrapItem1Image;
    public Image scrapItem2Image;
    public TMP_Text creditItem1Text;
    public TMP_Text creditItem2Text;
    public TMP_Text scrapItem1Text;
    public TMP_Text scrapItem2Text;
    public TMP_Text creditItem1CostText;
    public TMP_Text creditItem2CostText;
    public TMP_Text scrapItem1CostText;
    public TMP_Text scrapItem2CostText;
    public TMP_Text item1ButtonText;
    public TMP_Text item2ButtonText;
    public TMP_Text item3ButtonText;
    public TMP_Text item4ButtonText;
    public Button item1Button;
    public Button item2Button;
    public Button item3Button;
    public Button item4Button;

    public TreasureDatabase treasureDatabase;
    public CrewDatabase crewDatabase;

    private NodeState currentState;
    private OutpostDefinition currentEvent;

    private Treasure t1;
    private Treasure t2;
    private CrewMember c1;
    private CrewMember c2;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(OutpostDefinition outpostDefinition, NodeState state)
    {
        currentEvent = outpostDefinition;
        currentState = state;

        panel.SetActive(true);
        RefreshCurrency();
        PopulateItems();
    }

    public void Close()
    {
        panel.SetActive(false);
        MapRunState.Instance.CompleteCurrentNodeAfterEvent(currentState.node);
    }

    void RefreshCurrency()
    {
        creditText.text = "Credits: " + RunManager.Instance.money.ToString();
        scrapText.text = "Scrap: " + RunManager.Instance.scrap.ToString();
    }

    void PopulateItems()
    {
        t1 = treasureDatabase.GetRandomPurchasableCommonTreasure(null);
        t2 = treasureDatabase.GetRandomPurchasableCommonTreasure(
            new[] { t1 },
            t1 != null ? new[] { t1.type } : null);

        List<CrewMember> unavailableCrew = new List<CrewMember>(RunManager.Instance.activeCrew);
        c1 = crewDatabase.GetRandomPurchasableCrew(unavailableCrew);
        if (c1 != null)
            unavailableCrew.Add(c1);
        c2 = crewDatabase.GetRandomPurchasableCrew(unavailableCrew);

        ResetPurchaseButton(item1Button, item1ButtonText, t1 != null);
        ResetPurchaseButton(item2Button, item2ButtonText, t2 != null);
        ResetPurchaseButton(item3Button, item3ButtonText, c1 != null);
        ResetPurchaseButton(item4Button, item4ButtonText, c2 != null);

        UpdateT1Slot();
        UpdateT2Slot();
        UpdateC1Slot();
        UpdateC2Slot();
    }

    void UpdateT1Slot()
    {
        if (t1 == null)
        {
            SetUnavailableSlot(creditItem1Image, creditItem1Text, creditItem1CostText);
            return;
        }
        creditItem1Image.sprite = t1.icon;
        creditItem1Text.text = t1.description;
        creditItem1CostText.text = "Buy: " + t1.price.ToString() + " Credits";
    }

    void UpdateT2Slot()
    {
        if (t2 == null)
        {
            SetUnavailableSlot(creditItem2Image, creditItem2Text, creditItem2CostText);
            return;
        }
        creditItem2Image.sprite = t2.icon;
        creditItem2Text.text = t2.description;
        creditItem2CostText.text = "Buy: " + t2.price.ToString() + " Credits";
    }

    void UpdateC1Slot()
    {
        if (c1 == null)
        {
            SetUnavailableSlot(scrapItem1Image, scrapItem1Text, scrapItem1CostText);
            return;
        }
        scrapItem1Image.sprite = c1.icon;
        scrapItem1Text.text = c1.description;
        scrapItem1CostText.text = "Buy: " + c1.price.ToString() + " Credits";
    }

    void UpdateC2Slot()
    {
        if (c2 == null)
        {
            SetUnavailableSlot(scrapItem2Image, scrapItem2Text, scrapItem2CostText);
            return;
        }
        scrapItem2Image.sprite = c2.icon;
        scrapItem2Text.text = c2.description;
        scrapItem2CostText.text = "Buy: " + c2.price.ToString() + " Credits";
    }

    void ResetPurchaseButton(Button button, TMP_Text buttonText, bool hasItem)
    {
        button.interactable = hasItem;
        buttonText.text = hasItem ? "Buy" : "Unavailable";
    }

    void SetUnavailableSlot(Image image, TMP_Text description, TMP_Text cost)
    {
        image.sprite = null;
        description.text = "No item available";
        cost.text = string.Empty;
    }

    public void BuyItem1()
    {
        if(RunManager.Instance.RemoveMoney(t1.price))
        {
            t1.ApplyEffect();
            item1ButtonText.text = "Purchased!";
            item1Button.interactable = false;
        }
        RefreshCurrency();
    }
    public void BuyItem2()
    {
        if(RunManager.Instance.RemoveMoney(t2.price))
        {
            t2.ApplyEffect();
            item2ButtonText.text = "Purchased!";
            item2Button.interactable = false;
        }
        RefreshCurrency();
    }
    public void BuyItem3()
    {
        if(RunManager.Instance.RemoveMoney(c1.price))
        {
            RunManager.Instance.AddCrew(c1.crewName);
            item3ButtonText.text = "Purchased!";
            item3Button.interactable = false;
        }
        RefreshCurrency();
    }
    public void BuyItem4()
    {
        if(RunManager.Instance.RemoveMoney(c2.price))
        {
            RunManager.Instance.AddCrew(c2.crewName);
            item4ButtonText.text = "Purchased!";
            item4Button.interactable = false;
        }
        RefreshCurrency();
    }
    
}
