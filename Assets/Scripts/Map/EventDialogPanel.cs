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
            outcomeText.text = string.Empty;
            outcomeText.gameObject.SetActive(false);
            optionAButton.gameObject.SetActive(false);
            optionBButton.gameObject.SetActive(false);
        }
        else
        {
            bodyText.text = eventDefinition.introText;
            outcomeText.text = string.Empty;
            outcomeText.gameObject.SetActive(false);

            ConfigureChoiceButton(
                optionAButton,
                optionAText,
                eventDefinition.choices,
                0);
            ConfigureChoiceButton(
                optionBButton,
                optionBText,
                eventDefinition.choices,
                1);
        }

        PanelAnimation.Open(panel);
    }

    public void ChooseOption(int optionIndex)
    {
        NodeResolutionResult result = currentEvent.ResolveChoice(currentState, optionIndex);

        if (!currentState.eventChoiceMade)
        {
            outcomeText.text = result.summary;
            outcomeText.gameObject.SetActive(true);
            return;
        }

        optionAButton.gameObject.SetActive(false);
        optionBButton.gameObject.SetActive(false);
        optionAText.text = "";
        optionBText.text = "";

        outcomeText.text = result.summary;
        outcomeText.gameObject.SetActive(true);

        RefreshAllNodeViews();
    }

    void ConfigureChoiceButton(
        Button button,
        TMP_Text buttonText,
        System.Collections.Generic.IList<EventChoice> choices,
        int index)
    {
        bool hasChoice = choices != null && index >= 0 && index < choices.Count;
        button.gameObject.SetActive(hasChoice);

        if (!hasChoice)
            return;

        EventChoice choice = choices[index];
        if (choice == null)
        {
            button.interactable = false;
            buttonText.text = "Choice unavailable\n<color=#FF7777>Missing configuration</color>";
            return;
        }

        bool unlocked = choice.RequirementsMet(
            RunManager.Instance,
            out string failureReason);
        button.interactable = unlocked;
        buttonText.text = unlocked
            ? choice.choiceText
            : $"{choice.choiceText}\n<color=#FF7777>Requires: {failureReason}</color>";
    }

    public void CompleteNode()
    {
        MapRunState.Instance.CompleteCurrentNodeAfterEvent(currentState.node);
        RefreshAllNodeViews();
    }

    public void Close()
    {
        PanelAnimation.Close(panel);
    }

    void RefreshAllNodeViews()
    {
        foreach (NodeView view in FindObjectsByType<NodeView>(FindObjectsSortMode.None))
        {
            view.UpdateColour();
        }
    }
}
