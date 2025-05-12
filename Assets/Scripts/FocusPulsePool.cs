using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class FocusPulsePool : MonoBehaviour
{
    public static FocusPulsePool Instance;
    public GameObject pulseEffectPrefab;
    public RectTransform canvasRectTransform; // ← Canvas RectTransform 직접 연결
    public Camera uiCamera; // ← RenderMode가 Camera일 경우 필수

    private Queue<FocusPulseEffect> pool = new Queue<FocusPulseEffect>();

    void Awake()
    {
        Instance = this;
    }

    public void Request(Vector2 screenPos)
    {
        FocusPulseEffect effect = GetFromPool();
        RectTransform rt = effect.GetComponent<RectTransform>();
        rt.SetParent(canvasRectTransform, false);

        Vector3 worldPos;
        bool result = RectTransformUtility.ScreenPointToWorldPointInRectangle(
            canvasRectTransform,
            screenPos,
            uiCamera,
            out worldPos);

        if (result)
            rt.position = worldPos;

        effect.gameObject.SetActive(true);
        effect.Play(ReleaseToPool);
    }

    private FocusPulseEffect GetFromPool()
    {
        if (pool.Count > 0)
            return pool.Dequeue();

        return Instantiate(pulseEffectPrefab).GetComponent<FocusPulseEffect>();
    }

    private void ReleaseToPool(FocusPulseEffect effect)
    {
        effect.gameObject.SetActive(false);
        pool.Enqueue(effect);
    }
}
