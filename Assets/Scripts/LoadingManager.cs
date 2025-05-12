using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingManager : MonoBehaviour
{

    public static LoadingManager Instance;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI endText;

    private bool isDataLoaded = false;

    private void Start()
    {
        PlayFabAuthService1.Instance.OnLoginSuccess += () =>
        {
            StartCoroutine(LoadDataThenScene());
        };
    }

    private IEnumerator LoadDataThenScene()
    {
        float fakeProgress = 0f;

        // ������ ����
        PlayFabChatService.Instance.Connect(PlayFabAuthService1.Instance.PlayFabId);
        // ������ ���� �ε�
        PlayFabCurrencyService.Load(() =>
        {
            PlayFabStageService.Load((clearedStages, highestStage) =>
            {
                PlayFabStatsService.Load(() =>
                {
                    isDataLoaded = true;
                });
            });
        });

        while (!isDataLoaded)
        {
            // �ε� UI ���� ȿ��
            fakeProgress = Mathf.MoveTowards(fakeProgress, 0.9f, Time.deltaTime);
            progressBar.value = fakeProgress;
            progressText.text = (fakeProgress * 100).ToString("F0") + "%";
            yield return null;
        }

        // �Ϸ� UI ǥ��
        progressBar.value = 1f;
        progressText.text = "100%";
        endText.text = "Press anywhere to Start Game";

        // Ű �Է� ���
        while (!Input.anyKeyDown)
        {
            yield return null;
        }

        SceneManager.LoadScene("SampleScene");
    }
}
