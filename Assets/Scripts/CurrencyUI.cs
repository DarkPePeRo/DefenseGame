using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    public List<TextMeshProUGUI> goldTexts;
    public List<TextMeshProUGUI> diamondTexts;

    private int currentGold;    // 화면에 표시되는 현재 금액
    private int targetGold;     // 목표 금액
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
        // 목표 금액이 변경되었는지 확인하고, 변경 시 목표를 업데이트
        if (targetGold != PlayerCurrency.Instance.gold.amount)
        {
            targetGold = PlayerCurrency.Instance.gold.amount;
            if (goldCoroutine == null) // 실행 중인 코루틴이 없으면 새로 시작
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
        while (currentGold != targetGold) // 목표 금액에 도달할 때까지 반복
        {
            float duration = 0.2f;
            float elapsed = 0f;
            int startGold = currentGold;

            // 목표 금액이 변경될 수 있으므로 목표에 도달할 때까지 점진적 증가
            while (elapsed < duration && currentGold != targetGold)
            {
                elapsed += Time.deltaTime;
                currentGold = (int)Mathf.Lerp(startGold, targetGold, elapsed / duration);
                UpdateGoldText();
                yield return null;
            }

            currentGold = targetGold; // 목표 금액 도달 시 최종 업데이트
            UpdateGoldText();
        }
        goldCoroutine = null; // 코루틴 완료 후 null로 설정
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
