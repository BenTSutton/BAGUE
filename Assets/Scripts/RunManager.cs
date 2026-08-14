using UnityEngine;
using System.Collections.Generic;
using System;

public enum CrewAcquisitionResult
{
    Success,
    InvalidCrew,
    UnknownCrew,
    AlreadyOwned,
    RosterFull,
    InsufficientCredits
}

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    public int fuel;
    public int currentShipHealth;
    public int maxShipHealth;
    public int money;
    public int scrap;
    public int fuelCostToJump = 5;
    [Min(1)] public int crewCapacity = 4;

    //For multiple levels, new maps etc
    public int level;

    private float shipDodgeChance = 0;
    private float additionalDodgeChanceFromCloak = 100f;

    public EnemyShip activeEnemyShip;
    
    // Assigned by cannon script to let other scripts know which cannon is firing
    public Cannon activeCannon;

    public List<CrewMember> activeCrew = new List<CrewMember>();
    public List<string> runFlags = new List<string>();
    public List<PersistentTreasureEffect> activeTreasureEffects =
        new List<PersistentTreasureEffect>();
    public List<RoomInstance> shipRooms = new List<RoomInstance>();

    public CrewDatabase crewDatabase;
    public event Action OnHealthChange;

    public static event Action OnPlayerShipDestroyed;
    public bool LastShipHitWasNegated { get; private set; }

    public bool isCloaked = false;
    public bool canSeeCombatsBeforeStarting = false;
    public bool nextFightHasOneHP = false;

    public bool inBossFight = false;

    private int originalFuel;
    private int originalMaxHealth;
    private int originalMoney;
    private int originalScrap;
    private int originalLevel;
    private int originalFuelCostToJump;
    private List<int> originalRoomLevels = new List<int>();
    private bool mechanicEmergencyRepairUsed;
    private bool shieldFirstHitUsed;
    private int cannonShotsFiredThisCombat;
    private readonly HashSet<PersistentTreasureEffect>
        usedTreasureFirstHitProtectionEffects =
            new HashSet<PersistentTreasureEffect>();

    public EnemyFactionProfile enemyFaction;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        EnsureRunCollections();

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

        originalRoomLevels.Clear();

        foreach (RoomInstance room in shipRooms)
        {
            originalRoomLevels.Add(room.level);
        }
    }

    public void AddFuel(int toAdd)
    {
        if (toAdd <= 0)
        {
            Debug.LogWarning("AddFuel only accepts a positive amount. Use RemoveFuel for fuel losses.");
            return;
        }

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyFuelGain(toAdd);
        }

        fuel += toAdd;
        SetLogForResource("Fuel", toAdd);
    }

    public void RemoveFuel(int toRemove)
    {
        if (toRemove <= 0)
        {
            Debug.LogWarning("RemoveFuel only accepts a positive amount.");
            return;
        }

        fuel = Mathf.Max(0, fuel);
        int removed = Mathf.Min(fuel, toRemove);
        fuel -= removed;

        if (removed > 0)
            SetLogForResource("Fuel", -removed);
    }

    public void AddHealth(int toAdd)
    {
        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
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
        LastShipHitWasNegated = false;
        bool shieldsOperational = IsRoomOperational<ShieldRoom>();
        ShieldRoom shieldRoom = GetRoomData<ShieldRoom>();
        int shieldLevel = shieldsOperational
            ? GetRoomLevel<ShieldRoom>()
            : 0;

        if (shieldsOperational &&
            !shieldFirstHitUsed &&
            shieldRoom.NegatesFirstHit(shieldLevel))
        {
            shieldFirstHitUsed = true;
            LastShipHitWasNegated = true;
            Debug.Log("Shield Room negated the first hull hit.");
            SFXManager.Instance?.PlayPlayerShieldHit();
            return;
        }

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect == null
                || usedTreasureFirstHitProtectionEffects.Contains(effect)
                || !effect.PreventsFirstHit(toAdd))
            {
                continue;
            }

            usedTreasureFirstHitProtectionEffects.Add(effect);
            LastShipHitWasNegated = true;
            Debug.Log($"{effect.DisplayName} prevented the first hull hit.");
            return;
        }

        if (shieldsOperational)
        {
            toAdd = shieldRoom.ModifyIncomingDamage(toAdd, shieldLevel);
        }
        else
        {
            toAdd += 1;
        }

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyDamageTaken(toAdd);
        }

        toAdd = Mathf.Max(0, toAdd);
        int temp = currentShipHealth - toAdd;
        SetLogForResource("Ship Health", toAdd * -1);
        if (temp <= 0)
        {
            MechanicRoom mechanicRoom = GetRoomData<MechanicRoom>();
            int mechanicLevel = GetRoomLevel<MechanicRoom>();
            bool mechanicOperational = IsRoomOperational<MechanicRoom>();
            int emergencyHealth = mechanicOperational
                ? mechanicRoom.GetEmergencyRestoreHealth(mechanicLevel)
                : 0;

            if (!mechanicEmergencyRepairUsed && emergencyHealth > 0)
            {
                mechanicEmergencyRepairUsed = true;
                currentShipHealth = Mathf.Min(emergencyHealth, maxShipHealth);
                OnHealthChange?.Invoke();
                Debug.Log("Mechanic Room prevented lethal ship damage.");
                return;
            }

            Debug.Log("SHOULD DIE, SHIP DESTROYED");
            OnPlayerShipDestroyed?.Invoke();
        }
        currentShipHealth = temp;
        OnHealthChange?.Invoke();
    }

    public bool CheckIfDodged()
    {
        float currentDodgeChance = shipDodgeChance;

        EngineRoom engineRoom = GetRoomData<EngineRoom>();

        if (IsRoomOperational<EngineRoom>())
        {
            currentDodgeChance += engineRoom.GetDodgeChance(
                GetRoomLevel<EngineRoom>());
        }

        HelmRoom helmRoom = GetRoomData<HelmRoom>();

        if (IsRoomOperational<HelmRoom>())
        {
            currentDodgeChance += helmRoom.GetDodgeChance(
                GetRoomLevel<HelmRoom>());
        }

        float randomRoll = UnityEngine.Random.Range(0f, 100f);

        if (isCloaked)
        {
            currentDodgeChance += additionalDodgeChanceFromCloak;
        }

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                currentDodgeChance = crew.crewEffect.ModifyDodgeChance(currentDodgeChance);
        }
        
        return currentDodgeChance >= randomRoll;
    }

    public void AddMaxHealth(int toAdd)
    {
        maxShipHealth += toAdd;
        currentShipHealth += toAdd;
        SetLogForResource("Max Ship Health", toAdd);
        OnHealthChange?.Invoke();
    }

    public void AddMoney(int toAdd)
    {
        AddMoneyInternal(toAdd, true);
    }

    private void AddMoneyInternal(int toAdd, bool applyBunkBonus)
    {
        if (toAdd <= 0)
        {
            Debug.LogWarning("AddMoney only accepts a positive amount. Use LoseMoney or TrySpendMoney for credit losses.");
            return;
        }

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyMoneyGain(toAdd);
        }

        if (applyBunkBonus && IsRoomOperational<BunkRoom>())
        {
            BunkRoom bunkRoom = GetRoomData<BunkRoom>();
            toAdd = bunkRoom.ModifyMoneyGain(
                toAdd,
                GetRoomLevel<BunkRoom>());
        }

        money += toAdd;
        SetLogForResource("Credits", toAdd);
    }

    public void LoseMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("LoseMoney only accepts a positive amount.");
            return;
        }

        money = Mathf.Max(0, money);
        int lost = Mathf.Min(money, amount);
        money -= lost;

        if (lost > 0)
            SetLogForResource("Credits", -lost);
    }

    public bool TrySpendMoney(int cost)
    {
        if (cost < 0)
        {
            Debug.LogWarning("Credit costs cannot be negative.");
            return false;
        }

        if (money < cost)
            return false;

        money -= cost;

        if (cost > 0)
            SetLogForResource("Credits", -cost);

        return true;
    }

    // Kept for existing UnityEvents and older callers. New purchases should use TrySpendMoney.
    public bool RemoveMoney(int toRemove)
    {
        return TrySpendMoney(toRemove);
    }
    
    public void AddScrap(int toAdd)
    {
        AddScrapInternal(toAdd, true);
    }

    private void AddScrapInternal(int toAdd, bool applyBunkBonus)
    {
        if (toAdd <= 0)
        {
            Debug.LogWarning("AddScrap only accepts a positive amount. Use LoseScrap or TrySpendScrap for scrap losses.");
            return;
        }

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                toAdd = crew.crewEffect.ModifyScrapGain(toAdd);
        }

        if (applyBunkBonus && IsRoomOperational<BunkRoom>())
        {
            BunkRoom bunkRoom = GetRoomData<BunkRoom>();
            toAdd = bunkRoom.ModifyScrapGain(
                toAdd,
                GetRoomLevel<BunkRoom>());
        }

        scrap += toAdd;
        SetLogForResource("Scrap", toAdd);
    }

    public void LoseScrap(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("LoseScrap only accepts a positive amount.");
            return;
        }

        scrap = Mathf.Max(0, scrap);
        int lost = Mathf.Min(scrap, amount);
        scrap -= lost;

        if (lost > 0)
            SetLogForResource("Scrap", -lost);
    }

    public bool TrySpendScrap(int cost)
    {
        if (cost < 0)
        {
            Debug.LogWarning("Scrap costs cannot be negative.");
            return false;
        }

        if (scrap < cost)
            return false;

        scrap -= cost;

        if (cost > 0)
            SetLogForResource("Scrap", -cost);

        return true;
    }

    public CrewAcquisitionResult TryRecruitCrew(string crewName, bool allowDuplicate = false)
    {
        if (crewDatabase == null)
        {
            Debug.LogError("Cannot add crew because RunManager has no CrewDatabase assigned.");
            return CrewAcquisitionResult.UnknownCrew;
        }

        CrewMember crewMember = crewDatabase.GetByName(crewName);
        if (crewMember == null)
        {
            Debug.LogWarning($"Cannot add unknown crew member '{crewName}'.");
            return CrewAcquisitionResult.UnknownCrew;
        }

        return TryRecruitCrew(crewMember, allowDuplicate);
    }

    public CrewAcquisitionResult TryRecruitCrew(
        CrewMember crewMember,
        bool allowDuplicate = false)
    {
        CrewAcquisitionResult availability = GetCrewAcquisitionAvailability(
            crewMember,
            allowDuplicate);

        if (availability != CrewAcquisitionResult.Success)
            return availability;

        AddCrewUnchecked(crewMember);
        return CrewAcquisitionResult.Success;
    }

    public CrewAcquisitionResult TryPurchaseCrew(CrewMember crewMember, int basePrice)
    {
        CrewAcquisitionResult availability = GetCrewAcquisitionAvailability(crewMember);

        if (availability != CrewAcquisitionResult.Success)
            return availability;

        int price = GetPurchasePrice(basePrice);
        if (!TrySpendMoney(price))
            return CrewAcquisitionResult.InsufficientCredits;

        AddCrewUnchecked(crewMember);
        return CrewAcquisitionResult.Success;
    }

    public CrewAcquisitionResult GetCrewAcquisitionAvailability(
        CrewMember crewMember,
        bool allowDuplicate = false)
    {
        if (crewMember == null)
            return CrewAcquisitionResult.InvalidCrew;

        EnsureRunCollections();
        activeCrew.RemoveAll(crew => crew == null);

        if (!allowDuplicate && activeCrew.Contains(crewMember))
            return CrewAcquisitionResult.AlreadyOwned;

        if (activeCrew.Count >= MaxCrewCapacity)
            return CrewAcquisitionResult.RosterFull;

        return CrewAcquisitionResult.Success;
    }

    public string GetCrewAcquisitionMessage(
        CrewAcquisitionResult result,
        CrewMember crewMember = null)
    {
        string crewName = crewMember != null ? crewMember.crewName : "That crew member";

        return result switch
        {
            CrewAcquisitionResult.Success => $"{crewName} joined the crew.",
            CrewAcquisitionResult.AlreadyOwned => $"{crewName} is already in your crew.",
            CrewAcquisitionResult.RosterFull =>
                $"Crew roster full ({ActiveCrewCount}/{MaxCrewCapacity}). Nobody was replaced.",
            CrewAcquisitionResult.InsufficientCredits =>
                "Not enough Credits to recruit this crew member.",
            CrewAcquisitionResult.UnknownCrew =>
                "That crew member is not present in the crew database.",
            _ => "That crew member is not configured correctly."
        };
    }

    public int MaxCrewCapacity => Mathf.Max(1, crewCapacity);

    public int ActiveCrewCount
    {
        get
        {
            if (activeCrew == null)
                return 0;

            int count = 0;
            foreach (CrewMember crew in activeCrew)
            {
                if (crew != null)
                    count++;
            }

            return count;
        }
    }

    public int AvailableCrewSlots => Mathf.Max(0, MaxCrewCapacity - ActiveCrewCount);
    public bool HasCrewCapacity => AvailableCrewSlots > 0;

    public bool AddPersistentTreasureEffect(PersistentTreasureEffect effect)
    {
        if (effect == null)
            return false;

        EnsureRunCollections();
        activeTreasureEffects.RemoveAll(activeEffect => activeEffect == null);
        if (activeTreasureEffects.Contains(effect))
            return false;

        activeTreasureEffects.Add(effect);
        return true;
    }

    public bool TryProtectFromEventEffect(
        EventEffectData eventEffect,
        out string protectionName)
    {
        protectionName = string.Empty;

        if (eventEffect == null || activeTreasureEffects == null)
            return false;

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect == null || !effect.ProtectsFromEventEffect(eventEffect))
                continue;

            protectionName = effect.DisplayName;
            return true;
        }

        return false;
    }

    public bool HasRunFlag(string flagId)
    {
        return !string.IsNullOrWhiteSpace(flagId)
            && runFlags != null
            && runFlags.Contains(flagId);
    }

    public bool SetRunFlag(string flagId)
    {
        if (string.IsNullOrWhiteSpace(flagId))
            return false;

        EnsureRunCollections();

        if (runFlags.Contains(flagId))
            return false;

        runFlags.Add(flagId);
        return true;
    }

    public bool AddCrew(string crewName)
    {
        CrewAcquisitionResult result = TryRecruitCrew(crewName);
        if (result != CrewAcquisitionResult.Success)
            Debug.LogWarning(GetCrewAcquisitionMessage(result));
        return result == CrewAcquisitionResult.Success;
    }

    public bool AddCrew(CrewMember crewMember, bool allowDuplicate = false)
    {
        CrewAcquisitionResult result = TryRecruitCrew(crewMember, allowDuplicate);
        if (result != CrewAcquisitionResult.Success)
            Debug.LogWarning(GetCrewAcquisitionMessage(result, crewMember));
        return result == CrewAcquisitionResult.Success;
    }

    void AddCrewUnchecked(CrewMember crewMember)
    {
        activeCrew.Add(crewMember);
        SetLogForCrew(crewMember.crewName, true);
    }

    public int GetPurchasePrice(int basePrice)
    {
        int modifiedPrice = Mathf.Max(0, basePrice);

        foreach (CrewMember crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                modifiedPrice = crew.crewEffect.ModifyPurchaseCost(modifiedPrice);
        }

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect != null)
                modifiedPrice = effect.ModifyShopPrice(modifiedPrice);
        }

        return Mathf.Max(0, modifiedPrice);
    }

    public void RemoveCrew(string crewName)
    {
        if (crewDatabase == null)
        {
            Debug.LogError("Cannot remove crew because RunManager has no CrewDatabase assigned.");
            return;
        }

        CrewMember crewMember = crewDatabase.GetByName(crewName);
        if (crewMember != null && activeCrew.Contains(crewMember))
        {
            activeCrew.Remove(crewMember);
            SetLogForCrew(crewName, false);
        }
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

        if (!TrySpendScrap(cost))
        {
            Debug.Log("Not enough scrap.");
            return false;
        }

        roomInstance.Upgrade();
        Debug.Log($"{room.roomName} upgraded to level {roomInstance.level}.");
        return true;
    }

    public RoomInstance GetRoomInstance(Room room)
    {
        RoomInstance roomInstance = shipRooms.Find(r => r.roomData == room);
        return roomInstance;
    }

    public bool TryRepairRoom(
        Room room,
        int integrityToRestore,
        int scrapCost)
    {
        if (room == null)
        {
            Debug.LogWarning("Cannot repair a missing room.");
            return false;
        }

        if (integrityToRestore <= 0)
        {
            Debug.LogWarning("Room repair amount must be positive.");
            return false;
        }

        if (scrapCost < 0)
        {
            Debug.LogWarning("Room repair scrap cost cannot be negative.");
            return false;
        }

        RoomInstance roomInstance = GetRoomInstance(room);

        if (roomInstance == null || !roomInstance.unlocked)
        {
            Debug.LogWarning($"Cannot repair unavailable room {room.roomName}.");
            return false;
        }

        roomInstance.InitializeIntegrityIfNeeded();

        if (roomInstance.CurrentIntegrity >= roomInstance.MaximumIntegrity)
        {
            Debug.Log($"{room.roomName} is already at full integrity.");
            return false;
        }

        if (!TrySpendScrap(scrapCost))
        {
            Debug.Log("Not enough scrap to repair the room.");
            return false;
        }

        int repairedIntegrity = roomInstance.RepairIntegrity(
            integrityToRestore);

        Debug.Log(
            $"{room.roomName} repaired by {repairedIntegrity}. " +
            $"Integrity: {roomInstance.CurrentIntegrity}/" +
            $"{roomInstance.MaximumIntegrity}.");

        return repairedIntegrity > 0;
    }

    public RoomInstance GetRoomInstance<T>() where T : Room
    {
        return shipRooms.Find(r => r.unlocked && r.roomData is T);
    }

    public bool IsRoomOperational<T>() where T : Room
    {
        RoomInstance room = GetRoomInstance<T>();

        return room != null &&
               room.unlocked &&
               !room.IsDestroyed;
    }

    public bool WasRoomDisabledThisCombat<T>() where T : Room
    {
        RoomInstance room = GetRoomInstance<T>();

        return room != null && room.WasDisabledThisCombat;
    }

    public int GetRoomLevel<T>() where T : Room
    {
        return GetRoomInstance<T>().level;
    }

    public T GetRoomData<T>() where T : Room
    {
        return GetRoomInstance<T>().roomData as T;
    }

    public int GetJumpFuelCost()
    {
        HelmRoom helmRoom = GetRoomData<HelmRoom>();
        int cost = helmRoom.ModifyJumpFuelCost(fuelCostToJump, GetRoomLevel<HelmRoom>());

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                cost = crew.crewEffect.ModifyJumpFuelCost(cost);
        }

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect != null)
                cost = effect.ModifyJumpFuelCost(cost);
        }

        return Mathf.Max(0, cost);
    }

    public float ModifyCannonDamage(float damage)
    {
        int shotNumber = cannonShotsFiredThisCombat + 1;

        foreach (var crew in activeCrew)
        {
            if (crew != null && crew.crewEffect != null)
                damage = crew.crewEffect.ModifyCannonDamage(damage, shotNumber);
        }

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect != null)
                damage = effect.ModifyCannonDamage(damage, shotNumber);
        }

        return damage;
    }

    public void RecordCannonShot()
    {
        cannonShotsFiredThisCombat++;
    }

    public void ApplyPostCombatRoomEffects()
    {
        if (!WasRoomDisabledThisCombat<MechanicRoom>())
        {
            MechanicRoom mechanicRoom = GetRoomData<MechanicRoom>();
            int mechanicLevel = GetRoomLevel<MechanicRoom>();
            int repairAmount = mechanicRoom.GetPostCombatRepair(mechanicLevel);

            if (repairAmount > 0)
            {
                AddHealth(repairAmount);
            }
        }

        if (!WasRoomDisabledThisCombat<HelmRoom>())
        {
            HelmRoom helmRoom = GetRoomData<HelmRoom>();
            int helmLevel = GetRoomLevel<HelmRoom>();
            int fuelReward = helmRoom.GetPostCombatFuel(helmLevel);

            if (fuelReward > 0)
            {
                AddFuel(fuelReward);
            }
        }

        int crewHealing = 0;
        int crewScrap = 0;

        foreach (CrewMember crew in activeCrew)
        {
            if (crew == null || crew.crewEffect == null)
                continue;

            crewHealing += crew.crewEffect.GetPostCombatHealing();
            crewScrap += crew.crewEffect.GetPostCombatScrapReward();
        }

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect != null)
                crewHealing += effect.GetPostCombatHealing();
        }

        if (crewHealing > 0)
            AddHealth(crewHealing);

        if (crewScrap > 0)
        {
            AddScrapInternal(
                crewScrap,
                !WasRoomDisabledThisCombat<BunkRoom>());
        }
    }

    public void BeginCombatRoomEffects()
    {
        foreach (RoomInstance room in shipRooms)
        {
            room?.ResetCombatState();
        }

        mechanicEmergencyRepairUsed = false;
        shieldFirstHitUsed = false;
        cannonShotsFiredThisCombat = 0;
        usedTreasureFirstHitProtectionEffects.Clear();
    }

    public void GrantCombatRewards(int credits, int scrapReward, int fuelReward)
    {
        credits = Mathf.Max(0, credits);
        scrapReward = Mathf.Max(0, scrapReward);
        fuelReward = Mathf.Max(0, fuelReward);

        foreach (PersistentTreasureEffect effect in activeTreasureEffects)
        {
            if (effect == null)
                continue;

            credits = effect.ModifyCombatCreditsReward(credits);
            scrapReward = effect.ModifyCombatScrapReward(scrapReward);
            fuelReward = effect.ModifyCombatFuelReward(fuelReward);
        }

        bool bunkDisabled = WasRoomDisabledThisCombat<BunkRoom>();

        if (bunkDisabled)
        {
            credits = Mathf.FloorToInt(credits * 0.9f);
        }

        if (credits > 0)
        {
            AddMoneyInternal(credits, !bunkDisabled);
        }

        if (scrapReward > 0)
        {
            AddScrapInternal(scrapReward, !bunkDisabled);
        }

        if (fuelReward > 0)
        {
            AddFuel(fuelReward);
        }
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
        MusicManager.Instance.PlayVictoryMusic();
        GameObject.Find("UIManager").GetComponent<UIManager>().victoryPanelObj.SetActive(true);
    }

    public void LoseGame()
    {
        MusicManager.Instance.PlayDefeatMusic();
        GameObject.Find("UIManager").GetComponent<UIManager>().defeatPanelObj.SetActive(true);
        Reset();
    }

    public void Reset()
    {
        fuel = originalFuel;
        maxShipHealth = originalMaxHealth;
        currentShipHealth = maxShipHealth;
        money = originalMoney;
        scrap = originalScrap;
        fuelCostToJump = originalFuelCostToJump;
        level = originalLevel;

        for (int i = 0; i < shipRooms.Count; i++)
        {
            shipRooms[i].level = originalRoomLevels[i];
            shipRooms[i].ResetIntegrity();
            shipRooms[i].ResetCombatState();
        }

        EnsureRunCollections();
        activeCrew.Clear();
        runFlags.Clear();
        activeTreasureEffects.Clear();
        usedTreasureFirstHitProtectionEffects.Clear();
        canSeeCombatsBeforeStarting = false;
        nextFightHasOneHP = false;
        cannonShotsFiredThisCombat = 0;
    }

    private void EnsureRunCollections()
    {
        activeCrew ??= new List<CrewMember>();
        runFlags ??= new List<string>();
        activeTreasureEffects ??= new List<PersistentTreasureEffect>();
        shipRooms ??= new List<RoomInstance>();
    }
    
    void SetLogForResource(string resource, int amount)
    {
        if (GameManager.Instance.currentState == GameState.Navigation){
            GameObject.Find("ShowLog").GetComponent<ShowLog>().ConstructLogEntryForResource(resource, amount);
            GameObject.Find("ShowLog").GetComponent<ShowLog>().ShowTheLogWithSetTime(true);
        }
    }

    void SetLogForCrew(string crew, bool gained)
    {
        if (GameManager.Instance.currentState == GameState.Navigation){
            GameObject.Find("ShowLog").GetComponent<ShowLog>().ConstructLogEntryForCrew(crew, gained);
            GameObject.Find("ShowLog").GetComponent<ShowLog>().ShowTheLogWithSetTime(true);
        }
    }

    public BoardingEncounterDefinition getRandomBoardingEncounter()
    {
        if (enemyFaction == null ||
            enemyFaction.BoardingEncounterDefinitions == null ||
            enemyFaction.BoardingEncounterDefinitions.Count == 0)
        {
            Debug.LogWarning("No boarding encounters are configured. Go configure some.");
            return null;
        }

        int randomIndex = UnityEngine.Random.Range(0,enemyFaction.BoardingEncounterDefinitions.Count);

        return enemyFaction.BoardingEncounterDefinitions[randomIndex];
    }
}
