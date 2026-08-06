using System.Collections.Generic;
using UnityEngine;

public class TarotManager : MonoBehaviour
{
    public static TarotManager Instance;

    [SerializeField] private TreasureDatabase tarotDatabase;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.05f;

    private const float MaximumBalancedSpawnChance = 0.05f;

    private void Awake()
    {
        Instance = this;

        // Existing scenes may still contain the old 20% Inspector override.
        // Clamp it at runtime so the rebalance applies before those scenes are resaved.
        if (spawnChance > MaximumBalancedSpawnChance)
        {
            Debug.LogWarning(
                $"Tarot spawn chance was {spawnChance:P0}; clamping it to {MaximumBalancedSpawnChance:P0} for the balanced economy.",
                this);
            spawnChance = MaximumBalancedSpawnChance;
        }
    }

    public bool TryOpenTarot()
    {
        if (Random.value > spawnChance)
        {
            return false;
        }

        OpenTarot();
        return true;
    }

    public void OpenTarot()
    {
        if (tarotDatabase == null || tarotDatabase.treasures == null)
        {
            Debug.LogWarning("Cannot open Tarot because no Tarot database is assigned.", this);
            return;
        }

        List<Treasure> availableCards = tarotDatabase.treasures.FindAll(card => card != null);

        if (availableCards.Count < 3)
        {
            Debug.LogWarning("Tarot requires at least three configured cards.", this);
            return;
        }

        Treasure card1 = TakeRandomCard(availableCards);
        Treasure card2 = TakeRandomCard(availableCards);
        Treasure card3 = TakeRandomCard(availableCards);

        // Play Tarot music
        MusicManager.Instance.PlayTarotMusic();

        TarotDialogPanel.Instance.Open(card1, card2, card3);
    }

    private Treasure TakeRandomCard(List<Treasure> availableCards)
    {
        int index = Random.Range(0, availableCards.Count);
        Treasure card = availableCards[index];
        availableCards.RemoveAt(index);

        return card;
    }
}
