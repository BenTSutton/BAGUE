using UnityEngine;

public class RunManager : MonoBehaviour
{
    public static RunManager Instance;

    public int fuel;
    public int currentShipHealth;
    public int maxShipHealth;
    public int money;

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
        fuel += toAdd;
    }

    public void AddHealth(int toAdd)
    {
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
        money += toAdd;
    }
}
