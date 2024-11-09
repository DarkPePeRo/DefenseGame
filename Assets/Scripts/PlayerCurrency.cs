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
    public Currency gold;
    public Currency diamond;

    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondText;

    public PlayFabLogin playFab;

    public float timer;

    private void Start()
    {
        gold = new Currency { name = "Gold", amount = 0 };
        diamond = new Currency { name = "Diamond", amount = 0 };
    }

    private void Update()
    {
        goldText.text = gold.amount.ToString() + "G";
        diamondText.text = diamond.amount.ToString() + "G";
        timer += Time.deltaTime;
        if (timer > 10)
        {
            playFab.SavePlayerData("gold", gold.amount);
            playFab.SavePlayerData("diamond", diamond.amount);
            timer = 0;
        }
    }


    public void AddCurrency(Currency currency, int amount)
    {
        currency.amount += amount;
        Debug.Log($"{currency.name} added: {amount}. New total: {currency.amount}");
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
