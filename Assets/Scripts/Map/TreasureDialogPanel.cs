using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TreasureDialogPanel : MonoBehaviour
{
    public static TreasureDialogPanel Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text descriptionText;

    public GameObject item1Obj;
    public GameObject item2Obj;
    public GameObject item3Obj;
    public TMP_Text item1Rarity;
    public TMP_Text item2Rarity;
    public TMP_Text item3Rarity;

    public Image item1Image;
    public Image item2Image;
    public Image item3Image;
    public TMP_Text item1NameText;
    public TMP_Text item2NameText;
    public TMP_Text item3NameText;
    public TMP_Text item1Text;
    public TMP_Text item2Text;
    public TMP_Text item3Text;
    public GameObject t1ChooseButtonObj;
    public GameObject t2ChooseButtonObj;
    public GameObject t3ChooseButtonObj;
    public GameObject t1ChosenObj;
    public GameObject t2ChosenObj;
    public GameObject t3ChosenObj;
    public GameObject advanceButtonObj;

    private NodeState currentState;
    private Treasure treasure1;
    private Treasure treasure2;
    private Treasure treasure3;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(TreasureDefinition treasureDefinition, NodeState state, 
                     Treasure treasure1, Treasure treasure2, Treasure treasure3)
    {
        currentState = state;
        this.treasure1 = treasure1;
        this.treasure2 = treasure2;
        this.treasure3 = treasure3;

        PanelAnimation.Open(panel);
        advanceButtonObj.SetActive(false);
        t1ChooseButtonObj.SetActive(true);
        t2ChooseButtonObj.SetActive(true);
        t3ChooseButtonObj.SetActive(true);
        t1ChosenObj.SetActive(false);
        t2ChosenObj.SetActive(false);
        t3ChosenObj.SetActive(false);
        SetupTreasure(treasure1, item1Image, item1Rarity, item1NameText, item1Text, t1ChooseButtonObj);
        SetupTreasure(treasure2, item2Image, item2Rarity, item2NameText, item2Text, t2ChooseButtonObj);
        SetupTreasure(treasure3, item3Image, item3Rarity, item3NameText, item3Text, t3ChooseButtonObj);
    }

    public void Close()
    {
        PanelAnimation.Close(panel);
        MapRunState.Instance.CompleteCurrentNodeAfterEvent(currentState.node);
    }

    public void ChooseTreasure(int choiceNumber)
    {
        Treasure chosenTreasure = choiceNumber switch
        {
            1 => treasure1,
            2 => treasure2,
            3 => treasure3,
            _ => null
        };
        TMP_Text chosenDescription = choiceNumber switch
        {
            1 => item1Text,
            2 => item2Text,
            3 => item3Text,
            _ => null
        };

        if (chosenTreasure != null
            && !chosenTreasure.TryApplyEffect(out string resultMessage)
            && chosenDescription != null)
        {
            chosenDescription.text = resultMessage;
        }
    }

    void SetupTreasure(
        Treasure treasure,
        Image image,
        TMP_Text rarityText,
        TMP_Text nameText,
        TMP_Text descriptionText,
        GameObject chooseButtonObject)
    {
        if (treasure == null)
        {
            chooseButtonObject.SetActive(false);
            return;
        }

        chooseButtonObject.SetActive(true);
        image.sprite = treasure.icon;
        rarityText.text = treasure.rarity.ToString();
        nameText.text = treasure.treasureName;
        descriptionText.text = treasure.description;

        Button chooseButton = chooseButtonObject.GetComponent<Button>();
        if (chooseButton == null)
            return;

        chooseButton.interactable = true;
        if (treasure.type != TreasureType.Crew)
            return;

        CrewAcquisitionResult availability =
            RunManager.Instance.GetCrewAcquisitionAvailability(treasure.crewReward);
        if (availability == CrewAcquisitionResult.Success)
            return;

        chooseButton.interactable = false;
        descriptionText.text += "\n" + RunManager.Instance.GetCrewAcquisitionMessage(
            availability,
            treasure.crewReward);
    }

}
