using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using TMPro;
using GooglePlayGames.BasicApi;

[System.Serializable]
public class Currency
{
    public string name;
    public int amount;
}

public class PlayerCurrency : MonoBehaviour
{
    public static PlayerCurrency Instance;
    public Currency gold;
    public Currency diamond;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondText;


    public float timer;

    private void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 오브젝트 유지
        }
        else
        {
            Destroy(gameObject); // 중복된 오브젝트는 제거
        }
    }

    private void Start()
    {
        gold = new Currency { name = "gold", amount = 0 };
        diamond = new Currency { name = "diamond", amount = 0 };
    }

    private void Update()
    {
    }


    public void AddCurrency(Currency gold, int amountGold, Currency diamond, int amountDiamond)
    {
        gold.amount += amountGold;
        diamond.amount += amountDiamond;
        Debug.Log($"{gold.name} added: {amountGold}. New total: {gold.amount}");
        Debug.Log($"{diamond.name} added: {amountDiamond}. New total: {diamond.amount}");
        Debug.Log("Saved");
    }

    public bool SpendCurrency(Currency currency, int amount)
    {
        if (currency.amount >= amount)
        {
            currency.amount -= amount;
            Debug.Log($"{currency.name} spent: {amount}. New total: {currency.amount}");
            return true;
        }
        else
        {
            Debug.Log("Not enough " + currency.name);
            return false;
        }
    }

}
