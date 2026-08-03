using UnityEngine;

public static class PanelAnimation
{
    private static readonly int OpenTrigger = Animator.StringToHash("Open");
    private static readonly int CloseTrigger = Animator.StringToHash("Close");

    public static void Open(GameObject panel)
    {
        if (panel == null)
            return;

        panel.SetActive(true);
        SetInteraction(panel, true);

        Animator animator = panel.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{panel.name} needs an Animator to play its opening animation.", panel);
            return;
        }

        animator.ResetTrigger(CloseTrigger);
        animator.SetTrigger(OpenTrigger);
    }

    public static void Close(GameObject panel)
    {
        if (panel == null)
            return;

        SetInteraction(panel, false);

        Animator animator = panel.GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning($"{panel.name} needs an Animator to play its closing animation.", panel);
            panel.SetActive(false);
            return;
        }

        animator.ResetTrigger(OpenTrigger);
        animator.SetTrigger(CloseTrigger);
    }

    private static void SetInteraction(GameObject panel, bool enabled)
    {
        CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            return;

        canvasGroup.interactable = enabled;
        canvasGroup.blocksRaycasts = enabled;
    }
}
