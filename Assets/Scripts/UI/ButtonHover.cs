using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHover : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerDownHandler,
    IPointerUpHandler,
    ISelectHandler,
    IDeselectHandler
{
    private RectTransform rectTransform;
    private Vector3 restingScale;
    private float hoverScale = 1.045f;
    private bool highlighted;
    private bool pressed;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        restingScale = rectTransform.localScale;
    }

    public void SetHoverScale(float scale)
    {
        hoverScale = scale;
    }

    private void Update()
    {
        float scale = pressed
            ? 0.975f
            : highlighted ? hoverScale : 1f;

        Vector3 targetScale = restingScale * scale;

        float blend =
            1f - Mathf.Exp(-14f * Time.unscaledDeltaTime);

        rectTransform.localScale = Vector3.Lerp(
            rectTransform.localScale,
            targetScale,
            blend);
    }

    public void OnPointerEnter(PointerEventData eventData)
        => highlighted = true;

    public void OnPointerExit(PointerEventData eventData)
    {
        highlighted = false;
        pressed = false;
    }

    public void OnPointerDown(PointerEventData eventData)
        => pressed = true;

    public void OnPointerUp(PointerEventData eventData)
        => pressed = false;

    public void OnSelect(BaseEventData eventData)
        => highlighted = true;

    public void OnDeselect(BaseEventData eventData)
    {
        highlighted = false;
        pressed = false;
    }
}