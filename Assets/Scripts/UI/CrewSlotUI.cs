using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CrewSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text crewNameText;
    [SerializeField] private TMP_Text powerNameText;
    [SerializeField] private TMP_Text powerNameTextTooltip;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(CrewMember crew)
    {
        icon.sprite = crew.icon;
        crewNameText.text = crew.crewName;
        powerNameText.text = crew.powerName;
        powerNameTextTooltip.text = crew.powerName;
        descriptionText.text = crew.description;
        tooltip.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.SetActive(false);
    }
}
