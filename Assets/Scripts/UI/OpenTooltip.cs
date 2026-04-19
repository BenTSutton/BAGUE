using UnityEngine;

public class OpenTooltip : MonoBehaviour
{
    public GameObject tooltipObj;
    public bool isOpen = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && isOpen) 
        {
            if (tooltipObj != null)
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