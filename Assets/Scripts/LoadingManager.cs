using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private Slider progressBar; // 로딩 바
    [SerializeField] private TextMeshProUGUI progressText;  // 로딩 텍스트
    [SerializeField] private TextMeshProUGUI EndText;  // 로딩 텍스트

    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // 비동기적으로 다음 씬 로드 시작
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        // 로딩이 끝날 때까지 로딩 화면 표시
        operation.allowSceneActivation = false; // 로딩이 완료되기 전까지 자동 전환 방지

        while (!operation.isDone)
        {
            // 로딩 진행률 업데이트
            float progress = Mathf.Clamp01(operation.progress / 0.9f); // 90%까지 로딩 완료
            progressBar.value = progress;
            progressText.text = (progress * 100).ToString("F0") + "%";

            // 로딩이 90% 이상 완료되었을 때 전환
            if (operation.progress >= 0.9f)
            {
                EndText.text = "Press anywhere to Start Game";
                progressText.text = "Loading Success"; // 텍스트 변경
                if (Input.anyKeyDown)
                {
                    operation.allowSceneActivation = true; // 씬 전환 허용
                }
            }

            yield return null;
        }
    }
}
