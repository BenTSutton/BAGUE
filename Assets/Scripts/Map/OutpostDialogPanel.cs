using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OutpostDialogPanel : MonoBehaviour
{
    public static OutpostDialogPanel Instance;

    [Header("UI")]
    public GameObject panel;

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

    private NodeState currentState;
    private OutpostDefinition currentEvent;

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
    }

    public void Close()
    {
        panel.SetActive(false);
        MapRunState.Instance.CompleteCurrentNodeAfterEvent(currentState.node);
    }

    void RefreshAllNodeViews()
    {
        foreach (var view in FindObjectsOfType<NodeView>())
        {
            view.UpdateColour();
        }
    }
}