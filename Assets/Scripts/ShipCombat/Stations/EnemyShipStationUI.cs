using System;
using UnityEngine;
using UnityEngine.UI;

public class EnemyShipStationUI : MonoBehaviour
{
    protected EnemyShipStation station;
    [SerializeField] protected EnemyShipStationProfile stationProfile;
    
    [SerializeField] protected Image stationIcon;

    public Sprite GetStationSprite => stationProfile != null ? stationProfile.icon : null;

    protected virtual void Awake()
    {
        station = GetComponent<EnemyShipStation>();
    }
    
    protected virtual void Start()
    {
        ApplyProfile();
    }

    private void OnEnable()
    {
        // Subscribe to the event
        station.OnStationBroken += HandleBrokenStationUI;
    }

    private void OnDisable()
    {
        station.OnStationBroken -= HandleBrokenStationUI;
    }

    protected void ApplyProfile()
    {
        if (stationProfile != null && stationIcon != null)
        {
            stationIcon.sprite = stationProfile.icon;
        }
    }

    protected void HandleBrokenStationUI ()
    {
        ChangeColor(Color.red);
    }

    protected void ChangeColor (Color color)
    {
        stationIcon.color = color;
    }
}
