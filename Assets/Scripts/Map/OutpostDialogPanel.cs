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
        t1 = treasureDatabase.GetRandomCommonTreasure();
        t2 = treasureDatabase.GetRandomCommonTreasure();
        c1 = crewDatabase.GetRandomCrew();
        c2 = crewDatabase.GetRandomCrew();

        UpdateT1Slot();
        UpdateT2Slot();
        UpdateC1Slot();
        UpdateC2Slot();
    }

    void UpdateT1Slot()
    {
        creditItem1Image.sprite = t1.icon;
        creditItem1Text.text = t1.description;
        creditItem1CostText.text = "Buy: " + t1.price.ToString() + " Credits";
    }

    void UpdateT2Slot()
    {
        creditItem2Image.sprite = t2.icon;
        creditItem2Text.text = t2.description;
        creditItem2CostText.text = "Buy: " + t2.price.ToString() + " Credits";
    }

    void UpdateC1Slot()
    {
        scrapItem1Image.sprite = c1.icon;
        scrapItem1Text.text = c1.description;
        scrapItem1CostText.text = "Buy: " + c1.price.ToString() + " Credits";
    }

    void UpdateC2Slot()
    {
        scrapItem2Image.sprite = c2.icon;
        scrapItem2Text.text = c2.description;
        scrapItem2CostText.text = "Buy: " + c2.price.ToString() + " Credits";
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