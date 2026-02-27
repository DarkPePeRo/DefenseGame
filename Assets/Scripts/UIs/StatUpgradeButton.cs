using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class StatUpgradeButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerClickHandler
{
    public string statType;

    private bool isDown;
    private bool longPressTriggered;

    private Coroutine holdCoroutine;
    private int pressToken = 0;

    private const float holdThreshold = 0.3f;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDown = true;
        longPressTriggered = false;

        pressToken++;
        int myToken = pressToken;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        holdCoroutine = StartCoroutine(HoldChecker(myToken));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDown = false;

        pressToken++;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }

        if (longPressTriggered)
        {
            StatUpgradeUI.Instance.statsManager.StopStatLevelUpLoop();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (longPressTriggered) return;

        StatUpgradeUI.Instance.statsManager.RequestLevelUp(statType);
    }

    private IEnumerator HoldChecker(int myToken)
    {
        yield return new WaitForSeconds(holdThreshold);

        if (myToken != pressToken) yield break;

        if (!isDown) yield break;

        longPressTriggered = true;
        StatUpgradeUI.Instance.statsManager.StartStatLevelUpLoop(statType);
    }
}
