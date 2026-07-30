using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TarotDialogPanel : MonoBehaviour
{
    public static TarotDialogPanel Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text davyText;

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

    private Treasure t1;
    private Treasure t2;
    private Treasure t3;

    public List<string> DavySayings;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(Treasure treasure1, Treasure treasure2, Treasure treasure3)
    {
        panel.SetActive(true);
        advanceButtonObj.SetActive(false);
        t1ChooseButtonObj.SetActive(true);
        t2ChooseButtonObj.SetActive(true);
        t3ChooseButtonObj.SetActive(true);
        t1ChosenObj.SetActive(false);
        t2ChosenObj.SetActive(false);
        t3ChosenObj.SetActive(false);
        SetDavySaying();
        t1 = treasure1;
        t2 = treasure2;
        t3 = treasure3;
        SetupTreasure1();
        SetupTreasure2();
        SetupTreasure3();
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    void RefreshAllNodeViews()
    {
        foreach (var view in FindObjectsOfType<NodeView>())
        {
            view.UpdateColour();
        }
    }

    public void ChooseTreasure(int chosenNum)
    {
        switch(chosenNum)
        {
            case 1:
                t1.ApplyEffect();
                break;
            case 2:
                t2.ApplyEffect();
                break;
            case 3:
                t3.ApplyEffect();
                break;
        }
    }

    void SetupTreasure1()
    {
        SetupTreasure(t1, item1Image, item1Rarity, item1NameText, item1Text);
    }

    void SetupTreasure2()
    {
        SetupTreasure(t2, item2Image, item2Rarity, item2NameText, item2Text);
    }

    void SetupTreasure3()
    {
        SetupTreasure(t3, item3Image, item3Rarity, item3NameText, item3Text);
    }

    void SetupTreasure(Treasure t, Image img, TMP_Text rarityText, TMP_Text nameText, TMP_Text descText)
    {
        img.sprite = t.icon;
        rarityText.text = t.rarity.ToString();
        nameText.text = t.treasureName;
        descText.text = t.description;
    }

    void SetDavySaying()
    {
        int randomIndex = Random.Range(0, DavySayings.Count);
        davyText.text = DavySayings[randomIndex];
    }

}