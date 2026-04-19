using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class RoomUI : MonoBehaviour
{

    public Room room;
    public Image roomLogo;
    public TMP_Text tooltipText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        roomLogo.sprite = room.roomLogoSprite;   
        tooltipText.text = room.roomDescription;
    }
}
