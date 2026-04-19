using UnityEngine;
using System.Collections.Generic;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    public int fuel;
    public int currentShipHealth;
    public int maxShipHealth;
    public int money;
    public int scrap;

    public List<CrewMember> activeCrew = new List<CrewMember>();

    public CrewDatabase crewDatabase;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void AddFuel(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyFuelGain(toAdd);
        }

        fuel += toAdd;
    }

    public void AddHealth(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyHealing(toAdd);
        }
                
        int temp = currentShipHealth + toAdd;
        if (temp > maxShipHealth)
        {
            temp = maxShipHealth;
        }
        currentShipHealth = temp;
    }

    public void DamageShip(int toAdd)
    {
        int temp = currentShipHealth - toAdd;
        if (temp <= 0)
        {
            Debug.Log("SHOULD DIE, SHIP DESTROYED");
            temp = 0;
            //Death logic 
        }
        currentShipHealth = temp;
    }

    public void AddMaxHealth(int toAdd)
    {
        maxShipHealth += toAdd;
        currentShipHealth += toAdd;
    }

    public void AddMoney(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyMoneyGain(toAdd);
        }

        money += toAdd;
    }
    
    public void AddScrap(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyScrapGain(toAdd);
        }

        scrap += toAdd;
    }

    public void AddCrew(string crewName)
    {
        CrewMember crewMember = crewDatabase.GetByName(crewName);
        if (!activeCrew.Contains(crewMember))
            activeCrew.Add(crewMember);
    }

    public void RemoveCrew(string crewName)
    {
        CrewMember crewMember = crewDatabase.GetByName(crewName);
        if (activeCrew.Contains(crewMember))
            activeCrew.Remove(crewMember);
    }
}
