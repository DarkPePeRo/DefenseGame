using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageUIManager : MonoBehaviour
{
    [SerializeField] public GameObject damageTextPrefab; // 데미지 텍스트 프리팹
    [SerializeField] private float randomX = 1f;
    [SerializeField] private float minY = 0.5f;
    [SerializeField] private float maxY = 1f;

    public void ShowDamageText(Vector3 position, float damageAmount)
    {
        // 데미지 텍스트 인스턴스 생성 (Canvas의 자식으로)
        GameObject damageTextInstance = Instantiate(damageTextPrefab);
        Vector3 randomOffset = new Vector3(
        Random.Range(-randomX, randomX),
        Random.Range(minY, maxY),
        0f
        );
        damageTextInstance.transform.position = position + randomOffset;

        // 텍스트 설정
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (damageText != null)
        {
            damageText.text = FormatDamage(damageAmount);
        }

        // 데미지 텍스트를 카메라를 향하게 회전
        damageTextInstance.transform.LookAt(Camera.main.transform);
        damageTextInstance.transform.Rotate(0, 180, 0); // 텍스트가 뒤집히지 않도록 회전

        // 애니메이션 코루틴 시작
        StartCoroutine(DamageTextAnimation(damageTextInstance));
    }

    private IEnumerator DamageTextAnimation(GameObject damageTextInstance)
    {
        Vector3 startPosition = damageTextInstance.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, 0.4f, 0);

        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // 가속 (위로 갈수록 빨라짐)`
            float easeT = t * t;

            damageTextInstance.transform.position =
                Vector3.Lerp(startPosition, endPosition, easeT);

            yield return null;
        }

        Destroy(damageTextInstance);
    }
    private string FormatDamage(float damage)
    {
        long value = (long)damage;

        long eok = value / 100_000_000;                     // 억
        long man = (value % 100_000_000) / 10_000;          // 만
        long rest = value % 10_000;                         // 나머지

        // 1. 억 단위
        if (eok > 0)
        {
            if (man > 0)
                return $"{eok}억{man}만";
            else
                return $"{eok}억";
        }

        // 2. 만 단위
        if (man > 0)
        {
            if (rest > 0)
                return $"{man}만{rest}";
            else
                return $"{man}만";
        }

        // 3. 천 단위 이하
        if (value >= 1000)
            return value.ToString("N0");

        return value.ToString();
    }
}
