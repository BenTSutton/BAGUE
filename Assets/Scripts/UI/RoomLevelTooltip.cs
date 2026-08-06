using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class RoomLevelTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField, Min(1)] private int upgradeLevel = 1;
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private RoomUI roomUI;

    private void Awake()
    {
        if (roomUI == null)
        {
            roomUI = GetComponentInParent<RoomUI>();
        }

        HideTooltip();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltipObject == null || tooltipText == null || roomUI == null || roomUI.room == null)
        {
            return;
        }

        tooltipText.text = $"Level {upgradeLevel}: {roomUI.room.GetUpgradeDescription(upgradeLevel)}";
        tooltipObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }

    private void OnDisable()
    {
        HideTooltip();
    }

    private void HideTooltip()
    {
        if (tooltipObject != null)
        {
            tooltipObject.SetActive(false);
        }
    }
}
