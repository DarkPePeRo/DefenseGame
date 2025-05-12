using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public List<TextMeshProUGUI> goldTexts;
    public List<TextMeshProUGUI> diamondTexts;

    private int currentGold;    // ȭ�鿡 ǥ�õǴ� ���� �ݾ�
    private int targetGold;     // ��ǥ �ݾ�
    private int currentDiamond;
    private int targetDiamond;

    private Coroutine goldCoroutine;
    private Coroutine diamondCoroutine;

    void Start()
    {
        currentGold = PlayerCurrency.Instance.gold.amount;
        targetGold = currentGold;
        currentDiamond = PlayerCurrency.Instance.diamond.amount;
        targetDiamond = currentDiamond;

        UpdateGoldText();
        UpdateDiamondText();
    }

    void Update()
    {
        // ��ǥ �ݾ��� ����Ǿ����� Ȯ���ϰ�, ���� �� ��ǥ�� ������Ʈ
        if (targetGold != PlayerCurrency.Instance.gold.amount)
        {
            targetGold = PlayerCurrency.Instance.gold.amount;
            if (goldCoroutine == null) // ���� ���� �ڷ�ƾ�� ������ ���� ����
            {
                goldCoroutine = StartCoroutine(AnimateGold());
            }
        }

        if (targetDiamond != PlayerCurrency.Instance.diamond.amount)
        {
            targetDiamond = PlayerCurrency.Instance.diamond.amount;
            if (diamondCoroutine == null)
            {
                diamondCoroutine = StartCoroutine(AnimateDiamond());
            }
        }
    }

    private IEnumerator AnimateGold()
    {
        while (currentGold != targetGold) // ��ǥ �ݾ׿� ������ ������ �ݺ�
        {
            float duration = 0.2f;
            float elapsed = 0f;
            int startGold = currentGold;

            // ��ǥ �ݾ��� ����� �� �����Ƿ� ��ǥ�� ������ ������ ������ ����
            while (elapsed < duration && currentGold != targetGold)
            {
                elapsed += Time.deltaTime;
                currentGold = (int)Mathf.Lerp(startGold, targetGold, elapsed / duration);
                UpdateGoldText();
                yield return null;
            }

            currentGold = targetGold; // ��ǥ �ݾ� ���� �� ���� ������Ʈ
            UpdateGoldText();
        }
        goldCoroutine = null; // �ڷ�ƾ �Ϸ� �� null�� ����
    }

    private IEnumerator AnimateDiamond()
    {
        while (currentDiamond != targetDiamond)
        {
            float duration = 0.2f;
            float elapsed = 0f;
            int startDiamond = currentDiamond;

            while (elapsed < duration && currentDiamond != targetDiamond)
            {
                elapsed += Time.deltaTime;
                currentDiamond = (int)Mathf.Lerp(startDiamond, targetDiamond, elapsed / duration);
                UpdateDiamondText();
                yield return null;
            }

            currentDiamond = targetDiamond;
            UpdateDiamondText();
        }
        diamondCoroutine = null;
    }
    private void UpdateGoldText()
    {
        string formatted = FormatCurrency(currentGold);
        foreach (var text in goldTexts)
        {
            text.text = formatted;
        }
    }

    private void UpdateDiamondText()
    {
        string formatted = FormatCurrency(currentDiamond);
        foreach (var text in diamondTexts)
        {
            text.text = formatted;
        }
    }


    public string FormatCurrency(int amount)
    {
        if (amount >= 1_000_000_000)
        {
            return (amount / 1_000_000_000f).ToString("0.##") + "B";
        }
        else if (amount >= 1_000_000)
        {
            return (amount / 1_000_000f).ToString("0.##") + "M";
        }
        else if (amount >= 1_000)
        {
            return (amount / 1_000f).ToString("0.##") + "K";
        }
        else
        {
            return amount.ToString();
        }
    }
}
