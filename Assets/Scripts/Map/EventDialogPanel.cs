using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EventDialogPanel : MonoBehaviour
{
    public static EventDialogPanel Instance;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text titleText;
    public TMP_Text bodyText;
    public Button optionAButton;
    public Button optionBButton;
    public TMP_Text optionAText;
    public TMP_Text optionBText;
    public TMP_Text outcomeText;

    private NodeState currentState;
    private EventDefinition currentEvent;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(EventDefinition eventDefinition, NodeState state)
    {
        currentEvent = eventDefinition;
        currentState = state;

        titleText.text = eventDefinition.displayName;

        if (state.eventChoiceMade)
        {
            bodyText.text = state.resultSummary;
            optionAButton.gameObject.SetActive(false);
            optionBButton.gameObject.SetActive(false);
        }
        else
        {
            bodyText.text = eventDefinition.introText;

            optionAButton.gameObject.SetActive(eventDefinition.choices.Count > 0);
            optionBButton.gameObject.SetActive(eventDefinition.choices.Count > 1);

            if (eventDefinition.choices.Count > 0)
            {
                optionAText.text = eventDefinition.choices[0].choiceText;
            }

            if (eventDefinition.choices.Count > 1)
            {
                optionBText.text = eventDefinition.choices[1].choiceText;
            }
        }

        panel.SetActive(true);
    }

    public void ChooseOption(int optionIndex)
    {
        NodeResolutionResult result = currentEvent.ResolveChoice(currentState, optionIndex);

        optionAButton.gameObject.SetActive(false);
        optionBButton.gameObject.SetActive(false);
        optionAText.text = "";
        optionBText.text = "";

        outcomeText.text = result.summary;
        outcomeText.gameObject.SetActive(true);

        MapRunState.Instance.CompleteCurrentNodeAfterEvent(currentState.node);
        RefreshAllNodeViews();
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
}