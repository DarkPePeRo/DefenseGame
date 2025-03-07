using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageUIManager : MonoBehaviour
{
    [SerializeField] public GameObject damageTextPrefab; // 데미지 텍스트 프리팹

    public void ShowDamageText(Vector3 position, float damageAmount)
    {
        // 데미지 텍스트 인스턴스 생성 (Canvas의 자식으로)
        GameObject damageTextInstance = Instantiate(damageTextPrefab);
        damageTextInstance.transform.position = position + new Vector3(0, 0.7f, 0); // 위치 조정

        // 텍스트 설정
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (damageText != null)
        {
            damageText.text = damageAmount.ToString();
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
        Vector3 endPosition = startPosition + new Vector3(0, 0.4f, 0); // 위로 이동
        float duration = 0.2f; // 애니메이션 시간
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            damageTextInstance.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        Destroy(damageTextInstance); // 애니메이션 종료 후 텍스트 제거
    }
}
