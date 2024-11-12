using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI diamondText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        goldText.text = PlayerCurrency.Instance.gold.amount.ToString();
        diamondText.text = PlayerCurrency.Instance.diamond.amount.ToString();
    }
}
