using UnityEngine;

public class WeaponsCannonConnector : MonoBehaviour
{
    [SerializeField] private RoomHealth weaponsRoomHealth;
    [SerializeField] private Cannon cannon;

    private void Awake()
    {
        if (weaponsRoomHealth == null)
        {
            weaponsRoomHealth = GetComponent<RoomHealth>();
        }
    }

    private void OnEnable()
    {
        if (weaponsRoomHealth == null)
        {
            Debug.LogWarning("[WeaponsCannonConnector] Weapons RoomHealth is missing.", this);

            return;
        }

        weaponsRoomHealth.Destroyed += HandleWeaponsRoomDestroyed;
    }

    private void Start()
    {
        if (cannon == null)
        {
            Debug.LogWarning("[WeaponsCannonConnector] Cannon reference is missing.", this);

            return;
        }

        if (weaponsRoomHealth == null)
        {
            return;
        }

        // Handles an already-destroyed room safely.
        cannon.SetReloadPaused(weaponsRoomHealth.IsDestroyed);
    }

    private void OnDisable()
    {
        if (weaponsRoomHealth != null)
        {
            weaponsRoomHealth.Destroyed -= HandleWeaponsRoomDestroyed;
        }
    }

    private void HandleWeaponsRoomDestroyed()
    {
        if (cannon == null)
        {
            Debug.LogWarning("[WeaponsCannonConnector] Cannot pause reload because " + "the Cannon reference is missing.", this);

            return;
        }

        cannon.SetReloadPaused(true);

        Debug.Log("[WeaponsCannonConnector] Weapons Room destroyed. " + "Cannon reload paused for this combat.", this);
    }

    private void OnValidate()
    {
        if (weaponsRoomHealth == null)
        {
            weaponsRoomHealth = GetComponent<RoomHealth>();
        }
    }
}