using UnityEngine;
using UnityEngine.EventSystems;

public class StatUpgradeButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public string statType;
    private float pressTime;
    private bool isHolding;

    private const float holdThreshold = 0.3f;

    public void OnPointerDown(PointerEventData eventData)
    {
        pressTime = Time.time;
        isHolding = true;
        StartCoroutine(PressChecker());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;

        if (Time.time - pressTime < holdThreshold)
        {
            StatUpgradeUI.Instance.statsManager.RequestLevelUp(statType); // 단일 레벨업
        }
        else
        {
            StatUpgradeUI.Instance.statsManager.StopStatLevelUpLoop(); // 반복 중지
        }
    }

    private System.Collections.IEnumerator PressChecker()
    {
        yield return new WaitForSeconds(holdThreshold);
        if (isHolding)
        {
            StatUpgradeUI.Instance.statsManager.StartStatLevelUpLoop(statType); // 연속 시작
        }
    }
}
