using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DamageUIManager : MonoBehaviour
{
    [SerializeField] public GameObject damageTextPrefab; // ������ �ؽ�Ʈ ������

    public void ShowDamageText(Vector3 position, float damageAmount)
    {
        // ������ �ؽ�Ʈ �ν��Ͻ� ���� (Canvas�� �ڽ�����)
        GameObject damageTextInstance = Instantiate(damageTextPrefab);
        damageTextInstance.transform.position = position + new Vector3(0, 0.7f, 0); // ��ġ ����

        // �ؽ�Ʈ ����
        TextMeshProUGUI damageText = damageTextInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (damageText != null)
        {
            damageText.text = damageAmount.ToString();
        }

        // ������ �ؽ�Ʈ�� ī�޶� ���ϰ� ȸ��
        damageTextInstance.transform.LookAt(Camera.main.transform);
        damageTextInstance.transform.Rotate(0, 180, 0); // �ؽ�Ʈ�� �������� �ʵ��� ȸ��

        // �ִϸ��̼� �ڷ�ƾ ����
        StartCoroutine(DamageTextAnimation(damageTextInstance));
    }

    private IEnumerator DamageTextAnimation(GameObject damageTextInstance)
    {
        Vector3 startPosition = damageTextInstance.transform.position;
        Vector3 endPosition = startPosition + new Vector3(0, 0.4f, 0); // ���� �̵�
        float duration = 0.2f; // �ִϸ��̼� �ð�
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            damageTextInstance.transform.position = Vector3.Lerp(startPosition, endPosition, t);
            yield return null;
        }

        Destroy(damageTextInstance); // �ִϸ��̼� ���� �� �ؽ�Ʈ ����
    }
}
