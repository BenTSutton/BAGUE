using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PassiveEffectDisplay : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;

    public void Setup(PersistentTreasureEffect effect)
    {
        icon.sprite = effect.icon;
        nameText.text = effect.DisplayName;
        descriptionText.text = effect.description;
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

    private void OnDisable()
    {
        tooltip.SetActive(false);
    }
}