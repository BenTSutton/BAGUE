using UnityEngine;

public class WeaponsCannonConnector : MonoBehaviour
{
    [SerializeField] private RoomHealth weaponsRoomHealth;
    [SerializeField] private Cannon cannon;

    [SerializeField, Range(0.1f, 1f)]
    private float destroyedReloadRateMultiplier = 0.5f;

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

        cannon.SetReloadRateMultiplier(
            weaponsRoomHealth.IsDestroyed
                ? destroyedReloadRateMultiplier
                : 1f);
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
            Debug.LogWarning("[WeaponsCannonConnector] Cannot slow reload because " + "the Cannon reference is missing.", this);

            return;
        }

        cannon.SetReloadRateMultiplier(
            destroyedReloadRateMultiplier);

        Debug.Log("[WeaponsCannonConnector] Weapons Room destroyed. " + "Cannon reload slowed for this combat.", this);
    }

    private void OnValidate()
    {
        if (weaponsRoomHealth == null)
        {
            weaponsRoomHealth = GetComponent<RoomHealth>();
        }

        destroyedReloadRateMultiplier = Mathf.Clamp(
            destroyedReloadRateMultiplier,
            0.1f,
            1f);
    }
}
