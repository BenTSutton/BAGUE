using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EnemyShipStationUI : MonoBehaviour, IPointerEnterHandler
{
    protected EnemyShipStation station;
    [SerializeField] protected EnemyShipStationProfile stationProfile;
    
    [SerializeField] protected Image stationIcon;

    public event Action OnStationColourChanged;

    public static event Action<string> StationMessageRaised;

    public Sprite GetStationSprite => stationProfile != null ? stationProfile.icon : null;
    [SerializeField] private Button targetButton;

    protected void Awake()
    {
        station = GetComponent<EnemyShipStation>();

        if (targetButton == null)
        {
            targetButton = GetComponent<Button>();
        }
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

    protected void HandleBrokenStationUI()
    {
        ChangeColor(Color.red);

        if (targetButton != null)
        {
            targetButton.interactable = false;
        }

        StationMessageRaised?.Invoke(station.BrokenMessage);
    }

    protected void ChangeColor (Color color)
    {
        stationIcon.color = color;
        OnStationColourChanged?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (station != null && station.CanReceiveCannonShot)
            SFXManager.Instance?.PlayTargetHover();
    }
}
