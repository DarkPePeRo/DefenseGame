using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar; // �ε� ��
    [SerializeField] private TextMeshProUGUI progressText;  // �ε� �ؽ�Ʈ
    [SerializeField] private TextMeshProUGUI EndText;  // �ε� �ؽ�Ʈ

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // �񵿱������� ���� �� �ε� ����
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // �ε��� ���� ������ �ε� ȭ�� ǥ��
        operation.allowSceneActivation = false; // �ε��� �Ϸ�Ǳ� ������ �ڵ� ��ȯ ����

        while (!operation.isDone)
        {
            // �ε� ����� ������Ʈ
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // 90%���� �ε� �Ϸ�
            progressBar.value = progress;
            progressText.text = (progress * 100).ToString("F0") + "%";

            // �ε��� 90% �̻� �Ϸ�Ǿ��� �� ��ȯ
            if (operation.progress >= 0.9f)
            {
                EndText.text = "Press anywhere to Start Game";
                progressText.text = "Loading Success"; // �ؽ�Ʈ ����
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true; // �� ��ȯ ���
                }
            }

            yield return null;
        }
    }
}
