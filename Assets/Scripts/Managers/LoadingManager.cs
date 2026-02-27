using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string nextSceneName = "SampleScene";

    [Header("UI")]
    [SerializeField] private Slider progressBar;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private TMP_Text endText;
    [SerializeField] private TMP_Text errorText;

    [Header("Settings")]
    [SerializeField] private float stepTimeoutSeconds = 12f;
    [SerializeField] private int retryCount = 2;
    [SerializeField] private bool requireInputToStart = true;

    private float visualProgress = 0f;
    private float targetProgress = 0f;

    private bool loadFailed = false;
    private bool isLoading = false;
    private string failReason = "";

    private void Start()
    {
        if (endText != null) endText.text = "";
        if (errorText != null) errorText.text = "";
        ApplyProgressUI(0f);
    }

    private void Update()
    {
        visualProgress = Mathf.MoveTowards(visualProgress, targetProgress, Time.deltaTime * 0.8f);
        ApplyProgressUI(visualProgress);
    }

    public void StartLoading()
    {
        if (isLoading) return;

        if (!PlayFab.PlayFabClientAPI.IsClientLoggedIn())
        {
            ShowError("PlayFab 로그인 상태가 아닙니다.");
            return;
        }

        isLoading = true;
        loadFailed = false;
        visualProgress = 0f;
        targetProgress = 0f;

        if (endText != null) endText.text = "";
        if (errorText != null) errorText.text = "";

        StartCoroutine(LoadFlow());
    }
    private IEnumerator LoadFlow()
    {
        if (SignalRClient.Instance == null)
            Debug.LogWarning("[Loading] SignalRClient 없음");

        yield return RunStep("Currency", 0.33f, LoadCurrencyStep);
        if (loadFailed) yield break;

        yield return RunStep("Stage", 0.66f, LoadStageStep);
        if (loadFailed) yield break;

        yield return RunStep("GodStat", 0.90f, LoadGodStatStep);
        if (loadFailed) yield break;

        targetProgress = 1f;
        ApplyProgressUI(1f);

        if (endText != null)
            endText.text = requireInputToStart ? "Press anywhere to Start Game" : "Starting...";

        if (requireInputToStart)
        {
            while (!Input.anyKeyDown)
                yield return null;
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
        }

        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator RunStep(string stepName, float progressAfterStep, Func<Action<bool, string>, IEnumerator> stepFactory)
    {
        for (int attempt = 0; attempt <= retryCount; attempt++)
        {
            bool done = false;
            bool ok = false;
            string reason = "";

            Action<bool, string> complete = (success, msg) =>
            {
                ok = success;
                reason = msg ?? "";
                done = true;
            };

            yield return StartCoroutine(stepFactory(complete));

            float timeout = stepTimeoutSeconds;
            while (!done && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            if (!done)
            {
                ok = false;
                reason = $"timeout({stepTimeoutSeconds}s)";
            }

            if (ok)
            {
                if (errorText != null) errorText.text = "";
                targetProgress = progressAfterStep;
                yield break;
            }

            if (attempt < retryCount)
            {
                Debug.LogWarning($"[Loading] {stepName} 실패({reason}) 재시도 {attempt + 1}/{retryCount}");
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            loadFailed = true;
            failReason = $"{stepName} 최종 실패: {reason}";
            ShowError(failReason);
            if (endText != null) endText.text = "";
            yield break;
        }
    }

    private IEnumerator LoadCurrencyStep(Action<bool, string> complete)
    {
        bool called = false;

        PlayFabCurrencyService.Load(() =>
        {
            called = true;
            complete?.Invoke(true, "");
        });

        while (!called)
            yield return null;
    }

    private IEnumerator LoadStageStep(Action<bool, string> complete)
    {
        bool called = false;

        PlayFabStageService.Load((clearedStages, highestStage) =>
        {
            called = true;
            complete?.Invoke(true, "");
        });

        while (!called)
            yield return null;
    }

    private IEnumerator LoadGodStatStep(Action<bool, string> complete)
    {
        bool called = false;

        PlayFabGodStatService.Load((data, createdNew) =>
        {
            if (createdNew)
                PlayFabGodStatService.Save(data);

            called = true;
            complete?.Invoke(true, "");
        });

        while (!called)
            yield return null;
    }

    private void ApplyProgressUI(float p)
    {
        if (progressBar != null) progressBar.value = p;
        if (progressText != null) progressText.text = (p * 100f).ToString("F0") + "%";
    }

    private void ShowError(string msg)
    {
        Debug.LogError("[Loading] " + msg);
        if (errorText != null) errorText.text = msg;
    }
}
