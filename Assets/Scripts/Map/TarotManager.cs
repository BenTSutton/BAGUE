using System.Collections.Generic;
using UnityEngine;

public class TarotManager : MonoBehaviour
{
    public static TarotManager Instance;

    [SerializeField] private TreasureDatabase tarotDatabase;
    [SerializeField, Range(0f, 1f)] private float spawnChance = 0.2f;

    private void Awake()
    {
        Instance = this;
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
        List<Treasure> availableCards = new List<Treasure>(tarotDatabase.treasures);

        Treasure card1 = TakeRandomCard(availableCards);
        Treasure card2 = TakeRandomCard(availableCards);
        Treasure card3 = TakeRandomCard(availableCards);

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
