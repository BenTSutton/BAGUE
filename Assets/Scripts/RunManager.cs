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
    public bool canSeeCombatsBeforeStarting = false;
    public bool nextFightHasOneHP = false;

    public bool inBossFight = false;

    private int originalFuel;
    private int originalMaxHealth;
    private int originalMoney;
    private int originalScrap;
    private int originalLevel;
    private int originalFuelCostToJump;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        SetOriginalVals();
    }

    void SetOriginalVals()
    {
        originalFuel = fuel;
        originalMaxHealth = maxShipHealth;
        originalMoney = money;
        originalScrap = scrap;
        originalLevel = level;
        originalFuelCostToJump = fuelCostToJump;
    }

    public void AddFuel(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyFuelGain(toAdd);
        }

        fuel += toAdd;
        SetLogForResource("Fuel", toAdd);
    }
    public void RemoveFuel(int toRemove)
    {
        int temp = fuel;
        temp -= toRemove;
        if(temp < 0)
        {
            temp = 0;
        }
        fuel = temp;
        SetLogForResource("Fuel", toRemove * -1);
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
        SetLogForResource("Ship Health", toAdd);
    }
    
    public void DamageShip(int toAdd)
    {
        int temp = currentShipHealth - toAdd;
        SetLogForResource("Ship Health", toAdd * -1);
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
        SetLogForResource("Max Ship Health", toAdd);
    }

    public void AddMoney(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyMoneyGain(toAdd);
        }

        money += toAdd;
        SetLogForResource("Credits", toAdd);
    }

    public bool RemoveMoney(int toRemove)
    {
        SetLogForResource("Credits", toRemove * -1);
        if (money < toRemove)
        {
            int removed = toRemove - money;
            money -= removed;
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
        SetLogForResource("Scrap", toAdd);
    }

    public void AddCrew(string crewName)
    {
        CrewMember crewMember = crewDatabase.GetByName(crewName);
        if (!activeCrew.Contains(crewMember))
            activeCrew.Add(crewMember);
        SetLogForCrew(crewName, true);
    }

    public void RemoveCrew(string crewName)
    {
        CrewMember crewMember = crewDatabase.GetByName(crewName);
        if (activeCrew.Contains(crewMember))
            activeCrew.Remove(crewMember);

        SetLogForCrew(crewName, false);
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

    public void EnterBoss()
    {
        inBossFight = true;
    }

    public void CompleteBossFight(bool wonFight)
    {
        inBossFight = false;

        if(!wonFight)
        {
            LoseGame();
            return;
        }

        if(CheckIfShouldAdvanceMap())
        {
            AdvanceMap();
        }
        else
        {
            WinGame();
        }
    }

    bool CheckIfShouldAdvanceMap()
    {
        return false;
    }

    void AdvanceMap()
    {
        //Advance map logic here
    }

    void WinGame()
    {
        // Win game logic here
        GameObject.Find("UIManager").GetComponent<UIManager>().victoryPanelObj.SetActive(true);
    }

    public void LoseGame()
    {
        GameObject.Find("UIManager").GetComponent<UIManager>().defeatPanelObj.SetActive(true);
    }

    public void Reset()
    {
        fuel = originalFuel;
        maxShipHealth = originalMaxHealth;
        money = originalMoney;
        scrap = originalScrap;
        fuelCostToJump = originalFuelCostToJump;
        level = originalLevel;
        activeCrew = new List<CrewMember>();
        canSeeCombatsBeforeStarting = false;
        nextFightHasOneHP = false;
    }
    
    void SetLogForResource(string resource, int amount)
    {
        GameObject.Find("ShowLog").GetComponent<ShowLog>().ConstructLogEntryForResource(resource, amount);
        GameObject.Find("ShowLog").GetComponent<ShowLog>().ShowTheLogWithSetTime(true);
    }

    void SetLogForCrew(string crew, bool gained)
    {
        GameObject.Find("ShowLog").GetComponent<ShowLog>().ConstructLogEntryForCrew(crew, gained);
        GameObject.Find("ShowLog").GetComponent<ShowLog>().ShowTheLogWithSetTime(true);
    }
}
