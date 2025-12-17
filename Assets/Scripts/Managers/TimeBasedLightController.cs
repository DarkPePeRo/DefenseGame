using System.Drawing;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DayNightCycle2D : MonoBehaviour
{
    [Header("시간 설정")]
    [Range(0f, 24f)] public float currentTime = 6f;  // 시작 시간
    public float dayDurationInMinutes = 10f;

    [Header("태양 설정")]
    public Light2D sunLight;
    public AnimationCurve sunIntensityCurve;
    public Light2D sunLight2;
    public Transform sunTransform;
    public Vector3 sunStartPos;
    public Vector3 sunEndPos;

    [Header("달 설정")]
    public Light2D moonLight;
    public Transform moonTransform;
    public Vector3 moonStartPos;
    public Vector3 moonEndPos;
    public AnimationCurve moonIntensityCurve;

    [Header("빛 설정")]
    public Light2D horong1;
    public Light2D horong2;
    public Light2D horong3;

    void Update()
    {
        float timeScale = 24f / (dayDurationInMinutes * 60f);
        currentTime += Time.deltaTime * timeScale;
        if (currentTime > 24f) currentTime -= 24f;

        UpdateSun();
        UpdateMoon();
    }

    void UpdateSun()
    {
        // 06:00 ~ 18:00 사이만 적용
        float t = Mathf.InverseLerp(8f, 16f, currentTime);
        bool active = currentTime >= 8f && currentTime < 16f;

        sunLight2.gameObject.SetActive(active);

        if (active)
        {
            sunTransform.position = Vector3.Lerp(sunStartPos, sunEndPos, t);
            sunLight.intensity = sunIntensityCurve.Evaluate(t);
        }
    }

    void UpdateMoon()
    {
        // 18:00 ~ 다음날 06:00
        float t;

        if (currentTime >= 18f)
            t = Mathf.InverseLerp(18f, 24f, currentTime); // 20~24
        else
            t = Mathf.InverseLerp(0f, 6f, currentTime) + 1f; // 0~4 → 1~2로 이어지게 처리

        bool active = currentTime >= 18f || currentTime < 6f;

        moonLight.gameObject.SetActive(active);
        horong1.gameObject.SetActive(active);
        horong2.gameObject.SetActive(active);
        horong3.gameObject.SetActive(active);

        if (active)
        {
            float moonT = Mathf.InverseLerp(0f, 2f, t); // 0~2를 다시 0~1로 정규화
            moonTransform.position = Vector3.Lerp(moonStartPos, moonEndPos, moonT);
            moonLight.intensity = moonIntensityCurve.Evaluate(moonT);
        }
    }
}
