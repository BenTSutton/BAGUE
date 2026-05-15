using UnityEngine;
using System.Collections.Generic;
using System;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    public int fuel;
    public int currentShipHealth;
    public int maxShipHealth;
    public int money;
    public int scrap;

    //For multiple levels, new maps etc
    public int level;

    public EnemyShip activeEnemyShip;
    // Assigned by cannon script to let other scripts know which cannon is firing
    public Cannon activeCannon;

    public List<CrewMember> activeCrew = new List<CrewMember>();
    public List<RoomInstance> shipRooms = new List<RoomInstance>();

    public CrewDatabase crewDatabase;

    public int fuelCostToJump = 5;

    public event Action OnHealthChange;


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
        OnHealthChange?.Invoke();
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
        OnHealthChange?.Invoke();
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

    public bool RemoveMoney(int toRemove)
    {
        if (money < toRemove)
        {
           return false;
        }
        else
        {
            money -= toRemove;
            return true;
        }
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

    public bool UpgradeRoom(Room room)
    {
        RoomInstance roomInstance = GetRoomInstance(room);

        if (roomInstance == null)
        {
            Debug.LogWarning("Room not found.");
            return false;
        }

        if (!roomInstance.CanUpgrade())
        {
            Debug.Log("Room cannot be upgraded.");
            return false;
        }

        int cost = roomInstance.GetUpgradeCost();

        if (scrap < cost)
        {
            Debug.Log("Not enough scrap.");
            return false;
        }

        scrap -= cost;
        roomInstance.Upgrade();

        Debug.Log(room.roomName + " upgraded to level " + roomInstance.level);
        return true;
    }

    public RoomInstance GetRoomInstance(Room room)
    {
        RoomInstance roomInstance = shipRooms.Find(r => r.roomData == room);
        return roomInstance;
    }
}
