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
        station.OnStationBroken += HandleBrokenStation;
    }

    private void OnDisable()
    {
        station.OnStationBroken -= HandleBrokenStation;
    }

    protected void ApplyProfile()
    {
        if (stationProfile != null && stationIcon != null)
        {
            stationIcon.sprite = stationProfile.icon;
        }
    }

    protected void HandleBrokenStation ()
    {
        ChangeColor(Color.red);
    }

    protected void ChangeColor (Color color)
    {
        stationIcon.color = color;
    }
}
