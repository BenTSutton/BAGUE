using UnityEngine;
using UnityEngine.EventSystems;

public class OpenTooltip : MonoBehaviour
{
    public GameObject tooltipObj;
    public bool isOpen = false;

    public RectTransform tooltipRect;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isOpen)
        {
            bool insideTooltip = RectTransformUtility.RectangleContainsScreenPoint(
                tooltipRect,
                Input.mousePosition,
                null
            );

            // Only close if click is outside
            if (!insideTooltip)
            {
                tooltipObj.SetActive(false);
                isOpen = false;
            }
        }
    }

    public void SetIsOpen()
    {
        isOpen = true;
    }
}